using System.Collections;
using UnityEngine;

public class EntityDropSystem : MonoBehaviour
{
    [SerializeField] private ItemDropData itemDropData;
    [SerializeField] private ItemPickup itemPickupPrefab;

    private Entity entity;

    private void Awake()
    {
        entity = GetComponent<Entity>();
    }

    public void DropItems(Entity to)
    {
        var droppedItems = itemDropData.GetRandomDropItems();

        const float anglePerItem = 10.0f;
        const float maxArcAngle = 160.0f;
        const float force = 6.5f;

        Helper.EachDirectionsOnArc2D(Vector2.up, anglePerItem, maxArcAngle, droppedItems.Count, (index, dir) => 
        {
            var item = droppedItems[index];
            var itemPickup = Instantiate(itemPickupPrefab, entity.CenterPosition, Quaternion.identity);
            itemPickup.SetItem(item);

            StartCoroutine(DropItemCo(to, itemPickup, dir, force, 1.5f));
        });
    }

    private IEnumerator DropItemCo(Entity to, ItemPickup itemPickup, Vector2 direction, float force, float delay)
    {
        itemPickup.SetActivePickup(false);

        itemPickup.RigidBody.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(delay);

        itemPickup.SetActivePickup(true);

        float sqrDist = (to.CenterPosition - (Vector2)itemPickup.transform.position).sqrMagnitude;
        while (sqrDist > 1.0f)
        {
            if (itemPickup == null)
            {
                yield break;
            }

            Vector2 dirToEntity = to.CenterPosition - (Vector2)itemPickup.transform.position;
            itemPickup.RigidBody.linearVelocity = dirToEntity.normalized * 13.0f;
            yield return null;
            sqrDist = dirToEntity.sqrMagnitude;
        }
    }
}