using System;
using System.Collections.Generic;
using UnityEngine;

public class Merchant : NPC, IInteractable
{
    [SerializeField] private ItemMerchantData merchantData;

    private PooledObject interactionButtonTextOject;
    private InventorySystem merchantInventory;

    protected override void Awake()
    {
        base.Awake();

        merchantInventory = new InventorySystem();
        using (merchantInventory.BeginTransaction())
        {
            for (int i = 0; i < RPG.MaxMerchantItemCount; i++)
            {
                var item = merchantData.GetRandomItem();
                merchantInventory.AddItem(item);
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (interactionButtonTextOject != null)
        {
            DialogueTextPool.ReleaseObject(interactionButtonTextOject);
            interactionButtonTextOject = null;
        }
    }

    public override void End()
    {
        base.End();

        if (interactionButtonTextOject != null)
        {
            DialogueTextPool.ReleaseObject(interactionButtonTextOject);
            interactionButtonTextOject = null;
        }
    }

    protected override void OnPlayerInSurroundings(Player player)
    {
        LookAt(player.CenterPosition);

        if (interactionButtonTextOject == null)
        {
            interactionButtonTextOject = DialogueTextPool.GetObject();

            var floatingText = interactionButtonTextOject.GetComponent<DialogueText>();
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
        if (interactionButtonTextOject != null)
        {
            DialogueTextPool.ReleaseObject(interactionButtonTextOject);
            interactionButtonTextOject = null;
        }

        HideDialogueText();
    }

    public void Interact(Player player)
    {
        var customActions = new List<(string, Action)>()
        {
            ("npc_merchant_action_openshop", () => RPG.UISys.OpenPopup<MerchantUI>().Setup(merchantData, merchantInventory, player.InventorySystem))
        };

        OpenDialogueUI(player, customActions);
    }
}