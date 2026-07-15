using System.Text.Json.Serialization;
public class BenchmarkData
{
    [JsonPropertyName("raw")]
    public double Raw { get; set; }
    
    [JsonPropertyName("pct")]
    public double Pct { get; set; }
}