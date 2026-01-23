using UnityEngine;

public class FloatingEffector : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.5f;
    [SerializeField] private float frequency = 1f;

    private Vector3 startPosition;
    
    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float yOffset = amplitude * Mathf.Sin(Time.time * frequency * 2 * Mathf.PI);
        transform.position = startPosition + new Vector3(0, yOffset, 0);
    }
}