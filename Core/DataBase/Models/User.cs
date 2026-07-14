namespace TylersHomework.Core.Database.Models;

public class User
{
    public int Id { get; set; }
    public long TgId { get; set; }
    public string? AgentName { get; set; }
    public int Mmr { get; set; }
    public int TaskCompleted { get; set; }
    public long SteamId {get ; set; }
}