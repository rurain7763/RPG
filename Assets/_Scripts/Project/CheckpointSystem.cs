using System;

public class CheckpointSystem
{
    private UUID lastCheckpointID;

    public UUID LastCheckpointID
    {
        get => lastCheckpointID;
        set
        {
            if (lastCheckpointID != value)
            {
                lastCheckpointID = value;
                OnCheckpointChanged?.Invoke();
            }
        }
    }

    public event Action OnCheckpointChanged;
}