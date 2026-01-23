using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UserPlayDataTable : UserDataTable
{
    private RPGAppDataSystem appDataSys;
    private string filePath;

    public ulong Gold { get; set; }
    public InventorySystem Inventory { get; set; }
    public InventorySystem StorageInventory { get; set; }
    public EquipmentSystem Equipment { get; set; }
    public QuestSystem QuestSystem { get; set; }
    public CheckpointSystem Checkpoint { get; set; }
    public PortalSystem Portal { get; set; }
    public ProgressSystem Progress { get; set; }
    public OptionSystem Options { get; set; }
    public SkillSystem SkillSys { get; set; }

    public UserPlayDataTable(RPGAppDataSystem appDataSys, string basePath)
    {
        this.appDataSys = appDataSys;
        filePath = Path.Combine(basePath, "UserPlayData.json");
    }

    public override void Update()
    {
        try
        {
            UserPlayDataDTO playData = new();

            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string jsonData = reader.ReadToEnd();
                    JsonUtility.FromJsonOverwrite(jsonData, playData);
                }
            }

            UpdateCurrency(playData);
            UpdateInventory(playData);
            UpdateStorageInventory(playData);
            UpdateEquipment(playData);
            UpdateQuestSystem(playData);
            UpdateCheckpointSystem(playData);
            UpdatePortalSystem(playData);
            UpdateProgressSystem(playData);
            UpdateOptionsSystem(playData);
            UpdateSkillSystem(playData);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to update user play data: {e.Message}");
        }
    }

    private void UpdateCurrency(UserPlayDataDTO dto)
    {
        Gold = dto.Gold;
    }

    private void UpdateInventory(UserPlayDataDTO dto)
    {
        InventorySystemInitData initData = new()
        {
            SlotCapacity = RPG.MaxPlayerInventorySlots,
            InitialItems = new List<Item>()
        };

        foreach (var itemDTO in dto.Items)
        {
            var itemData = appDataSys.AppData.GetItemData(itemDTO.ID);
            if (itemData == null)
            {
                Logger.Warn($"ItemData with ID {itemDTO.ID} not found. Skipping item.");
                continue;
            }

            var item = itemData.CreateItem(itemDTO.SerialNumber);
            if (item == null)
            {
                Logger.Warn($"Failed to create Item from ItemData with ID {itemDTO.ID}. Skipping item.");
                continue;
            }

            initData.InitialItems.Add(item);
        }

        Inventory = new InventorySystem(initData);
    }

    private void UpdateStorageInventory(UserPlayDataDTO dto)
    {
        InventorySystemInitData initData = new()
        {
            SlotCapacity = RPG.MaxPlayerStorageInventorySlots,
            InitialItems = new List<Item>()
        };

        foreach (var itemDTO in dto.StorageItems)
        {
            var itemData = appDataSys.AppData.GetItemData(itemDTO.ID);
            if (itemData == null)
            {
                Logger.Warn($"ItemData with ID {itemDTO.ID} not found. Skipping item.");
                continue;
            }

            var item = itemData.CreateItem(itemDTO.SerialNumber);
            if (item == null)
            {
                Logger.Warn($"Failed to create Item from ItemData with ID {itemDTO.ID}. Skipping item.");
                continue;
            }

            initData.InitialItems.Add(item);
        }

        StorageInventory = new InventorySystem(initData);
    }

    private void UpdateEquipment(UserPlayDataDTO dto)
    {
        EquipmentSystemInitData initData = new()
        {
            Weapon = null,
            Armor = null,
            FirstTrinket = null,
            SecondTrinket = null
        };

        foreach (var equippedItemDTO in dto.EquippedItems)
        {
            var slotType = equippedItemDTO.SlotType;
            var itemDTO = equippedItemDTO.Item;

            var itemData = appDataSys.AppData.GetItemData(itemDTO.ID) as EquipmentItemData;
            if (itemData == null)
            {
                Logger.Warn($"EquipmentItemData with ID {itemDTO.ID} not found or is not an equipment item. Skipping item.");
                continue;
            }
            var item = itemData.CreateItem(itemDTO.SerialNumber) as EquipmentItem;
            if (item == null)
            {
                Logger.Warn($"Failed to create EquipmentItem from ItemData with ID {itemDTO.ID}. Skipping item.");
                continue;
            }

            if (slotType == EquipmentSlotType.Weapon)
            {
                initData.Weapon = item;
            }
            else if (slotType == EquipmentSlotType.Armor)
            {
                initData.Armor = item;
            }
            else if (slotType == EquipmentSlotType.FirstTrinket)
            {
                initData.FirstTrinket = item;
            }
            else if (slotType == EquipmentSlotType.SecondTrinket)
            {
                initData.SecondTrinket = item;
            }
        }

        Equipment = new EquipmentSystem(initData);
    }

    private void UpdateQuestSystem(UserPlayDataDTO dto)
    {
        QuestSystem = new QuestSystem(dto.QuestSystemData, id => appDataSys.AppData.GetQuestData(id));
    }

    private void UpdateCheckpointSystem(UserPlayDataDTO dto)
    {
        Checkpoint = new CheckpointSystem();
        Checkpoint.LastCheckpointID = dto.LastCheckpointID;
    }

    private void UpdatePortalSystem(UserPlayDataDTO dto)
    {
        var initData = new PortalSystemInitData
        {
            Launched = dto.PortalSystemData.Launched,
            TargetLevelID = dto.PortalSystemData.TargetLevelID,
            Position = dto.PortalSystemData.Position,
            DirectionRight = dto.PortalSystemData.DirectionRight
        };

        Portal = new PortalSystem(initData);
    }

    private void UpdateProgressSystem(UserPlayDataDTO dto)
    {
        var initData = new ProgressSystemInitData
        {
            LastLevelID = dto.ProgressData.LastLevelID,
            LastPosition = dto.ProgressData.LastPosition
        };

        Progress = new ProgressSystem(initData);
    }

    private void UpdateOptionsSystem(UserPlayDataDTO dto)
    {
        var initData = new OptionSystemInitData
        {
            BGMVolume = dto.OptionsData.BGMVolume,
            SFXVolume = dto.OptionsData.SFXVolume
        };

        Options = new OptionSystem(initData);
    }

    private void UpdateSkillSystem(UserPlayDataDTO dto)
    {
        SkillSys = new SkillSystem(dto.SkillSystemData);
    }

    public override void Upload()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            UserPlayDataDTO playData = new();
            UploadCurrency(ref playData);
            UploadInventory(ref playData);
            UploadStorageInventory(ref playData);
            UploadEquipment(ref playData);
            UploadQuestSystem(ref playData);
            UploadCheckpointSystem(ref playData);
            UploadPortalSystem(ref playData);
            UploadProgressSystem(ref playData);
            UploadOptionsSystem(ref playData);
            UploadSkillSystem(ref playData);

            string jsonData = JsonUtility.ToJson(playData, true);

            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.Write(jsonData);
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to upload user play data: {e.Message}");
        }
    }

    private void UploadCurrency(ref UserPlayDataDTO dto)
    {
        dto.Gold = Gold;
    }

    private void UploadInventory(ref UserPlayDataDTO dto)
    {
        List<ItemDTO> items = new();
        foreach (var slot in Inventory.Slots)
        {
            foreach (var item in slot.Items)
            {
                ItemDTO itemDTO = new ItemDTO
                {
                    ID = item.ItemData.ID,
                    SerialNumber = item.SerialNumber
                };
                items.Add(itemDTO);
            }
        }
        dto.Items = items.ToArray();
    }

    private void UploadStorageInventory(ref UserPlayDataDTO dto)
    {
        List<ItemDTO> items = new();
        foreach (var slot in StorageInventory.Slots)
        {
            foreach (var item in slot.Items)
            {
                ItemDTO itemDTO = new ItemDTO
                {
                    ID = item.ItemData.ID,
                    SerialNumber = item.SerialNumber
                };
                items.Add(itemDTO);
            }
        }
        dto.StorageItems = items.ToArray();
    }

    private void UploadEquipment(ref UserPlayDataDTO dto)
    {
        List<EquippedItemDTO> equippedItems = new();
        void AddEquippedItem(EquipmentSlotType slotType, EquipmentItem item)
        {
            if (item != null)
            {
                EquippedItemDTO equippedItemDTO = new EquippedItemDTO
                {
                    SlotType = slotType,
                    Item = new ItemDTO
                    {
                        ID = item.ItemData.ID,
                        SerialNumber = item.SerialNumber
                    }
                };
                equippedItems.Add(equippedItemDTO);
            }
        }
        AddEquippedItem(EquipmentSlotType.Weapon, Equipment.EquippedWeapon);
        AddEquippedItem(EquipmentSlotType.Armor, Equipment.EquippedArmor);
        AddEquippedItem(EquipmentSlotType.FirstTrinket, Equipment.EquippedFirstTrinket);
        AddEquippedItem(EquipmentSlotType.SecondTrinket, Equipment.EquippedSecondTrinket);

        dto.EquippedItems = equippedItems.ToArray();
    }

    private void UploadQuestSystem(ref UserPlayDataDTO dto)
    {
        dto.QuestSystemData = QuestSystem.CaptureDTO();
    }

    private void UploadCheckpointSystem(ref UserPlayDataDTO dto)
    {
        dto.LastCheckpointID = Checkpoint.LastCheckpointID;
    }

    private void UploadPortalSystem(ref UserPlayDataDTO dto)
    {
        dto.PortalSystemData = new PortalSystemDTO
        {
            Launched = Portal.Launched,
            TargetLevelID = Portal.TargetLevelID,
            Position = Portal.Position,
            DirectionRight = Portal.DirectionRight
        };
    }

    private void UploadProgressSystem(ref UserPlayDataDTO dto)
    {
        dto.ProgressData = new ProgressSystemDTO
        {
            LastLevelID = Progress.LastLevelID,
            LastPosition = Progress.LastPosition
        };
    }

    private void UploadOptionsSystem(ref UserPlayDataDTO dto)
    {
        dto.OptionsData = new OptionsSystemDTO
        {
            BGMVolume = Options.BGMVolume,
            SFXVolume = Options.SFXVolume
        };
    }

    private void UploadSkillSystem(ref UserPlayDataDTO dto)
    {
        dto.SkillSystemData = SkillSys.CaptureDTO();
    }

    public override void Clear()
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to clear user play data: {e.Message}");
        }

        Gold = 0;
        Inventory.Clear();
    }
}