using UnityEngine;
using UnityEngine.UI;

public enum QuestIndicatorType
{
    QuestAvailable,
    QuestInProgress,
    QuestComplete
}

public class QuestIndicator : MonoBehaviour
{
    [SerializeField] private Image iconRenderer;
    [SerializeField] private Sprite questAvailableIcon;
    [SerializeField] private Sprite questInProgressIcon;
    [SerializeField] private Sprite questCompleteIcon;
    [SerializeField] private WorldToUIFollower worldToUIFollower;

    public void SetIcon(QuestIndicatorType type)
    {
        switch (type)
        {
            case QuestIndicatorType.QuestAvailable:
                iconRenderer.sprite = questAvailableIcon;
                break;
            case QuestIndicatorType.QuestInProgress:
                iconRenderer.sprite = questInProgressIcon;
                break;
            case QuestIndicatorType.QuestComplete:
                iconRenderer.sprite = questCompleteIcon;
                break;
            default:
                iconRenderer.sprite = null;
                break;
        }
    }

    public void SetAnchor(Transform anchor)
    {
        worldToUIFollower.SetAnchor(anchor);
    }
}