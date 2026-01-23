using UnityEngine;

public abstract class Level : MonoBehaviour
{
    [SerializeField] protected string levelName;

    protected LevelSystem levelSystem;

    public virtual void Init(LevelSystem levelSystem)
    {
        this.levelSystem = levelSystem;
    }

    public virtual void Enter()
    {
    }

    public virtual void Execute(float deltaTime)
    {
    }

    public virtual void Exit()
    {
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            levelName = GetType().Name;
        }
    }
}