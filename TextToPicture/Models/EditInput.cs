using System.Text.Json.Serialization;

namespace TextToPicture.Models;

public record EditInput
{
    [JsonPropertyName("prompt")] public string Prompt { get; set; }
    [JsonPropertyName("n")] public short N { get; set; }
    [JsonPropertyName("size")] public string Size { get; set; }
    [JsonPropertyName("image")] public IFormFile Image { get; set; }
    [JsonPropertyName("mask")] public IFormFile Mask { get; set; }
}