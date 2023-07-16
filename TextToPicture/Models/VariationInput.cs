using System.Text.Json.Serialization;

namespace TextToPicture.Models;

public record VariationInput
{
    [JsonPropertyName("n")] public short N { get; set; }
    [JsonPropertyName("size")] public string Size { get; set; }
    [JsonPropertyName("image")] public IFormFile Image { get; set; }
}