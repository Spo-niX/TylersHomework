public static class UserTaskState
{
    private static readonly Dictionary<long, List<int>> _states = new();
    
    public static void SetState(long Id, List<int> state)
    {
        _states[Id] = state;
    }
    
    public static List<int> GetState(long Id)
    {
        _states.TryGetValue(Id, out var state);
        return state!;
    }
    
    public static void ClearState(long Id)
    {
        _states.Remove(Id);
    }
}