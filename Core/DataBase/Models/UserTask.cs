namespace TylersHomework.Core.Database.Models;

public class UserTask
{
    public int Id { get; set; }
    public long OwnerId { get; set; }
    public int Mode { get; set; }
    public int Hero { get; set; }
    public List<string>? Slots { get; set; }
    public bool IsActive { get; set; }
}