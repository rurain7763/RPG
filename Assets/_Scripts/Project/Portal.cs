using UnityEngine;

public class Portal : MonoBehaviour, IInteractable
{
    [SerializeField] private bool isTownPortal;

    private PortalSystem portalSystem;

    private bool launched;
    private LevelID targetLevel;

    private void Start()
    {
        portalSystem = RPG.UserDataSys.PlayData.Portal;

        UpdateState();

        portalSystem.OnPortalChanged += UpdateState;
    }

    private void OnDestroy()
    {
        if (portalSystem != null)
        {
            portalSystem.OnPortalChanged -= UpdateState;
        }
    }

    private void UpdateState()
    {
        launched = false;

        Vector2 position = Vector2.zero;
        bool directionRight = true;

        if (isTownPortal)
        {
            launched = portalSystem.Launched;
            targetLevel = portalSystem.TargetLevelID;
            position = transform.position;
            directionRight = transform.eulerAngles.y == 0f;
        }
        else if (portalSystem.Launched)
        {
            launched = true;
            targetLevel = RPG.GetNearestTownLevelFromCurrentLevel();
            position = portalSystem.Position;
            directionRight = portalSystem.DirectionRight;
        }

        if (!launched)
        {
            transform.gameObject.SetActive(false);
            return;
        }

        transform.position = position;

        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.y = directionRight ? 0f : 180f;
        transform.eulerAngles = eulerAngles;

        transform.gameObject.SetActive(true);
    }

    public void Interact(Player player)
    {
        if (!launched)
        {
            return;
        }

        portalSystem.ResetPortal();

        RPG.LoadLevel(targetLevel, new PortalSpawnPolicy());
    }
}
