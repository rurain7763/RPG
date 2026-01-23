using System;
using UnityEngine;
using UnityEngine.Events;

public class VFX : MonoBehaviour
{
    [SerializeField] private bool autoDestroy = true;
    [SerializeField] private float destroyDelay = 0f;

    private float destroyTimer = 0f;
    private Action destroyFunc;

    public UnityEvent OnSpawn;
    public UnityEvent OnDestroyed;

    public void Init(Action destroyFunc)
    {
        this.destroyFunc = destroyFunc;
        if (autoDestroy)
        {
            destroyTimer = destroyDelay;
        }

        OnSpawn?.Invoke();
    }

    private void Update()
    {
        if (!autoDestroy)
        {
            return;
        }

        destroyTimer -= Time.deltaTime;
        if (destroyTimer <= 0f)
        {
            DestroySelf();
        }
    }

    public void DestroySelf()
    {
        OnDestroyed?.Invoke();
        destroyFunc.Invoke();
    }
}
