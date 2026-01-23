using System;
using System.Collections.Generic;
using UnityEngine;

public class CircleDetector : TargetDetector2D
{
    [SerializeField] private float detectionRadius = 5f;

    private ContactFilter2D contactFilter;
    private List<Collider2D> rawTargets;
    private List<Collider2D> filteredTargets;

    public float DetectionRadius { get => detectionRadius; set => detectionRadius = value; }

    protected override void Awake()
    {
        base.Awake();

        rawTargets = new(maxDetectedTargets);
        filteredTargets = new(maxDetectedTargets);

        contactFilter = new();
        contactFilter.SetLayerMask(targetLayer);
    }

    public override Collider2D DetectFirstTarget(Predicate<Collider2D> filter = null)
    {
        rawTargets.Clear();
        Physics2D.OverlapCircle(detectionOrigin.position, detectionRadius, contactFilter, rawTargets);

        if (filter == null)
        {
            return rawTargets.Count > 0 ? rawTargets[0] : null;
        }

        for (int i = 0; i < rawTargets.Count; i++)
        {
            if (filter(rawTargets[i]))
            {
                return rawTargets[i];
            }
        }

        return null;
    }

    public override IReadOnlyList<Collider2D> DetectTargets(Predicate<Collider2D> filter = null)
    {
        rawTargets.Clear();
        filteredTargets.Clear();

        Physics2D.OverlapCircle(detectionOrigin.position, detectionRadius, contactFilter, rawTargets);

        if (filter == null)
        {
            return rawTargets;
        }

        for (int i = 0; i < rawTargets.Count; i++)
        {
            if (filter(rawTargets[i]))
            {
                filteredTargets.Add(rawTargets[i]);
            }
        }

        return filteredTargets;
    }

    public override void EachDetectedTargets(Action<Collider2D> onDetected, Predicate<Collider2D> filter = null)
    {
        rawTargets.Clear();
        Physics2D.OverlapCircle(detectionOrigin.position, detectionRadius, contactFilter, rawTargets);
        for (int i = 0; i < rawTargets.Count; i++)
        {
            var target = rawTargets[i];
            if (filter == null || filter(target))
            {
                onDetected(target);
            }
        }
    }

    public override bool IsTargetInRange(Collider2D target)
    {
        if (target == null)
        {
            return false;
        }

        float distanceSqr = ((Vector2)target.transform.position - (Vector2)detectionOrigin.position).sqrMagnitude;
        return distanceSqr <= detectionRadius * detectionRadius;
    }

    private void OnDrawGizmos()
    {
        if (detectionOrigin == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(detectionOrigin.position, detectionRadius);
    }
}
