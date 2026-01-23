using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private Collider2D pickupCollider;

    private SpriteRenderer spriteRenderer;
    public Rigidbody2D RigidBody { get; private set; }

    private Item itemIsntance;

    public Item Item => itemIsntance;

    private void Awake()
    {
        if (itemData != null)
        {
            itemIsntance = itemData.CreateItem();
        }

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        RigidBody = GetComponent<Rigidbody2D>();
    }

    public void SetItem(Item item)
    {
        itemData = item.ItemData;
        itemIsntance = item;
        itemIsntance = itemData.CreateItem();
        spriteRenderer.sprite = itemData.Icon;
    }

    public void SetActivePickup(bool isActive)
    {
        pickupCollider.enabled = isActive;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (itemIsntance == null)
        {
            return;
        }

        var hasInventory = collision.GetComponent<IHasInventory>();
        if (hasInventory == null)
        {
            return;
        }

        if (!hasInventory.InventorySystem.CanAddItem(itemIsntance))
        {
            Logger.Info("No available slot to pick up item: " + itemData.DisplayName);
            return;
        }

        hasInventory.InventorySystem.AddItem(itemIsntance);
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        if (itemData == null)
        {
            return;
        }

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = itemData.Icon;
        }
    }
}