using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TimeText : MonoBehaviour
{
    public enum TimeFormat
    {
        Seconds,
        Minutes,
        Hours,
        MinutesSeconds,
        MinutesSecondsMilliseconds,
        HoursMinutesSeconds,
        HoursMinutesSecondsMilliseconds
    }

    public TimeFormat timeFormat = TimeFormat.Seconds;

    TextMeshProUGUI timeText;
    float currentTimeInSeconds = 0f;

    private void Awake()
    {
        timeText = GetComponent<TextMeshProUGUI>();
    }

    public void SetTimeFormat(TimeFormat format)
    {
        timeFormat = format;
        timeText.text = FormatTime(currentTimeInSeconds, timeFormat);
    }

    public void SetTime(float timeInSeconds)
    {
        currentTimeInSeconds = timeInSeconds;
        timeText.text = FormatTime(timeInSeconds, timeFormat);
    }

    private string FormatTime(float timeInSeconds, TimeFormat format)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000f) % 1000f);
        switch (format)
        {
            case TimeFormat.Seconds:
                return string.Format("{0:00}", seconds);
            case TimeFormat.Minutes:
                return string.Format("{0:00}", minutes);
            case TimeFormat.Hours:
                return string.Format("{0:00}", hours);
            case TimeFormat.MinutesSeconds:
                return string.Format("{0:00}:{1:00}", minutes, seconds);
            case TimeFormat.MinutesSecondsMilliseconds:
                return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
            case TimeFormat.HoursMinutesSeconds:
                return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            case TimeFormat.HoursMinutesSecondsMilliseconds:
                return string.Format("{0:00}:{1:00}:{2:00}:{3:000}", hours, minutes, seconds, milliseconds);
            default:
                return "";
        }
    }
}