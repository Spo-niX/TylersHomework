using System.Text.Json.Serialization;

public class PlayerData
{
    [JsonPropertyName("account_id")]
    public long AccountId { get; set; }
    
    [JsonPropertyName("item_0")]
    public int Item0 { get; set; }
    
    [JsonPropertyName("item_1")]
    public int Item1 { get; set; }
    
    [JsonPropertyName("item_2")]
    public int Item2 { get; set; }
    
    [JsonPropertyName("item_3")]
    public int Item3 { get; set; }
    
    [JsonPropertyName("item_4")]
    public int Item4 { get; set; }
    
    [JsonPropertyName("item_5")]
    public int Item5 { get; set; }
    
    [JsonPropertyName("hero_id")]
    public int HeroId { get; set; }
    
    [JsonPropertyName("kills")]
    public int Kills { get; set; }
    
    [JsonPropertyName("deaths")]
    public int Deaths { get; set; }
    
    [JsonPropertyName("assists")]
    public int Assists { get; set; }
}