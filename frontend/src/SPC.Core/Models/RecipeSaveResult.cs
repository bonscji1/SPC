namespace SPC.Core.Models;

public sealed class RecipeSaveResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public static RecipeSaveResult Succeeded(string message) =>
        new() { Success = true, Message = message };

    public static RecipeSaveResult Failed(string message) =>
        new() { Success = false, Message = message };
}
