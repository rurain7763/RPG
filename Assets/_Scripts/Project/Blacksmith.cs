using System;
using System.Collections.Generic;
using UnityEngine;

public class Blacksmith : NPC, IInteractable
{
    private PooledObject interactionButtonTextObj;

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (interactionButtonTextObj != null)
        {
            DialogueTextPool.ReleaseObject(interactionButtonTextObj);
            interactionButtonTextObj = null;
        }
    }

    public override void End()
    {
        base.End();

        if (interactionButtonTextObj != null)
        {
            DialogueTextPool.ReleaseObject(interactionButtonTextObj);
            interactionButtonTextObj = null;
        }
    }

    protected override void OnPlayerInSurroundings(Player player)
    {
        LookAt(player.CenterPosition);

        if (interactionButtonTextObj == null)
        {
            interactionButtonTextObj = DialogueTextPool.GetObject();

            var floatingText = interactionButtonTextObj.GetComponent<DialogueText>();
            floatingText.SetText("E");
            floatingText.SetAnchor(CenterAnchor);
        }

        if (!IsQuestIndicatorActive())
        {
            ShowRandomDialogueText();
        }
    }

    protected override void OnPlayerStayInSurroundings(Player player)
    {
        LookAt(player.CenterPosition);
    }

    protected override void OnPlayerOutOfSurroundings(Player player)
    {
        if (interactionButtonTextObj != null)
        {
            DialogueTextPool.ReleaseObject(interactionButtonTextObj);
            interactionButtonTextObj = null;
        }

        HideDialogueText();
    }

    public void Interact(Player player)
    {
        var playDataTable = RPG.UserDataSys.GetTable<UserPlayDataTable>(player.UserID);
        if (playDataTable == null)
        {
            Logger.Error($"Failed to retrieve UserPlayDataTable for user ID {player.UserID}");
            return;
        }

        var customActions = new List<(string, Action)>()
        {
            ("Open storage", () => RPG.UISys.OpenPopup<BlacksmithUI>().Setup(playDataTable.StorageInventory, player.InventorySystem))
        };

        OpenDialogueUI(player, customActions);
    }
}