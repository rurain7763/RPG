using System;
using UnityEngine;

[Serializable]
public struct ParallaxLayer2D
{
    public Transform layerTransform;
    public BoxCollider2D layerCollider;
    [Range(0f, 1f)] public float parallaxFactor;
    public bool constrainXPosition;
    public bool constrainYPosition;
}

public class ParallaxBackground2D : MonoBehaviour
{
    [SerializeField] private ParallaxLayer2D[] parallaxLayers;
    [SerializeField] private Transform followTarget;

    private Vector3 previousTargetPosition;

    public Transform FollowTarget
    {
        get => followTarget;
        set => followTarget = value;
    }

    private void Start()
    {
        if (followTarget != null)
        {
            previousTargetPosition = followTarget.position;
        }
    }

    private void FixedUpdate()
    {
        if (followTarget == null)
        {
            return;
        }

        HandleMove();
        HandleSnap();
    }

    private void HandleMove()
    {
        Vector3 deltaMovement = followTarget.position - previousTargetPosition;
        foreach (var layer in parallaxLayers)
        {
            Vector3 newLayerPosition = layer.layerTransform.position + deltaMovement * layer.parallaxFactor;
            if (layer.constrainXPosition)
            {
                newLayerPosition.x = layer.layerTransform.position.x;
            }

            if (layer.constrainYPosition)
            {
                newLayerPosition.y = layer.layerTransform.position.y;
            }

            newLayerPosition.z = layer.layerTransform.position.z;

            layer.layerTransform.position = newLayerPosition;
        }

        previousTargetPosition = followTarget.position;
    }

    private void HandleSnap()
    {
        foreach (var layer in parallaxLayers)
        {
            Vector3 layerPosition = layer.layerTransform.position;
            Vector3 targetPosition = followTarget.position;
            Bounds layerBounds = layer.layerCollider.bounds;
            float halfWidth = layerBounds.extents.x;
            float halfHeight = layerBounds.extents.y;

            if (!layer.constrainXPosition)
            {
                float targetLayerDeltaX = targetPosition.x - layerPosition.x;

                if (targetLayerDeltaX > halfWidth)
                {
                    layerPosition.x += layerBounds.size.x;
                }
                else if (targetLayerDeltaX < -halfWidth)
                {
                    layerPosition.x -= layerBounds.size.x;
                }
            }

            if (!layer.constrainYPosition)
            {
                float targetLayerDeltaY = targetPosition.y - layerPosition.y;

                if (targetLayerDeltaY > halfHeight)
                {
                    layerPosition.y += layerBounds.size.y;
                }
                else if (targetLayerDeltaY < -halfHeight)
                {
                    layerPosition.y -= layerBounds.size.y;
                }
            }

            layer.layerTransform.position = layerPosition;
        }
    }
}
