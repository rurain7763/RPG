using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class TargetDetector2D : MonoBehaviour
{
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected Transform detectionOrigin;
    [SerializeField] protected int maxDetectedTargets = 20;

    public LayerMask TargetLayer { get => targetLayer; set => targetLayer = value; }

    protected virtual void Awake()
    {
        if (detectionOrigin == null)
        {
            detectionOrigin = transform;
        }
    }

    public abstract Collider2D DetectFirstTarget(Predicate<Collider2D> filter = null);
    public abstract IReadOnlyList<Collider2D> DetectTargets(Predicate<Collider2D> filter = null);
    public abstract void EachDetectedTargets(Action<Collider2D> onDetected, Predicate<Collider2D> filter = null);
    public abstract bool IsTargetInRange(Collider2D target);

    public virtual IReadOnlyList<Collider2D> DetectTargetsByDistance(Predicate<Collider2D> filter = null)
    {
        return DetectTargets(filter)
            .OrderBy(target =>
                ((Vector2)target.transform.position - (Vector2)detectionOrigin.position).sqrMagnitude).ToList();
    }

    public virtual Collider2D DetectNearestTarget(Predicate<Collider2D> filter = null)
    {
        var targets = DetectTargets(filter);

        Collider2D nearestTarget = null;
        float nearestDistanceSqr = float.MaxValue;
        Vector2 currentPosition = detectionOrigin.position;
        foreach (var target in targets)
        {
            float distanceSqr = ((Vector2)target.transform.position - currentPosition).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestTarget = target;
            }
        }

        return nearestTarget;
    }
}
