using UnityEngine;

public class ArcherElfArrow : Projectile2D
{
    private TrailRenderer trailRenderer;
    private Animator animator;

    private ICombatable owner;

    protected override void Awake()
    {
        base.Awake();

        trailRenderer = GetComponentInChildren<TrailRenderer>();
        animator = GetComponentInChildren<Animator>();
    }

    public void Setup(ICombatable owner)
    {
        this.owner = owner;
        HitLayerMask = owner.CombatSystem.EnemyMask;
        trailRenderer.emitting = true;
        animator.enabled = true;

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

        if (combatable.CombatSystem.ActiveCounter)
        {
            Setup(combatable);

            var speed = Velocity.magnitude;
            var reversedDirection = -(Velocity / speed);
            Fire(reversedDirection, speed);
        }
        else
        {
            var damage = RPG.CalcDamage(owner, combatable);
            var buff = RPG.CalcBuffByDamage(owner, combatable, damage);

            combatable.CombatSystem.TakeDamage(damage);
            combatable.BuffSystem.AddBuff(buff);
        }
    }

    protected override void OnStuck(Collider2D collision)
    {
        trailRenderer.emitting = false;
        animator.enabled = false;
    }
}