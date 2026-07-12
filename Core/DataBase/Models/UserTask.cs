namespace TylersHomework.Core.Database.Models;

public class UserTask
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public int Mode { get; set; }
    public int Hero { get; set; }
    public List<int>? Slots { get; set; }
}