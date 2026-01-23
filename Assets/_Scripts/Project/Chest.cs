using UnityEngine;

public class Chest : Entity, IInteractable
{
    private static readonly int OpenAnimHash = Animator.StringToHash("Open");

    private bool isOpened = false;

    private Rigidbody2D rigidBody;
    private EntityDropSystem dropSystem;


    protected override void Awake()
    {
        base.Awake();

        rigidBody = GetComponent<Rigidbody2D>();
        dropSystem = GetComponent<EntityDropSystem>();
    }

    public void Interact(Player player)
    {
        if (isOpened)
        {
            return;
        }

        rigidBody.linearVelocity = new Vector2(0, 5.0f);
        Animator.Play(OpenAnimHash);

        dropSystem.DropItems(player);

        isOpened = true;
    }
}