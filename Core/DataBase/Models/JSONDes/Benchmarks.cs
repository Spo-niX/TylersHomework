using System.Text.Json.Serialization;

public class Benchmarks
{
    [JsonPropertyName("gold_per_min")]
    public BenchmarkData GoldPerMin { get; set; }
    
    [JsonPropertyName("xp_per_min")]
    public BenchmarkData XpPerMin { get; set; }
    
    [JsonPropertyName("kills_per_min")]
    public BenchmarkData KillsPerMin { get; set; }
    
    [JsonPropertyName("last_hits_per_min")]
    public BenchmarkData LastHitsPerMin { get; set; }
    
    [JsonPropertyName("hero_damage_per_min")]
    public BenchmarkData HeroDamagePerMin { get; set; }
    
    [JsonPropertyName("hero_healing_per_min")]
    public BenchmarkData HeroHealingPerMin { get; set; }
    
    [JsonPropertyName("tower_damage")]
    public BenchmarkData TowerDamage { get; set; }
}