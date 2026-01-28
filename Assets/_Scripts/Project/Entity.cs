using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected LayerMask wallLayer;
    [SerializeField] protected Transform centerAnchor;
    [SerializeField] protected float centerToHeadDistance;
    [SerializeField] protected float centerToFeetDistance;
    [SerializeField] protected float centerToFrontDistance;
    [SerializeField] protected float centerToBackDistance;

    public float MoveForce;
    public float JumpForce;

    public SpriteRenderer SpriteRenderer { get; private set; }
    public Animator Animator { get; private set; }
    public Vector2 MovementAxis { get; protected set; }
    public bool IsFacingRight { get; private set; } = true;
    public float FacingDirection => IsFacingRight ? 1f : -1f;
    public bool IsGrounded { get; private set; }

    public Transform CenterAnchor => centerAnchor;
    public Vector2 CenterPosition => centerAnchor.position;
    public float CenterToFrontDistance => centerToFrontDistance;
    public float CenterToBackDistance => centerToBackDistance;
    public float CenterToHeadDistance => centerToHeadDistance;
    public float CenterToFeetDistance => centerToFeetDistance;
    public Vector2 BodySize => new Vector2(centerToFrontDistance + centerToBackDistance, centerToHeadDistance + centerToFeetDistance);

    public RPGLevel IncludedLevel { get; private set; }

    protected virtual void Awake()
    {
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        Animator = GetComponentInChildren<Animator>();
    }

    protected virtual void OnDestroy()
    {
        IncludedLevel.UnregisterEntity(this);
    }

    public virtual void Begin()
    {
        IncludedLevel = RPG.LevelSys.CurrentLevel as RPGLevel;
        IncludedLevel.RegisterEntity(this);
    }

    public virtual void Tick(float delta)
    {
        HandleGroundDetection();
    }

    public virtual void End()
    {
        IncludedLevel.UnregisterEntity(this);
        IncludedLevel = null;
    }

    private void HandleGroundDetection()
    {
        var hit = Physics2D.Raycast(CenterPosition, Vector2.down, centerToFeetDistance, groundLayer);
        IsGrounded = hit.collider != null;
    }

    public void SetFacing(bool right)
    {
        if (IsFacingRight == right)
        {
            return;
        }

        IsFacingRight = right;
        transform.eulerAngles = new Vector3(0, right ? 0 : 180, 0);
    }

    public void LookAt(Vector2 targetPosition)
    {
        SetFacing(targetPosition.x >= CenterPosition.x);
    }

    public void FlipFacing()
    {
        SetFacing(!IsFacingRight);
    }

    public bool IsWallInFront()
    {
        return IsWallImpl(true);
    }

    public bool IsWallBehind()
    {
        return IsWallImpl(false);
    }

    private bool IsWallImpl(bool inFront)
    {
        RaycastHit2D hit = Physics2D.Raycast(CenterPosition, Vector2.right * FacingDirection * (inFront ? 1 : -1), inFront ? centerToFrontDistance : centerToBackDistance, wallLayer);
        return hit.collider != null;
    }

    public bool IsCliffInFront()
    {
        return IsCliffImpl(true);
    }

    public bool IsCliffBehind()
    {
        return IsCliffImpl(false);
    }

    private bool IsCliffImpl(bool inFront)
    {
        Vector2 frontOrigin = CenterPosition + Vector2.right * FacingDirection * centerToFrontDistance;
        Vector2 backOrigin = CenterPosition + Vector2.left * FacingDirection * centerToBackDistance;
        RaycastHit2D frontHit = Physics2D.Raycast(frontOrigin, Vector2.down, centerToFeetDistance, groundLayer);
        RaycastHit2D backHit = Physics2D.Raycast(backOrigin, Vector2.down, centerToFeetDistance, groundLayer);
        return inFront ? (frontHit.collider == null && backHit.collider != null) : (backHit.collider == null && frontHit.collider != null);
    }

    protected virtual void OnDrawGizmos()
    {
        if (centerAnchor == null)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(CenterPosition, CenterPosition + Vector2.up * centerToHeadDistance);
        Gizmos.DrawLine(CenterPosition, CenterPosition + Vector2.down * centerToFeetDistance);
        Gizmos.DrawLine(CenterPosition, CenterPosition + Vector2.right * FacingDirection * centerToFrontDistance);
        Gizmos.DrawLine(CenterPosition, CenterPosition + Vector2.left * FacingDirection * centerToBackDistance);
    }
}
