using Microsoft.JSInterop;
using System.Text.Json;

public class SessionService
{
    private const string AppStateKey = "AppState";
    private const string InitialAppStateKey = "InitalAppState";
    private readonly IJSRuntime _jsRuntime;

    public SessionService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetAppStateToSession(AppState state)
    {
        var jsonState = JsonSerializer.Serialize(state);
        await _jsRuntime.InvokeVoidAsync("browserStorage.setSessionItem", AppStateKey, jsonState);
    }

    public async Task DeleteAppStateFromSession()
    {
        await _jsRuntime.InvokeVoidAsync("browserStorage.removeSessionItem", AppStateKey);
    }

    public async Task<AppState> GetAppStateFromSession()
    {
        var jsonState = await _jsRuntime.InvokeAsync<string?>("browserStorage.getSessionItem", AppStateKey);

        if (string.IsNullOrWhiteSpace(jsonState))
        {
            return new AppState();
        }

        var appState = JsonSerializer.Deserialize<AppState>(jsonState);
        return appState ?? new AppState();
    }

    public async Task SetInitalAppStateToSession(AppState state)
    {
        var jsonState = JsonSerializer.Serialize(state);
        await _jsRuntime.InvokeVoidAsync("browserStorage.setSessionItem", InitialAppStateKey, jsonState);
    }

    public async Task<AppState?> GetInitalAppStateFromSession()
    {
        var jsonState = await _jsRuntime.InvokeAsync<string?>("browserStorage.getSessionItem", InitialAppStateKey);

        if (string.IsNullOrWhiteSpace(jsonState))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AppState>(jsonState);
    }
}