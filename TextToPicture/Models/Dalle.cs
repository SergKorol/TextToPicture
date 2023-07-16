namespace TextToPicture.Models;

public record Dalle
{
    public GenerateInput? GenerateInput { get; set; }
    public EditInput? EditInput { get; set; }
    public VariationInput? VariationInput { get; set; }
    public List<string>? Links { get; set; }
}