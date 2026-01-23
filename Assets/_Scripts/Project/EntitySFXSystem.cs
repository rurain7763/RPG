using UnityEngine;

public class EntitySFXSystem : MonoBehaviour
{
    [SerializeField] private SFXID hitVFX;
    [SerializeField] private SFXID missVFX;

    private Entity owner;

    private void Awake()
    {
        owner = GetComponent<Entity>();
    }

    public void PlayHitSFX()
    {
        var sfxParams = new SFXExtentionParams
        {
            minDistance = -1f,
            maxDistance = -1f,
            randomPitch = true,
            randomStartTime = false
        };

        RPG.AudioSys.PlaySFX(Local.GetSFXPath(hitVFX), owner.CenterPosition, 1f, sfxParams);
    }

    public void PlayMissSFX()
    {
        var sfxParams = new SFXExtentionParams
        {
            minDistance = -1f,
            maxDistance = -1f,
            randomPitch = true,
            randomStartTime = false
        };

        RPG.AudioSys.PlaySFX(Local.GetSFXPath(missVFX), owner.CenterPosition, 1f, sfxParams);
    }
}