using System.Text.Json.Serialization;

namespace TextToPicture.Models;

public record ResponseModel
{
    [JsonPropertyName("created")]
    public long Created { get; set; }
    [JsonPropertyName("data")]
    public List<Link>? Data { get; set; }
}