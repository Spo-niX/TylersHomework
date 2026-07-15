using System.Text.Json.Serialization;

public class PlayerData
{
    [JsonPropertyName("account_id")]
    public long? AccountId { get; set; }  // 👈 Может быть null для анонимов
    
    [JsonPropertyName("player_slot")]
    public int PlayerSlot { get; set; }
    
    [JsonPropertyName("team_number")]
    public int TeamNumber { get; set; }
    
    [JsonPropertyName("team_slot")]
    public int TeamSlot { get; set; }
    
    [JsonPropertyName("hero_id")]
    public int HeroId { get; set; }
    
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
    
    [JsonPropertyName("backpack_0")]
    public int Backpack0 { get; set; }
    
    [JsonPropertyName("backpack_1")]
    public int Backpack1 { get; set; }
    
    [JsonPropertyName("backpack_2")]
    public int Backpack2 { get; set; }
    
    [JsonPropertyName("item_neutral")]
    public int ItemNeutral { get; set; }
    
    [JsonPropertyName("item_neutral2")]
    public int ItemNeutral2 { get; set; }
    
    [JsonPropertyName("kills")]
    public int Kills { get; set; }
    
    [JsonPropertyName("deaths")]
    public int Deaths { get; set; }
    
    [JsonPropertyName("assists")]
    public int Assists { get; set; }
    
    [JsonPropertyName("leaver_status")]
    public int LeaverStatus { get; set; }
    
    [JsonPropertyName("last_hits")]
    public int LastHits { get; set; }
    
    [JsonPropertyName("denies")]
    public int Denies { get; set; }
    
    [JsonPropertyName("gold_per_min")]
    public int GoldPerMin { get; set; }
    
    [JsonPropertyName("xp_per_min")]
    public int XpPerMin { get; set; }
    
    [JsonPropertyName("level")]
    public int Level { get; set; }
    
    [JsonPropertyName("net_worth")]
    public int NetWorth { get; set; }
    
    [JsonPropertyName("hero_damage")]
    public int HeroDamage { get; set; }
    
    [JsonPropertyName("tower_damage")]
    public int TowerDamage { get; set; }
    
    [JsonPropertyName("hero_healing")]
    public int HeroHealing { get; set; }
    
    [JsonPropertyName("gold")]
    public int Gold { get; set; }
    
    [JsonPropertyName("gold_spent")]
    public int GoldSpent { get; set; }
    
    [JsonPropertyName("personaname")]
    public string PersonaName { get; set; } = "Аноним";
    
    [JsonPropertyName("rank_tier")]
    public int? RankTier { get; set; }  // 👈 Может быть null
    
    [JsonPropertyName("computed_mmr")]
    public double? ComputedMmr { get; set; }
    
    [JsonPropertyName("is_subscriber")]
    public bool IsSubscriber { get; set; }
    
    [JsonPropertyName("radiant_win")]
    public bool RadiantWin { get; set; }
    
    [JsonPropertyName("win")]
    public int Win { get; set; }
    
    [JsonPropertyName("lose")]
    public int Lose { get; set; }
    
    [JsonPropertyName("kda")]
    public double Kda { get; set; }
    
    [JsonPropertyName("abandons")]
    public int Abandons { get; set; }
    
    [JsonPropertyName("benchmarks")]
    public Benchmarks Benchmarks { get; set; }
    
    [JsonPropertyName("ability_upgrades_arr")]
    public List<int> AbilityUpgrades { get; set; }
}