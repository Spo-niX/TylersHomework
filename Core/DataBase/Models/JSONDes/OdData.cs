using System.Text.Json.Serialization;
public class OdData
{
    [JsonPropertyName("has_api")]
    public bool HasApi { get; set; }
    
    [JsonPropertyName("has_gcdata")]
    public bool HasGcdata { get; set; }
    
    [JsonPropertyName("has_parsed")]
    public bool HasParsed { get; set; }
    
    [JsonPropertyName("has_archive")]
    public bool HasArchive { get; set; }
}