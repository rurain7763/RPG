using System;
using System.Collections;
using UnityEngine;

public class Wisp : MonoBehaviour
{
    [SerializeReference, SubclassSelector] private IWispEffect wispEffect;

    private Rigidbody2D rb;
    private HomingMovement2D homingMovement;
    private Entity target;

    private Coroutine setTargetCo;

    public Entity Owner { get; set; }

    public Entity Target
    {
        get => target;
        set
        {
            target = value;
            if (target != null)
            {
                if (setTargetCo != null)
                {
                    StopCoroutine(setTargetCo);
                }

                setTargetCo = StartCoroutine(SetHomingTarget(value));
            }
        }
    }

    public IWispEffect Effect { get => wispEffect; set => wispEffect = value; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        homingMovement = GetComponent<HomingMovement2D>();
    }

    private void Start()
    {
        Vector2 rndDir = UnityEngine.Random.insideUnitCircle.normalized;
        float rndForce = UnityEngine.Random.Range(7f, 10f);

        rb.AddForce(rndDir * rndForce, ForceMode2D.Impulse);

        homingMovement.OnTargetReached.AddListener(HandleReached);
    }

    private void HandleReached()
    {
        if (Target != null)
        {
            Effect?.Apply(Owner, Target);
        }

        Destroy(gameObject);
    }

    private IEnumerator SetHomingTarget(Entity target)
    {
        yield return new WaitForSeconds(0.3f);
        homingMovement.Target = target.CenterAnchor;
    }
}

public interface IWispEffect
{
    void Apply(Entity owner, Entity target);
}

[Serializable]
public class HealingWispEffect : IWispEffect
{
    public float HealRate;

    public void Apply(Entity owner, Entity target)
    {
        if (target is not ICombatable combatable)
        {
            return;
        }

        var healing = new Healing(combatable.CombatSystem, combatable.CombatSystem)
        {
            Amount = combatable.StatSystem.TotalHealth.FinalValue * HealRate,
        };

        combatable.CombatSystem.TakeHeal(healing);
    }
}

[Serializable]
public class CooldownReductionWispEffect : IWispEffect
{
    public float CooldownReductionRate;

    public void Apply(Entity owner, Entity target)
    {
        if (target is not ISkillUser skillUser)
        {
            return;
        }

        foreach (var skill in skillUser.SkillSystem.Skills)
        {
            float cooldownReduction = skill.Cooldown * CooldownReductionRate;
            skill.ReduceCoolTime(cooldownReduction);
        }
    }
}

[Serializable]
public class CleanseDebuffWispEffect : IWispEffect
{
    public void Apply(Entity owner, Entity target)
    {
        if (target is not ICombatable combatable)
        {
            return;
        }

        combatable.BuffSystem.RemoveAllBuffWithCategory(BuffCategory.Negative);
    }
}