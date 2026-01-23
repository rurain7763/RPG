using UnityEngine;

public abstract class LevelEnvironment : MonoBehaviour
{
    LevelSystem levelSystem;

    protected virtual void OnEnable()
    {
        levelSystem = AppManager.Instance.GetSystem<LevelSystem>();
        if (levelSystem == null)
        {
            Logger.Error("LevelEnvironment must be used with LevelSystem.");
            return;
        }

        levelSystem.Environment = this;
    }

    protected virtual void OnDisable()
    {
        if (levelSystem != null && levelSystem.Environment == this)
        {
            levelSystem.Environment = null;
        }
    }
}
