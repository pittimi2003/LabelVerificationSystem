using Microsoft.JSInterop;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ScriptLoaderService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HashSet<string> _loadedScripts = new(); // Prevent duplicate loads
    private bool _loadScriptFunctionDefined = false;

    public ScriptLoaderService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task EnsureLoadScriptFunctionAsync()
    {
        if (_loadScriptFunctionDefined)
            return;

        await _jsRuntime.InvokeVoidAsync("eval", @"
            window.loadScript ??= function(url) {
                return new Promise(function(resolve, reject) {
                    if (document.querySelector(`script[src='${url}']`)) {
                        resolve();
                        return;
                    }
                    var script = document.createElement('script');
                    script.src = url;
                    script.onload = resolve;
                    script.onerror = reject;
                    document.head.appendChild(script);
                });
            };
        ");

        _loadScriptFunctionDefined = true;
    }

    public async Task LoadScriptAsync(string url)
    {
        if (_loadedScripts.Contains(url))
            return;

        await EnsureLoadScriptFunctionAsync();

        await _jsRuntime.InvokeVoidAsync("loadScript", url);

        _loadedScripts.Add(url);
    }

    public async Task AddInlineScriptAsync(string jsCode)
    {
        await _jsRuntime.InvokeVoidAsync("eval", jsCode);
    }

    public void ResetLoadedScripts()
    {
        _loadedScripts.Clear();
        _loadScriptFunctionDefined = false;
    }
}
