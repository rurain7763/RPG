using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HomingMovement2D : MonoBehaviour
{
    private const float RotationDampingFactor = 2.73f;

    public float Speed = 5f;
    public float MoveDampingInnerDistance = 0.5f;

    [Range(0, 360)] public float RotationSpeedDeg = 15f;
    public float RotationDampingOuterDistance = 1f;

    public Transform Target;

    private Rigidbody2D rb;

    public UnityEngine.Events.UnityEvent OnTargetReached;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (Target == null)
        {
            return;
        }

        Vector2 desiredDir = Target.position - transform.position;
        float distance = desiredDir.magnitude;

        if (distance == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        desiredDir /= distance;

        HandleRotation(desiredDir, distance);
        HandleSpeed(distance);

        if (distance < 0.05f)
        {
            OnTargetReached?.Invoke();
        }
    }

    private void HandleRotation(Vector2 direction, float distance)
    {
        float dampingFactor = 1f;
        if (distance > RotationDampingOuterDistance)
        {
            dampingFactor = Mathf.Clamp(RotationDampingOuterDistance / distance * RotationDampingFactor, 0, 0.28f);
        }

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angle = Mathf.LerpAngle(rb.rotation, targetAngle, RotationSpeedDeg * Time.fixedDeltaTime * dampingFactor);

        rb.rotation = angle;
    }

    private void HandleSpeed(float distance)
    {
        float currentSpeed = Speed;
        if (distance < MoveDampingInnerDistance)
        {
            currentSpeed = Speed * (distance / MoveDampingInnerDistance);
            if (distance < 0.05f)
            {
                currentSpeed = 0f;
            }
        }

        rb.linearVelocity = transform.right * currentSpeed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, MoveDampingInnerDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, RotationDampingOuterDistance);
    }
}
