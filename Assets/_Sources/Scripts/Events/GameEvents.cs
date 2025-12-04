using System;

public static class GameEvents
{
    public static event Action OnSelectionChanged;
    
    public static void TriggerSelectionChanged()
    {
        OnSelectionChanged?.Invoke();
    }
    
}