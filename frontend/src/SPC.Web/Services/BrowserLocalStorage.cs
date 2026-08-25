using System.Text.Json;
using Microsoft.JSInterop;

namespace SPC.Web.Services;

public sealed class BrowserLocalStorage(IJSRuntime jsRuntime) : IBrowserLocalStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    public async Task<T?> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public async Task SetItemAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, key, json);
    }

    public Task RemoveItemAsync(string key, CancellationToken cancellationToken = default) =>
        jsRuntime.InvokeVoidAsync("localStorage.removeItem", cancellationToken, key).AsTask();
}
