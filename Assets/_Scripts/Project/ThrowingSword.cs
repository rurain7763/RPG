using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingSword : MonoBehaviour
{
    private static readonly int IdleAnimHash = Animator.StringToHash("Idle");
    private static readonly int SpinAnimHash = Animator.StringToHash("Spinning");

    [SerializeField] private VFXID hitVFX;
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private LayerMask cantPierceLayers;
    [SerializeField] private float backToOwnerSpeed = 10f;
    [SerializeField] private CircleDetector detectorOnSpin;
    [SerializeField] private float spinDetectInterval = 0.2f;
    [SerializeField] private CircleDetector detectorOnBounce;
    [SerializeField] private float bounceSpeed = 7f;

    private Rigidbody2D rb;
    private Animator animator;
    private Attacher attacher;

    private Vector2 originPosition;
    private bool isStucked = false;
    private bool isReturning = false;
    private bool isSpinning = false;
    private bool isBouncing = false;
    private int currentPierceCount = 0;

    private Coroutine backToOwnerCoroutine;
    private Coroutine spinCoroutine;
    private Coroutine bounceCoroutine;

    public Entity Owner { get; set; }
    public Entity LastHitTarget { get; set; }
    public int PierceCount { get; set; }
    public float MaxDistanceFromOrigin { get; set; }
    public int BounceCount { get; set; }

    public event Action OnStuck;
    public event Action OnReachedMaxDistance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        attacher = GetComponent<Attacher>();

        PierceCount = 0;
        MaxDistanceFromOrigin = -1f;
    }

    private void Update()
    {
        if (!rb.simulated)
        {
            return;
        }

        transform.right = rb.linearVelocity.normalized * (isReturning ? -1f : 1f);

        if (MaxDistanceFromOrigin > 0f)
        {
            float distanceFromOrigin = Vector2.Distance(originPosition, transform.position);
            if (distanceFromOrigin >= MaxDistanceFromOrigin)
            {
                OnReachedMaxDistance?.Invoke();
                OnReachedMaxDistance = null;
            }
        }
    }

    public void Throw(Vector2 direction, float force)
    {
        isStucked = false;
        currentPierceCount = 0;
        originPosition = transform.position;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    public void BackToOwner(Action onComplete)
    {
        if (backToOwnerCoroutine != null)
        {
            StopCoroutine(backToOwnerCoroutine);
        }

        backToOwnerCoroutine = StartCoroutine(BackToOwnerCo(onComplete));
    }

    private IEnumerator BackToOwnerCo(Action onComplete)
    {
        DetachFromParent();
        isReturning = true;

        Vector2 directionToOwner = Owner.CenterPosition - (Vector2)transform.position;
        float distanceToOwner = directionToOwner.magnitude;
        while (distanceToOwner >= 0.3f)
        {
            directionToOwner = Owner.CenterPosition - (Vector2)transform.position;
            distanceToOwner = directionToOwner.magnitude;
            directionToOwner /= distanceToOwner;
            rb.linearVelocity = directionToOwner * backToOwnerSpeed;
            yield return null;
        }

        onComplete?.Invoke();
    }

    public void Spin(float duration, Action onComplete)
    {
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
        }

        spinCoroutine = StartCoroutine(SpinCo(duration, onComplete));
    }

    private IEnumerator SpinCo(float duration, Action onComplete)
    {
        DetachFromParent();
        isSpinning = true;
        rb.simulated = false;

        animator.Play(SpinAnimHash);

        ICombatable combatable = Owner as ICombatable;

        float elapsed = 0f;
        float detectTimer = 0f;
        while (elapsed < duration)
        {
            if (combatable != null && detectTimer <= 0)
            {
                var targets = detectorOnSpin.DetectTargets();
                foreach (var targetCollider in targets)
                {
                    ICombatable hitEntity = targetCollider.GetComponentInChildren<ICombatable>();
                    if (hitEntity != null)
                    {
                        var damage = RPG.CalcDamage(combatable, hitEntity);

                        hitEntity.CombatSystem.TakeDamage(damage);
                        
                        var slow = new Slow(0.1f, 0.8f, combatable);
                        hitEntity.BuffSystem.AddBuff(slow);
                    }
                }

                detectTimer += spinDetectInterval;
            }

            detectTimer -= Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        animator.Play(IdleAnimHash);
        rb.simulated = true;
        isSpinning = false;

        onComplete?.Invoke();
    }

    public void Bounce(Action onComplete)
    {
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
        }

        if (LastHitTarget == null)
        {
            onComplete?.Invoke();
            return;
        }

        bounceCoroutine = StartCoroutine(BounceCo(onComplete));
    }

    private IEnumerator BounceCo(Action onComplete)
    {
        DetachFromParent();
        isBouncing = true;

        HashSet<Entity> bouncedTargets = new();

        int bouncesDone = 0;
        
        ICombatable combatable = Owner as ICombatable;
        Entity currentTarget = LastHitTarget;
        bouncedTargets.Add(currentTarget);

        while (bouncesDone < BounceCount)
        {
            var targets = detectorOnBounce.DetectTargets();

            Entity bestTarget = null;
            int bestScore = 0;
            float nearestDistanceSqr = float.MaxValue;
            foreach (var targetCollider in targets)
            {
                var hitEntity = targetCollider.GetComponentInChildren<Entity>();
                if (hitEntity == currentTarget || hitEntity is not ICombatable hitCombatable || hitCombatable.CombatSystem.IsDead)
                {
                    continue;
                }

                int score = 0;

                float distanceSqr = (hitEntity.CenterPosition - currentTarget.CenterPosition).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    score += 5;
                }

                if (!bouncedTargets.Contains(hitEntity))
                {
                    score += 10;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = hitEntity;
                    nearestDistanceSqr = distanceSqr;
                }
            }

            if (bestTarget == null)
            {
                break;
            }

            float distanceToBestSqr = (bestTarget.CenterPosition - (Vector2)transform.position).sqrMagnitude;
            while (distanceToBestSqr > 0.1f)
            {
                Vector2 directionToTarget = bestTarget.CenterPosition - (Vector2)transform.position;
                float distanceToTarget = directionToTarget.magnitude;
                directionToTarget /= distanceToTarget;

                rb.linearVelocity = directionToTarget * bounceSpeed;
                distanceToBestSqr = distanceToTarget * distanceToTarget;

                yield return null;
            }

            var bestTargetCombatable = bestTarget as ICombatable;

            var damage = RPG.CalcDamage(combatable, bestTargetCombatable);

            bestTargetCombatable.CombatSystem.TakeDamage(damage);
            bestTargetCombatable.CombatSystem.Knockback(transform.position, 3f, 0.1f);

            currentTarget = bestTarget;
            bouncedTargets.Add(currentTarget);
            bouncesDone++;
        }

        isBouncing = false;
        rb.linearVelocity = Vector2.zero;

        onComplete?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isStucked || isReturning || isSpinning || isBouncing)
        {
            return;
        }

        LayerMask layerMask = 1 << collision.gameObject.layer;

        bool canHit = (layerMask & hitLayers) != 0;

        if (canHit)
        {
            ICombatable combatable = Owner as ICombatable;

            Entity hitEntity = collision.GetComponentInChildren<Entity>();
            if (hitEntity is ICombatable hitCombatable)
            {
                var damage = RPG.CalcDamage(combatable, hitCombatable);

                hitCombatable.CombatSystem.TakeDamage(damage);
                hitCombatable.CombatSystem.Knockback(transform.position, 3f, 0.1f);

                LastHitTarget = hitEntity;
            }
        }

        bool isCantPierceLayer = (layerMask & cantPierceLayers) != 0;
        bool notEnoughPierceLeft = canHit && currentPierceCount >= PierceCount;

        if (isCantPierceLayer || notEnoughPierceLeft)
        {
            AttachTo(collision.transform);
            OnStuck?.Invoke();
            OnStuck = null;

            var vfx = RPG.VFXSys.SpawnVFX(Local.GetVFXPath(hitVFX));
            vfx.transform.position = transform.position;
        }
        else if (canHit)
        {
            currentPierceCount++;
        }
    }

    private void AttachTo(Transform target)
    {
        rb.simulated = false;
        isStucked = true;
        attacher.Attach(target);
        attacher.OnLooseTarget += DetachFromParent;
    }

    private void DetachFromParent()
    {
        attacher.OnLooseTarget -= DetachFromParent;
        attacher.Detach();
        isStucked = false;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
    }
}
