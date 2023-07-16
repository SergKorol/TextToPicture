using System.Text.Json.Serialization;

namespace TextToPicture.Models;

public record Link
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}