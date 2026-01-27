using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(ParentConstraint))]
public class Attacher : MonoBehaviour
{
    private ParentConstraint constraint;
    private Vector3 validPos;
    private Quaternion validRot;

    public Transform Target { get; private set; }

    public event Action OnLooseTarget;

    private void Awake()
    {
        constraint = GetComponent<ParentConstraint>();
    }

    private void LateUpdate()
    {
        if (!constraint.constraintActive)
        {
            return;
        }

        if (Target == null)
        {
            OnLooseTarget?.Invoke();
        }
        else
        {
            validPos = transform.position;
            validRot = transform.rotation;
        }
    }

    private void ClearConstraintSources()
    {
        while (constraint.sourceCount > 0)
        {
            constraint.RemoveSource(0);
        }
    }

    public void Attach(Transform target, bool maintainWorldPosition = true, bool maintainWorldRotation = true)
    {
        var source = new ConstraintSource
        {
            sourceTransform = target,
            weight = 1f
        };

        ClearConstraintSources();
        constraint.AddSource(source);

        Vector3 localPos = Vector3.zero;
        if (maintainWorldPosition)
        {
            localPos = target.InverseTransformPoint(transform.position);
            Vector3 lossyScale = target.lossyScale;
            localPos = new Vector3(localPos.x * lossyScale.x, localPos.y * lossyScale.y, localPos.z * lossyScale.z);
        }

        constraint.SetTranslationOffset(0, localPos);

        Vector3 localRot = Vector3.zero;
        if (maintainWorldRotation)
        {
            localRot = (Quaternion.Inverse(target.rotation) * transform.rotation).eulerAngles;
        }

        constraint.SetRotationOffset(0, localRot);

        constraint.weight = 1f;
        constraint.constraintActive = true;

        validPos = transform.position;
        validRot = transform.rotation;

        Target = target;
    }

    public void Detach()
    {
        if (!constraint.constraintActive)
        {
            return;
        }

        constraint.constraintActive = false;
        constraint.weight = 0f;

        ClearConstraintSources();

        transform.SetPositionAndRotation(validPos, validRot);

        Target = null;
    }
}