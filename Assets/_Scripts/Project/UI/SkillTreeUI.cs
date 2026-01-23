using System.Linq;
using TMPro;
using UnityEngine;

public class SkillTreeUI : PopupUI
{
    [SerializeField] private SkillTreeBuilder skillTreeBuilder;
    [SerializeField] private UILineRenderer lineUIRenderer;
    [SerializeField, Reference("Text_SkillPoints")] private TMP_Text skillPointsText;
    [SerializeField, Reference("")] private SkillTreeNodeDisplayer[] skillTreeNodeDisplayers;
    [SerializeField, Reference("SkillToolTip")] private SkillToolTip toolTip;

    private SkillTree skillTree;
    private Player player;
    private SkillSystem skillSystem;

    public override void OnOpen(Transform parent)
    {
        base.OnOpen(parent);

        skillTree = skillTreeBuilder.Build();
        foreach (var displayer in skillTreeNodeDisplayers)
        {
            var node = skillTree.GetNodeBySkillData(displayer.SkillData);
            if (node == null)
            {
                continue;
            }

            foreach (var child in node.Children)
            {
                var childDisplayer = skillTreeNodeDisplayers.FirstOrDefault(d => d.SkillData == child.SkillData);
                if (childDisplayer == null)
                {
                    continue;
                }

                lineUIRenderer.AddLineFromScreen(displayer.transform.position, childDisplayer.transform.position);
            }
        }
    }

    public void Setup(Player player)
    {
        this.player = player;
        skillSystem = RPG.UserDataSys.GetTable<UserPlayDataTable>().SkillSys;

        UpdateSkillPointsText();
        UpdateSkillTreeNodeDisplayers();
    }

    private void UpdateSkillTreeNodeDisplayers()
    {
        var cetificateArgs = new PlayerSkillCertificateArguments(player);
        foreach (var displayer in skillTreeNodeDisplayers)
        {
            var node = skillTree.GetNodeBySkillData(displayer.SkillData);
            if (node == null)
            {
                continue;
            }

            displayer.Setup(node, HandleOnClickNodeDisplayer, HandleOnPointerEnterNodeDisplayer, HandleOnPointerExitNodeDisplayer);
            displayer.SetUnlocked(node.IsUnlocked(cetificateArgs));
        }
    }

    private void HandleOnClickNodeDisplayer(SkillTreeNodeDisplayer displayer)
    {
        if (skillSystem.AvailableSkillPoints <= 0)
        {
            return;
        }

        var node = displayer.Node;
        var cetificateArgs = new PlayerSkillCertificateArguments(player);

        if (node.IsUnlocked(cetificateArgs))
        {
            return;
        }

        if (!node.CanUnlock(cetificateArgs))
        {
            return;
        }

        node.Unlock(cetificateArgs);
        displayer.SetUnlocked(true);
    }

    private void HandleOnPointerEnterNodeDisplayer(SkillTreeNodeDisplayer displayer)
    {
        toolTip.Setup(displayer.SkillData);
        toolTip.Show(displayer.transform as RectTransform);
    }

    private void HandleOnPointerExitNodeDisplayer(SkillTreeNodeDisplayer displayer)
    {
        toolTip.Hide();
    }

    private void UpdateSkillPointsText()
    {
        skillPointsText.text = $"Skill Points: {skillSystem.AvailableSkillPoints}";
    }
}