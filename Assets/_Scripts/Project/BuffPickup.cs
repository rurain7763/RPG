using UnityEngine;

public class BuffPickup : MonoBehaviour
{
    [SerializeReference, SubclassSelector] private RPGBuff buff;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (buff == null)
        {
            return;
        }

        var combatable = collision.GetComponent<ICombatable>();
        if (combatable == null)
        {
            return;
        }

        combatable.BuffSystem.AddBuff(buff);

        Destroy(gameObject);
    }
}
