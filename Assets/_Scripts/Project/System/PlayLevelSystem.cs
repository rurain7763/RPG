using System;

public class PlayLevelSystem
{
    public int TotalExp { get; private set; }
    public int CurrentLevel { get; private set; }
    public int ExpToReachNextLevel { get; private set; }
    public int CurrentExp { get; private set; }

    public event Action OnLevelChanged;
    public event Action OnExpChanged;

    public PlayLevelSystem()
    {
        TotalExp = 0;
        CurrentLevel = 1;
        ExpToReachNextLevel = GetExpToReachLevel(CurrentLevel + 1);
        CurrentExp = 0;
    }

    public PlayLevelSystem(PlayLevelSystemDTO dto)
    {
        TotalExp = dto.TotalExp;
        CurrentLevel = CalcLevelFromTotalExp(TotalExp);
        ExpToReachNextLevel = GetExpToReachLevel(CurrentLevel + 1);
        CurrentExp = TotalExp - GetCumulativeExpForLevel(CurrentLevel);
    }

    public void AddExp(int amount)
    {
        bool levelChanged = false;

        TotalExp += amount;
        CurrentExp += amount;
        while (CurrentLevel < RPG.MaxPlayerLevel && CurrentExp >= ExpToReachNextLevel)
        {
            CurrentExp -= ExpToReachNextLevel;
            CurrentLevel++;
            ExpToReachNextLevel = GetExpToReachLevel(CurrentLevel + 1);
            levelChanged = true;
        }

        if (levelChanged)
        {
            OnLevelChanged?.Invoke();
        }

        OnExpChanged?.Invoke();
    }

    public void SubtractExp(int amount)
    {
        bool levelChanged = false;

        if (amount >= TotalExp)
        {
            TotalExp = 0;
            CurrentExp = 0;
            CurrentLevel = 1;
            ExpToReachNextLevel = GetExpToReachLevel(CurrentLevel + 1);
            OnExpChanged?.Invoke();
            return;
        }

        TotalExp -= amount;

        if (amount <= CurrentExp)
        {
            CurrentExp -= amount;
        }
        else
        {
            int remainingSubtract = amount - CurrentExp;
            while (remainingSubtract > 0 && CurrentLevel > 1)
            {
                CurrentLevel--;
                ExpToReachNextLevel = GetExpToReachLevel(CurrentLevel + 1);

                if (remainingSubtract > ExpToReachNextLevel)
                {
                    remainingSubtract -= ExpToReachNextLevel;
                }
                else
                {
                    CurrentExp = ExpToReachNextLevel - remainingSubtract;
                    remainingSubtract = 0;
                }

                levelChanged = true;
            }
        }

        if (levelChanged)
        {
            OnLevelChanged?.Invoke();
        }

        OnExpChanged?.Invoke();
    }

    public PlayLevelSystemDTO CaptureDTO()
    {
        return new PlayLevelSystemDTO
        {
            TotalExp = TotalExp
        };
    }

    private int CalcLevelFromTotalExp(int totalExp)
    {
        int level = 1;
        int cumulativeExp = 0;
        while (level < RPG.MaxPlayerLevel)
        {
            int nextExp = GetExpToReachLevel(level + 1);
            if (totalExp < cumulativeExp + nextExp)
            {
                break;
            }
            cumulativeExp += nextExp;
            level++;
        }
        return level;
    }

    private int GetExpToReachLevel(int level)
    {
        if (level <= 1 || level > RPG.MaxPlayerLevel)
        {
            return 0;
        }
#if false
        return (int)(50 * Math.Pow(level - 1, 2));
#else
        return 50;
#endif
    }

    private int GetCumulativeExpForLevel(int level)
    {
        int totalExp = 0;
        for (int i = 2; i <= level; i++)
        {
            totalExp += GetExpToReachLevel(i);
        }
        return totalExp;
    }
}

[Serializable]
public class PlayLevelSystemDTO
{
    public int TotalExp;
}