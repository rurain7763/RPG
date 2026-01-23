using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class IntegralNumberText : MonoBehaviour
{
    private const float AnimationDuration = 0.5f;

    private TMP_Text _textComponent;

    private double _startNumber = 0;
    private double _targetNumber = 0;
    private double _currentNumber = 0;

    private float _animationTime = 0f;

    private void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
        _textComponent.text = "0";
    }

    private void Update()
    {
        if (_animationTime >= AnimationDuration)
        {
            return;
        }

        _animationTime += Time.deltaTime;

        float t = Mathf.Clamp01(_animationTime / AnimationDuration);

        _currentNumber = _startNumber + (_targetNumber - _startNumber) * t;

        if (t >= 1.0f)
        {
            _currentNumber = _targetNumber;
            _animationTime = AnimationDuration;
        }

        ulong displayValue = (ulong)_currentNumber;

        _textComponent.text = displayValue.ToString("N0");
    }

    public void SetNumber(ulong number)
    {
        _startNumber = _currentNumber;
        _targetNumber = number;
        _animationTime = 0f;
    }
}