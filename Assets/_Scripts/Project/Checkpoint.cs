using UnityEngine;

public class Checkpoint : MonoBehaviour, IInteractable
{
    private static readonly int IdleAnimatorHash = Animator.StringToHash("Idle");
    private static readonly int ActiveAnimatorHash = Animator.StringToHash("Active");

    [SerializeField] private UUID checkpointID;

    private Animator animator;
    private AudioSource audioSource;

    private CheckpointSystem checkpointSystem;

    public UUID CheckpointID => checkpointID;

    private void Awake()
    {
        if (!checkpointID.IsValid())
        {
            checkpointID.Generate();
        }

        animator = GetComponentInChildren<Animator>();
    }

    private void UpdateAnimator()
    {
        if (checkpointSystem.LastCheckpointID == checkpointID)
        {
            animator.Play(ActiveAnimatorHash);
        }
        else
        {
            animator.Play(IdleAnimatorHash);
        }
    }

    private void UpdateAudio()
    {
        if (checkpointSystem.LastCheckpointID == checkpointID)
        {
            if (audioSource != null)
            {
                return;
            }

            var sfxParams = new SFXExtentionParams
            {
                minDistance = -1f,
                maxDistance = -1f,
                randomPitch = true,
                randomStartTime = false
            };

            audioSource = RPG.AudioSys.PlayLoopSFX(Local.GetSFXPath(SFXID.FireBurn), transform.position, 0.3f, sfxParams);
        }
        else
        {
            if (audioSource == null)
            {
                return;
            }

            RPG.AudioSys.StopLoopSFX(audioSource);
            audioSource = null;
        }
    }

    private void OnEnable()
    {
        checkpointSystem = RPG.UserDataSys.PlayData.Checkpoint;

        UpdateAnimator();
        UpdateAudio();

        checkpointSystem.OnCheckpointChanged += HandleOnCheckpointChanged;
    }

    private void HandleOnCheckpointChanged()
    {
        UpdateAnimator();
        UpdateAudio();
    }

    private void OnDisable()
    {
        if (checkpointSystem == null)
        {
            return;
        }

        if (audioSource != null)
        {
            RPG.AudioSys.StopLoopSFX(audioSource);
            audioSource = null;
        }

        checkpointSystem.OnCheckpointChanged -= HandleOnCheckpointChanged;
    }

    public void Interact(Player player)
    {
        if (!player.IsLocalPlayer())
        {
            return;
        }

        if (checkpointSystem == null)
        {
            return;
        }

        checkpointSystem.LastCheckpointID = checkpointID;

        UpdateAnimator();
        UpdateAudio();
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        #if UNITY_EDITOR
        var allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        foreach (var checkpoint in allCheckpoints)
        {
            if (checkpoint == this)
            {
                continue;
            }

            if (checkpoint.checkpointID == checkpointID)
            {
                checkpointID.Generate();
                UnityEditor.EditorUtility.SetDirty(this);
                break;
            }
        }

        if (!checkpointID.IsValid())
        {
            checkpointID.Generate();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif
    }
}
