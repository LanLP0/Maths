using System.Text.Json;

namespace LToolBox.Ui;

public static class Global
{
    public static JsonSerializerOptions SerializerOptions { get; } = new()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = false
    };
}