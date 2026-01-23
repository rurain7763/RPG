using System;
using System.Collections.Generic;
using UnityEngine;

public class SquareDetector : TargetDetector2D
{
    [SerializeField] private Vector2 detectionSize = new(5f, 5f);

    private float detectionAngle => detectionOrigin.eulerAngles.z;

    private ContactFilter2D contactFilter;
    private List<Collider2D> rawTargets;
    private List<Collider2D> filteredTargets;

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
        Physics2D.OverlapBox(detectionOrigin.position, detectionSize, detectionAngle, contactFilter, rawTargets);
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
        Physics2D.OverlapBox(detectionOrigin.position, detectionSize, detectionAngle, contactFilter, rawTargets);
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
        Physics2D.OverlapBox(detectionOrigin.position, detectionSize, detectionAngle, contactFilter, rawTargets);
        foreach (var target in rawTargets)
        {
            if (filter == null || filter(target))
            {
                onDetected(target);
            }
        }
    }

    public override bool IsTargetInRange(Collider2D target)
    {
        Vector2 localPoint = detectionOrigin.InverseTransformPoint(target.transform.position);
        Rect rect = new Rect(-detectionSize / 2, detectionSize);
        return rect.Contains(localPoint);
    }

    private void OnDrawGizmos()
    {
        if (detectionOrigin == null)
        {
            return;
        }

        Matrix4x4 backupMat = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(detectionOrigin.position, Quaternion.Euler(0f, 0f, detectionAngle), Vector3.one);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector2.zero, detectionSize);

        Gizmos.matrix = backupMat;
    }
}