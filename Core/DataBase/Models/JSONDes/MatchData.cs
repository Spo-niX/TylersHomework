using System.Text.Json.Serialization;

public class MatchData
{
    [JsonPropertyName("players")]
    public List<PlayerData> Players { get; set; }
    
    [JsonPropertyName("radiant_win")]
    public bool RadiantWin { get; set; }
    
    [JsonPropertyName("duration")]
    public int Duration { get; set; }
    
    [JsonPropertyName("start_time")]
    public long StartTime { get; set; }
    
    [JsonPropertyName("match_id")]
    public long MatchId { get; set; }
    
    [JsonPropertyName("game_mode")]
    public int GameMode { get; set; }
    
    [JsonPropertyName("lobby_type")]
    public int LobbyType { get; set; }
    
    [JsonPropertyName("cluster")]
    public int Cluster { get; set; }
    
    [JsonPropertyName("patch")]
    public int Patch { get; set; }
    
    [JsonPropertyName("region")]
    public int Region { get; set; }
    
    [JsonPropertyName("picks_bans")]
    public List<PickBan> PicksBans { get; set; }
    
    [JsonPropertyName("od_data")]
    public OdData OdData { get; set; }
}