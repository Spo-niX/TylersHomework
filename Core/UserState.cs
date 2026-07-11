public static class UserStates
{
    private static readonly Dictionary<long, string> _states = new();
    
    public static void SetState(long tgId, string state)
    {
        _states[tgId] = state;
    }
    
    public static string GetState(long tgId)
    {
        _states.TryGetValue(tgId, out var state);
        return state;
    }
    
    public static void ClearState(long tgId)
    {
        _states.Remove(tgId);
    }
}