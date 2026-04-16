using Microsoft.JSInterop;
using System.Text.Json;

namespace LabelVerificationSystem.Web.Components.Auth;

public sealed class AuthSessionStorage
{
    private const string AuthSessionKey = "AuthSessionV1";
    private readonly IJSRuntime _jsRuntime;

    public AuthSessionStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SaveAsync(AuthSessionSnapshot snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot);
        await _jsRuntime.InvokeVoidAsync("browserStorage.setSessionItem", AuthSessionKey, json);
    }

    public async Task<AuthSessionSnapshot?> LoadAsync()
    {
        var json = await _jsRuntime.InvokeAsync<string?>("browserStorage.getSessionItem", AuthSessionKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AuthSessionSnapshot>(json);
    }

    public Task ClearAsync()
    {
        return _jsRuntime.InvokeVoidAsync("browserStorage.removeSessionItem", AuthSessionKey).AsTask();
    }
}
