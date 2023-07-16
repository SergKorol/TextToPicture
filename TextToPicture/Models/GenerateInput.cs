using System.Text.Json.Serialization;

namespace TextToPicture.Models;

public record GenerateInput
{
    [JsonPropertyName("prompt")] public string? Prompt { get; set; }
    [JsonPropertyName("n")] public short? N { get; set; }
    [JsonPropertyName("size")] public string? Size { get; set; }
}
