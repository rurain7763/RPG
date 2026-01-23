using UnityEngine;

public enum Projectile2DHitPolicy
{
    Destroy,
    Stuck,
}

public class Projectile2D : MonoBehaviour
{
    public LayerMask HitLayerMask;
    public LayerMask StuckLayerMask;
    public Projectile2DHitPolicy HitPolicy = Projectile2DHitPolicy.Destroy;
    public bool AlighnToVelocity = true;
    public float LifeTimeAfterFire = 5f; // 0 means infinite
    public float LifeTimeAfterStuck = 10f; // 0 means infinite
    public int LimitHitCount = 0; // 0 means infinite

    private float fireTime = -1f;
    private float stuckTime = -1f;

    private int hits = 0;

    public Rigidbody2D RigidBody { get; private set; }
    public Collider2D Collider { get; private set; }
    public bool IsFired => fireTime > 0f;
    public bool IsStuck => stuckTime > 0f;
    public Vector2 Velocity => RigidBody.linearVelocity;

    protected virtual void Awake()
    {
        RigidBody = GetComponent<Rigidbody2D>();
        Collider = GetComponent<Collider2D>();

        if (Collider.isTrigger == false)
        {
            Logger.Warn("Projectile2D requires Collider2D with isTrigger=true");
            Collider.isTrigger = true;
        }
    }

    protected virtual void Update()
    {
        HandleLifeTime();
        HandleFacing();
    }

    private void HandleLifeTime()
    {
        if (IsStuck)
        {
            if (LifeTimeAfterStuck > 0f && (Time.time - stuckTime >= LifeTimeAfterStuck))
            {
                Destroy(gameObject);
            }
        }
        else if (IsFired)
        {
            if (LifeTimeAfterFire > 0f && (Time.time - fireTime >= LifeTimeAfterFire))
            {
                Destroy(gameObject);
            }
        }
    }

    private void HandleFacing()
    {
        if (!AlighnToVelocity || IsStuck || RigidBody.linearVelocity.sqrMagnitude < 0.01f)
        {
            return;
        }

        transform.right = RigidBody.linearVelocity.normalized;
    }

    public void Fire(Vector2 direction, float speed)
    {
        Fire(direction * speed);
    }

    public void Fire(Vector2 velocity)
    {
        fireTime = Time.time;
        stuckTime = -1f;
        hits = 0;
        RigidBody.linearVelocity = velocity;
        Collider.enabled = true;

        if (AlighnToVelocity)
        {
            transform.right = velocity.normalized;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        LayerMask layerMask = 1 << collision.gameObject.layer;

        bool isStuck = false;
        bool isDestroyed = false;

        if ((layerMask & StuckLayerMask) != 0)
        {
            isStuck = true;
        }

        if ((layerMask & HitLayerMask) != 0)
        {
            ++hits;
            
            OnHit(collision);

            if (LimitHitCount == 0 || (LimitHitCount > 0 && hits == LimitHitCount))
            {
                if (HitPolicy == Projectile2DHitPolicy.Stuck)
                {
                    isStuck = true;
                }
                else if (HitPolicy == Projectile2DHitPolicy.Destroy)
                {
                    isDestroyed = true;
                }
            }
        }

        if (isDestroyed)
        {
            Destroy(gameObject);
        }
        else if (isStuck)
        {
            if (IsStuck)
            {
                return;
            }

            Vector2 hitPoint = collision.ClosestPoint(transform.position);
            transform.position = hitPoint;
            transform.SetParent(collision.transform);

            stuckTime = Time.time;
            RigidBody.linearVelocity = Vector2.zero;
            RigidBody.bodyType = RigidbodyType2D.Kinematic;
            Collider.enabled = false;

            OnStuck(collision);
        }
    }

    protected virtual void OnHit(Collider2D collision)
    {
    }

    protected virtual void OnStuck(Collider2D collision)
    {
    }
}