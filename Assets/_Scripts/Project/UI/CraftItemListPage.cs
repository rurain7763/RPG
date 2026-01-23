using Gpm.Ui;
using UnityEngine;

public class CraftItemListPage : TabPage
{
    [SerializeField, Reference("Scroll View")] private InfiniteScroll craftItemScroll;

    private CraftItemCategory category;
    private CraftItemCategoryTabData tabData;

    protected override void OnNotify(Tab tab)
    {
        base.OnNotify(tab);

        if (tab.IsSelected() == false)
        {
            return;
        }

        var categoryTab = tab as CraftItemCategoryTab;
        if (categoryTab == null)
        {
            return;
        }

        category = categoryTab.Category;

        tabData = categoryTab.GetData() as CraftItemCategoryTabData;
        if (tabData == null)
        {
            return;
        }

        UpdateCraftItemList();
    }

    private void UpdateCraftItemList()
    {
        craftItemScroll.Clear();

        foreach (var entry in tabData.ItemCraftingData.CraftingDataEntries)
        {
            if (entry.Category != category)
            {
                continue;
            }

            var scrollItemData = new CraftItemScrollItemData
            {
                craftingDataEntry = entry,
                OnClick = tabData.OnClick,
            };

            craftItemScroll.InsertData(scrollItemData);
        }
    }
}