using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shard : MonoBehaviour
{
    [SerializeField] private VFXID explosionVFX;
    [SerializeField] private Transform groundAnchor;
    [SerializeField] private CircleDetector explosionDetector;
    [SerializeField] private CircleDetector targetDetector;
    [SerializeField] private float detonationTime;
    [SerializeField] private float speed;

    private float spawnTime;
    private Transform closestTarget;

    private Coroutine explodeManuallyCo;

    public Entity Owner { get; set; }
    public Vector2 GroundPosition => groundAnchor.position;
    public bool MoveToClosestEnemy { get; set; } = false;
    public bool MoveToManualTarget { get; set; } = false;
    public Entity ManualTarget { get; set; }
    public bool AutoExplode { get; set; } = true;

    private void Start()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        if (!AutoExplode)
        {
            return;
        }

        HandleMoveToManualTarget();
        HandleMoveToClosestTarget();
        HandleExplosion();
    }

    private void HandleMoveToManualTarget()
    {
        if (!MoveToManualTarget || ManualTarget == null)
        {
            return;
        }

        Vector2 newPosition = Vector2.MoveTowards(transform.position, ManualTarget.CenterPosition, speed * Time.deltaTime);
        transform.position = newPosition;
    }

    private void HandleMoveToClosestTarget()
    {
        if (!MoveToClosestEnemy)
        {
            return;
        }

        if (closestTarget == null)
        {
            var targets = targetDetector.DetectTargets();

            float closestDist = float.MaxValue;
            closestTarget = null;
            foreach (var target in targets)
            {
                float sqrDist = (target.transform.position - transform.position).sqrMagnitude;
                if (sqrDist < closestDist)
                {
                    closestDist = sqrDist;
                    closestTarget = target.transform;
                }
            }
        }
        else
        {
            Vector2 newPosition = Vector2.MoveTowards(transform.position, closestTarget.position, speed * Time.deltaTime);
            newPosition.y = transform.position.y;

            transform.position = newPosition;
        }
    }

    private void HandleExplosion()
    {
        var targets = explosionDetector.DetectTargets();

        if (targets.Count > 0 || Time.time - spawnTime >= detonationTime)
        {
            Explode(targets);
            Destroy(gameObject);
        }
    }

    private void Explode(IReadOnlyList<Collider2D> colliders)
    {
        if (Owner is ICombatable combatable)
        {
            foreach (var collider in colliders)
            {
                var otherCombatable = collider.GetComponent<ICombatable>();
                if (otherCombatable == null)
                {
                    continue;
                }

                var damage = RPG.CalcDamage(combatable, otherCombatable);

                otherCombatable.CombatSystem.TakeDamage(damage);
                otherCombatable.CombatSystem.Knockback(transform.position, 10f, 1.0f);
            }
        }

        var vfx = RPG.VFXSys.SpawnVFX(Local.GetVFXPath(explosionVFX));
        vfx.transform.position = transform.position;
    }

    public void ExplodeManually(float delay = 0)
    {
        if (explodeManuallyCo != null)
        {
            StopCoroutine(explodeManuallyCo);
        }

        explodeManuallyCo = StartCoroutine(ExplodeManuallyCo(delay));
    }

    private IEnumerator ExplodeManuallyCo(float delay)
    {
        yield return new WaitForSeconds(delay);
        var targets = explosionDetector.DetectTargets();
        Explode(targets);
        Destroy(gameObject);
    }
}
