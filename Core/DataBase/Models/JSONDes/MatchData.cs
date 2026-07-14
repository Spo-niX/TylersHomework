using System.Text.Json.Serialization;

public class MatchData
{
    [JsonPropertyName("players")]
    public List<PlayerData> Players { get; set; }
    
    [JsonPropertyName("radiant_win")]
    public bool RadiantWin { get; set; }
}