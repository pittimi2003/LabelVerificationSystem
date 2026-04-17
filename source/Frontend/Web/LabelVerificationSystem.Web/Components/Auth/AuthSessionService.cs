using Microsoft.AspNetCore.Components;
using System.Net;

namespace LabelVerificationSystem.Web.Components.Auth;

public sealed class AuthSessionService
{
    private static readonly TimeSpan ProactiveRefreshWindow = TimeSpan.FromMinutes(3);

    private readonly AuthApiClient _authApiClient;
    private readonly AuthSessionStorage _storage;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<AuthSessionService> _logger;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);

    private AuthSessionSnapshot _current = AuthSessionSnapshot.Anonymous();
    private Task<bool>? _inFlightRefreshTask;
    private CancellationTokenSource? _refreshLoopCts;
    private Task? _initializeTask;
    private bool _initialized;

    public AuthSessionService(
        AuthApiClient authApiClient,
        AuthSessionStorage storage,
        NavigationManager navigationManager,
        ILogger<AuthSessionService> logger)
    {
        _authApiClient = authApiClient;
        _storage = storage;
        _navigationManager = navigationManager;
        _logger = logger;
    }

    public AuthSessionSnapshot Current => _current;
    public bool IsInitialized => _initialized;
    public event Action? SessionChanged;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _initializeTask ??= InitializeCoreAsync(cancellationToken);
        return _initializeTask;
    }

    private async Task InitializeCoreAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            var stored = await _storage.LoadAsync();
            if (stored is not null)
            {
                _current = stored;
            }

            if (_current.IsUserMode && !string.IsNullOrWhiteSpace(_current.AccessToken))
            {
                var refreshed = await EnsureValidAccessTokenAsync(cancellationToken);
                if (refreshed)
                {
                    await LoadCurrentUserAsync(cancellationToken);
                }
                else
                {
                    await ClearSessionInternalAsync();
                }
            }
            else
            {
                await TryHydrateBypassAsync(cancellationToken);
            }

            StartRefreshLoop();
        }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "No se pudo restaurar la sesión al iniciar la app. Se continuará como anónimo.");
            await ClearSessionInternalAsync();
        }
        finally
        {
            _initialized = true;
            NotifyChanged();
        }
    }

    public async Task LoginAsync(string usernameOrEmail, string password, bool rememberMe, CancellationToken cancellationToken = default)
    {
        var response = await _authApiClient.LoginAsync(
            new AuthLoginRequestDto(
                usernameOrEmail,
                password,
                rememberMe,
                new AuthClientInfoDto("BlazorWasm", null)),
            cancellationToken);

        _current = new AuthSessionSnapshot
        {
            IsAuthenticated = true,
            AuthenticationMode = "User",
            AccessToken = response.AccessToken,
            AccessTokenExpiresAtUtc = response.ExpiresAtUtc,
            RefreshToken = response.RefreshToken,
            RefreshTokenExpiresAtUtc = response.RefreshExpiresAtUtc,
            User = response.User
        };

        await LoadCurrentUserAsync(cancellationToken);
        await _storage.SaveAsync(_current);
        StartRefreshLoop();
        NotifyChanged();
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _authApiClient.LogoutAsync(_current.AccessToken, _current.RefreshToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo completar logout remoto, se limpiará sesión local.");
        }

        await ClearSessionInternalAsync();
        _navigationManager.NavigateTo("/signin", forceLoad: false);
    }

    public async Task<bool> EnsureValidAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!_current.IsUserMode)
        {
            return _current.IsAuthenticated;
        }

        if (string.IsNullOrWhiteSpace(_current.AccessToken) || _current.AccessTokenExpiresAtUtc is null)
        {
            return false;
        }

        if (!NeedsRefresh())
        {
            return true;
        }

        return await RefreshSingleFlightAsync(cancellationToken);
    }

    public async Task HandleAuthorizationFailureAsync(HttpStatusCode statusCode, CancellationToken cancellationToken = default)
    {
        if (statusCode == HttpStatusCode.Forbidden)
        {
            _navigationManager.NavigateTo("/error401", forceLoad: false);
            return;
        }

        await ClearSessionInternalAsync();
        _navigationManager.NavigateTo("/signin", forceLoad: false);
    }

    private async Task LoadCurrentUserAsync(CancellationToken cancellationToken)
    {
        var me = await _authApiClient.GetMeAsync(_current.AccessToken, cancellationToken);
        if (me is null)
        {
            throw new HttpRequestException("No fue posible validar sesión actual.", null, HttpStatusCode.Unauthorized);
        }

        _current.IsAuthenticated = me.IsAuthenticated;
        _current.AuthenticationMode = me.AuthenticationMode;
        _current.User = me.User;

        if (me.Session is not null)
        {
            _current.AccessTokenExpiresAtUtc = me.Session.ExpiresAtUtc;
        }
    }

    private async Task TryHydrateBypassAsync(CancellationToken cancellationToken)
    {
        var me = await _authApiClient.GetMeAsync(null, cancellationToken);
        if (me is null || !me.IsAuthenticated || !string.Equals(me.AuthenticationMode, "Bypass", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _current = new AuthSessionSnapshot
        {
            IsAuthenticated = true,
            AuthenticationMode = me.AuthenticationMode,
            User = me.User
        };

        await _storage.SaveAsync(_current);
    }

    private async Task<bool> RefreshSingleFlightAsync(CancellationToken cancellationToken)
    {
        await _refreshGate.WaitAsync(cancellationToken);
        try
        {
            if (_inFlightRefreshTask is null || _inFlightRefreshTask.IsCompleted)
            {
                _inFlightRefreshTask = ExecuteRefreshAsync(cancellationToken);
            }
        }
        finally
        {
            _refreshGate.Release();
        }

        return await _inFlightRefreshTask;
    }

    private async Task<bool> ExecuteRefreshAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_current.RefreshToken) || _current.RefreshTokenExpiresAtUtc is null)
        {
            await ClearSessionInternalAsync();
            return false;
        }

        if (_current.RefreshTokenExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            await ClearSessionInternalAsync();
            return false;
        }

        try
        {
            var response = await _authApiClient.RefreshAsync(_current.RefreshToken, cancellationToken);
            _current.AccessToken = response.AccessToken;
            _current.AccessTokenExpiresAtUtc = response.ExpiresAtUtc;
            _current.RefreshToken = response.RefreshToken;
            _current.RefreshTokenExpiresAtUtc = response.RefreshExpiresAtUtc;
            _current.User = response.User;
            _current.IsAuthenticated = true;
            _current.AuthenticationMode = "User";

            await _storage.SaveAsync(_current);
            NotifyChanged();
            return true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Conflict)
        {
            _logger.LogWarning(ex, "Refresh inválido/reutilizado. Se limpiará la sesión.");
            await ClearSessionInternalAsync();
            return false;
        }
    }

    private bool NeedsRefresh()
    {
        if (_current.AccessTokenExpiresAtUtc is null)
        {
            return true;
        }

        var timeToExpire = _current.AccessTokenExpiresAtUtc.Value - DateTime.UtcNow;
        return timeToExpire <= ProactiveRefreshWindow;
    }

    private void StartRefreshLoop()
    {
        _refreshLoopCts?.Cancel();
        _refreshLoopCts?.Dispose();

        if (!_current.IsUserMode || !_current.IsAuthenticated)
        {
            _refreshLoopCts = null;
            return;
        }

        _refreshLoopCts = new CancellationTokenSource();
        _ = RunRefreshLoopAsync(_refreshLoopCts.Token);
    }

    private async Task RunRefreshLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var delay = ComputeDelayUntilProactiveRefresh();

            try
            {
                await Task.Delay(delay, cancellationToken);
                var refreshed = await EnsureValidAccessTokenAsync(cancellationToken);
                if (!refreshed)
                {
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error inesperado en refresh proactivo.");
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }
        }
    }

    private TimeSpan ComputeDelayUntilProactiveRefresh()
    {
        if (_current.AccessTokenExpiresAtUtc is null)
        {
            return TimeSpan.FromSeconds(30);
        }

        var delay = _current.AccessTokenExpiresAtUtc.Value - DateTime.UtcNow - ProactiveRefreshWindow;
        if (delay <= TimeSpan.Zero)
        {
            return TimeSpan.FromSeconds(1);
        }

        return delay;
    }

    private async Task ClearSessionInternalAsync()
    {
        _refreshLoopCts?.Cancel();
        _refreshLoopCts?.Dispose();
        _refreshLoopCts = null;

        _current = AuthSessionSnapshot.Anonymous();
        await _storage.ClearAsync();
        NotifyChanged();
    }

    private void NotifyChanged() => SessionChanged?.Invoke();
}
