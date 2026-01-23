using UnityEngine;

public class DeathThunder : MonoBehaviour
{
    private Animator animator;
    private AnimationEventReciever animEventReciever;
    private TargetDetector2D detector;

    private ICombatable owner;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        animEventReciever = GetComponentInChildren<AnimationEventReciever>();
        detector = GetComponentInChildren<TargetDetector2D>();

        animator.enabled = false;
        animEventReciever.RegisterEventHandler("Hit", HandleHitEvent);
    }

    public void Setup(ICombatable owner)
    {
        this.owner = owner;
        detector.TargetLayer = owner.CombatSystem.EnemyMask;
        animator.enabled = true;

        if (owner is Entity entity)
        {
            transform.SetParent(entity.IncludedLevel.transform, true);
        }

        Destroy(gameObject, 2.0f);
    }

    private void HandleHitEvent(IEventData data)
    {
        detector.EachDetectedTargets(collision =>
        {
            var target = collision.GetComponent<ICombatable>();
            if (target == null)
            {
                return;
            }

            var damage = RPG.CalcDamage(owner, target);
            var buff = RPG.CalcBuffByDamage(owner, target, damage);

            target.CombatSystem.TakeDamage(damage);
            target.BuffSystem.AddBuff(buff);
        });
    }
}