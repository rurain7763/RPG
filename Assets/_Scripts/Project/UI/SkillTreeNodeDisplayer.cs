using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillTreeNodeDisplayer : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private SkillDataObject skillData;
    [SerializeField, Reference("Icon")] private Image iconImage;
    [SerializeField, Reference("Image_Lock")] private Image lockImage;

    private bool isUnlocked = false;
    private Action<SkillTreeNodeDisplayer> onPointerDownAction;
    private Action<SkillTreeNodeDisplayer> onPointerEnterAction;
    private Action<SkillTreeNodeDisplayer> onPointerExitAction;

    public SkillTreeNode Node { get; private set; }
    public SkillDataObject SkillData => skillData;

    public void Setup(SkillTreeNode node, Action<SkillTreeNodeDisplayer> onPointerDown, Action<SkillTreeNodeDisplayer> onPointerEnter, Action<SkillTreeNodeDisplayer> onPointerExit)
    {
        Node = node;
        onPointerDownAction = onPointerDown;
        onPointerEnterAction = onPointerEnter;
        onPointerExitAction = onPointerExit;
    }

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        if (lockImage != null)
        {
            lockImage.gameObject.SetActive(!isUnlocked);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDownAction?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnterAction?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExitAction?.Invoke(this);
    }

    private void OnValidate()
    {
        if (skillData == null)
        {
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = skillData.Icon;
        }
    }
}