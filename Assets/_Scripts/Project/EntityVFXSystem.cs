using System.Collections;
using UnityEngine;

public class EntityVFXSystem : MonoBehaviour
{
    [SerializeField] private VFXID hitVFX;
    [SerializeField] private VFXID fireHitVFX;
    [SerializeField] private VFXID lightingHitVFX;
    [SerializeField] private VFXID iceHitVFX;
    [SerializeField] private VFXID criticalHitVFX;
    [SerializeField] private VFXID echoVFX;
    [SerializeField] private VFXID deathVFX;

    private Entity owner;

    private SpriteRenderer spriteRenderer;
    private EntityCombatSystem combatSystem;

    private Coroutine takeDamageCoroutine;

    private void Awake()
    {
        owner = GetComponent<Entity>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.color = Color.white;

        if (owner is ICombatable combatable)
        {
            combatSystem = combatable.CombatSystem;
        }
    }

    private void Start()
    {
        if (combatSystem != null)
        {
            combatSystem.OnTakeDamage += HandleTakeDamage;
        }
    }

    private void HandleTakeDamage(Damage damage)
    {
        if (takeDamageCoroutine != null)
        {
            StopCoroutine(takeDamageCoroutine);
        }

        takeDamageCoroutine = StartCoroutine(HandleTakeDamageCo(damage.ElementType));
    }

    private IEnumerator HandleTakeDamageCo(ElementType elementType)
    {
        Color originalColor = Color.white;

        Color targetColor;
        if (elementType == ElementType.Ice)
        {
            targetColor = Color.cyan;
        }
        else if (elementType == ElementType.Fire)
        {
            targetColor = Color.red;
        }
        else if (elementType == ElementType.Lightning)
        {
            targetColor = Color.yellow;
        }
        else
        {
            targetColor = Color.gray;
        }

        spriteRenderer.color = targetColor;
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = originalColor;
        takeDamageCoroutine = null;
    }

    public void SpawnHitVFX(Entity target, ElementType elementType = ElementType.Physical)
    {
        Vector2 spawnPosition = target.CenterPosition;
        Vector2 rndOffset = Random.insideUnitCircle * 0.5f;

        VFXID vfxId = elementType switch
        {
            ElementType.Ice => iceHitVFX,
            ElementType.Fire => fireHitVFX,
            ElementType.Lightning => lightingHitVFX,
            _ => hitVFX,
        };

        VFX vfx = RPG.VFXSys.SpawnVFX(Local.GetVFXPath(vfxId));
        vfx.transform.position = spawnPosition + rndOffset;
    }

    public void SpawnCriticalHitVFX(Entity target)
    {
        Vector2 spawnPosition = target.CenterPosition;
        float rndYOffset = Random.Range(-0.5f, 0.5f);
        VFX vfx = RPG.VFXSys.SpawnVFX(Local.GetVFXPath(VFXID.CriticalHit));
        vfx.transform.position = spawnPosition + new Vector2(0f, rndYOffset);
        if (owner.IsFacingRight)
        {
            vfx.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
        else
        {
            vfx.transform.eulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

    public void SpawnImageEchoVFX(int echoCount = 1, float duration = 0)
    {
        StartCoroutine(SpawnImageEchoVFXCo(echoCount, duration));
    }

    private IEnumerator SpawnImageEchoVFXCo(int echoCount, float duration)  
    {
        float interval = duration / echoCount;
        for (int i = 0; i < echoCount; i++)
        {
            VFX vfx = RPG.VFXSys.SpawnVFX(Local.GetVFXPath(echoVFX));
            vfx.transform.position = owner.transform.position;
            vfx.transform.localScale = owner.transform.localScale;
            vfx.transform.rotation = owner.transform.rotation;
            
            SpriteRenderer vfxSpriteRenderer = vfx.GetComponentInChildren<SpriteRenderer>();
            if (vfxSpriteRenderer != null)
            {
                vfxSpriteRenderer.sprite = spriteRenderer.sprite;
                vfxSpriteRenderer.flipX = spriteRenderer.flipX;
                vfxSpriteRenderer.flipY = spriteRenderer.flipY;
                vfxSpriteRenderer.transform.localPosition = spriteRenderer.transform.localPosition;
                vfxSpriteRenderer.transform.localScale = spriteRenderer.transform.localScale;
            }

            yield return new WaitForSeconds(interval);
        }
    }

    public void SpawnDeathVFX()
    {
        VFX vfx = RPG.VFXSys.SpawnVFX(Local.GetVFXPath(deathVFX));
        vfx.transform.position = owner.CenterPosition;
    }
}
