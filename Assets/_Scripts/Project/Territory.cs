using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Territory : MonoBehaviour
{
    private Coroutine expandCo;

    public float LifeTime { get; set; } = 5f;

    public event Action<Entity> OnEntityEnter;
    public event Action<Entity> OnEntityExit;

    private void Update()
    {
        if (LifeTime > 0f)
        {
            LifeTime -= Time.deltaTime;
            if (LifeTime <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }

    public void Expand(float targetSize, float duration)
    {
        if (expandCo != null)
        {
            StopCoroutine(expandCo);
        }

        expandCo = StartCoroutine(ExpandCo(targetSize, duration));
    }

    private IEnumerator ExpandCo(float targetSize, float duration)
    {
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(targetSize, targetSize, initialScale.z);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, Easing.OutSine(t));
            yield return null;
        }

        transform.localScale = targetScale;
        expandCo = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var entity = other.GetComponent<Entity>();
        if (entity != null)
        {
            OnEntityEnter?.Invoke(entity);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var entity = other.GetComponent<Entity>();
        if (entity != null)
        {
            OnEntityExit?.Invoke(entity);
        }
    }
}
