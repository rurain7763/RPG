using UnityEngine;

public class MagicBall : Projectile2D
{
    private Animator animator;

    private ICombatable owner;

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponentInChildren<Animator>();
    }
    
    public void Setup(ICombatable owner)
    {
        this.owner = owner;
        HitLayerMask = owner.CombatSystem.EnemyMask;
        animator.enabled = false;

        if (owner is Entity entity)
        {
            transform.SetParent(entity.IncludedLevel.transform);
        }
    }

    protected override void OnHit(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();
        if (entity == null)
        {
            return;
        }

        ICombatable combatable = entity as ICombatable;
        if (combatable == null)
        {
            return;
        }

        var damage = RPG.CalcDamage(owner, combatable);
        var buff = RPG.CalcBuffByDamage(owner, combatable, damage);
        combatable.CombatSystem.TakeDamage(damage);
        combatable.BuffSystem.AddBuff(buff);
    }

    protected override void OnStuck(Collider2D collision)
    {
        animator.enabled = true;
    }
}