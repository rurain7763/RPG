using System;
using System.Collections;
using UnityEngine;

public class Damage
{
    public readonly EntityCombatSystem Source;
    public readonly EntityCombatSystem Destination;

    public float PhysicalAmount;
    public bool IsCritical;

    public ElementType ElementType;
    public float ElementalAmount;

    public bool IsEnvironmental => Source == null;
    public float TotalAmount => PhysicalAmount + ElementalAmount;

    public Damage(EntityCombatSystem destination)
        : this(null, destination)
    {
    }

    public Damage(EntityCombatSystem source, EntityCombatSystem destination)
    {
        Source = source;
        Destination = destination;
    }
}

public class Healing
{
    public readonly EntityCombatSystem Source;
    public readonly EntityCombatSystem Destination;

    public float Amount;
    public bool IsEnvironmental => Source == null;

    public Healing(EntityCombatSystem destination)
        : this(null, destination)
    {
    }

    public Healing(EntityCombatSystem source, EntityCombatSystem destination)
    {
        Source = source;
        Destination = destination;
    }
}

public class EntityCombatSystem : MonoBehaviour
{
    const float HealthRegenerationInterval = 1f;

    [SerializeField] private LayerMask allyMask;
    [SerializeField] private LayerMask enemyMask;

    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    private float healthRegenerationTimer;

    private Rigidbody2D rb;

    private Coroutine nockbackCoroutine;
    private Coroutine airborneCoroutine;
    private Coroutine stunCoroutine;

    public Entity Owner { get; private set; }

    public LayerMask AllyMask => allyMask;
    public LayerMask EnemyMask => enemyMask;

    public float MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
    }

    public float HealthRegeneration { get; set; }

    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsKnockbacked { get; private set; }
    public bool IsAirborne { get; private set; }
    public float StunDuration { get; private set; }
    public bool IsStunned => StunDuration > 0f;
    public bool ActiveCounter { get; set; }
    public bool ActiveKnockbackImmunity { get; set; }
    public bool ActiveAirborneImmunity { get; set; }
    public bool ActiveStunImmunity { get; set; }

    public event Action OnDie;
    public event Action OnHealthChanged;
    public event Action<Damage> OnDealDamage;
    public event Action<Damage> OnTakeDamage;
    public event Action<Healing> OnTakeHeal;

    private void Awake()
    {
        IsKnockbacked = false;
        IsAirborne = false;

        Owner = GetComponent<Entity>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Tick(float delta)
    {
        if (IsDead)
        {
            return;
        }

        if (HealthRegeneration > 0f)
        {
            healthRegenerationTimer += delta;
            if (healthRegenerationTimer >= HealthRegenerationInterval)
            {
                healthRegenerationTimer = 0f;
                currentHealth = Mathf.Min(currentHealth + HealthRegeneration, MaxHealth);
                OnHealthChanged?.Invoke();
            }
        }
    }

    public bool ChanceToCritical(float criticalRate)
    {
        return UnityEngine.Random.Range(0f, 1f) < criticalRate;
    }

    public bool ChanceToEvasion(float evasionRate)
    {
        return UnityEngine.Random.Range(0f, 1f) < evasionRate;
    }

    public float CalcCriticalDamage(float amount, float critPower)
    {
        return amount * (1f + critPower);
    }

    public float CalcDamageAfterArmor(float amount, float armor, float armorConstant, float armorReduction)
    {
        float ar = 1.0f - armorReduction;
        float reducedArmor = armor * ar;
        return amount * (1.0f - (reducedArmor / (reducedArmor + armorConstant)));
    }

    public float CalcDamageAfterResistance(float amount, float resistance)
    {
        return amount * (1.0f - resistance);
    }

    public Frozen GetFrozenBuff(float duration, float iceResistance, ICombatable source)
    {
        float adjustedDuration = duration * (1f - iceResistance);
        if (adjustedDuration <= 0f)
        {
            return null;
        }
        return new Frozen(adjustedDuration, source);
    }

    public Burning GetBurningBuff(float duration, float fireResistance, ICombatable source)
    {
        float damageTickRate = 0.02f;
        damageTickRate *= 1f - fireResistance;
        return new Burning(duration, damageTickRate, source);
    }

    public Electrified GetElectrifiedBuff(float duration, float lightningResistance, ICombatable source)
    {
        float maxChargeCount = 5;
        maxChargeCount = Mathf.RoundToInt(maxChargeCount * (1f + lightningResistance));
        return new Electrified(duration, (int)maxChargeCount, source);
    }

    public void SetHealthToMax()
    {
        currentHealth = MaxHealth;
        OnHealthChanged?.Invoke();
    }

    public void Die()
    {
        currentHealth = 0;
        OnDie?.Invoke();
    }

    public void TakeDamage(Damage damage)
    {
        if (IsDead)
        {
            return;
        }

        currentHealth = Mathf.Max(currentHealth - damage.TotalAmount, 0);
        OnTakeDamage?.Invoke(damage);
        OnHealthChanged?.Invoke();

        if (!damage.IsEnvironmental)
        {
            damage.Source.OnDealDamage?.Invoke(damage);
        }

        if (IsDead)
        {
            OnDie?.Invoke();
        }
    }

    public void TakeHeal(Healing healing)
    {
        if (IsDead)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + healing.Amount, MaxHealth);
        OnTakeHeal?.Invoke(healing);
        OnHealthChanged?.Invoke();
    }

    public void Knockback(Vector2 origin, float force, float duration)
    {
        if (ActiveKnockbackImmunity)
        {
            return;
        }

        if (rb == null)
        {
            return;
        }

        if (nockbackCoroutine != null)
        {
            StopCoroutine(nockbackCoroutine);
        }

        if (origin.x >= transform.position.x)
        {
            force = -Mathf.Abs(force);
        }
        else
        {
            force = Mathf.Abs(force);
        }

        nockbackCoroutine = StartCoroutine(KnockbackCo(force, duration));
    }

    private IEnumerator KnockbackCo(float force, float duration)
    {
        IsKnockbacked = true;
        rb.linearVelocity = new Vector2(force, rb.linearVelocity.y);
        yield return new WaitForSeconds(duration);
        IsKnockbacked = false;
        nockbackCoroutine = null;
    }

    public void Airborne(float force, float duration)
    {
        if (ActiveAirborneImmunity)
        {
            return;
        }

        if (rb == null)
        {
            return;
        }

        if (airborneCoroutine != null)
        {
            StopCoroutine(airborneCoroutine);
        }

        airborneCoroutine = StartCoroutine(AirborneCo(force, duration));
    }

    private IEnumerator AirborneCo(float force, float duration)
    {
        IsAirborne = true;
        rb.linearVelocity = new Vector2(0, force);
        yield return new WaitForSeconds(duration);
        IsAirborne = false;
        airborneCoroutine = null;
    }

    public void Stun(float duration)
    {
        if (ActiveStunImmunity)
        {
            return;
        }

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        
        stunCoroutine = StartCoroutine(StunCo(duration));
    }

    private IEnumerator StunCo(float duration)
    {
        StunDuration = duration;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        while (StunDuration > 0f)
        {
            StunDuration -= Time.deltaTime;
            yield return null;
        }

        StunDuration = 0f;
        stunCoroutine = null;
    }
}
