using System;
using System.Collections.Generic;

public struct EquipmentSystemInitData
{
    public EquipmentItem Weapon;
    public EquipmentItem Armor;
    public EquipmentItem FirstTrinket;
    public EquipmentItem SecondTrinket;
}

public class EquipmentSystem
{
    private Entity owner;

    private Dictionary<EquipmentSlotType, EquipmentItem> equippedItems = new()
    {
        { EquipmentSlotType.Weapon, null },
        { EquipmentSlotType.Armor, null },
        { EquipmentSlotType.FirstTrinket, null },
        { EquipmentSlotType.SecondTrinket, null } 
    };

    public EquipmentItem EquippedWeapon => equippedItems[EquipmentSlotType.Weapon];
    public EquipmentItem EquippedArmor => equippedItems[EquipmentSlotType.Armor];
    public EquipmentItem EquippedFirstTrinket => equippedItems[EquipmentSlotType.FirstTrinket];
    public EquipmentItem EquippedSecondTrinket => equippedItems[EquipmentSlotType.SecondTrinket];

    public event Action OnEquipmentChanged;

    public EquipmentSystem(EquipmentSystemInitData? initData = null)
    {
        if (initData.HasValue)
        {
            var data = initData.Value;
            equippedItems[EquipmentSlotType.Weapon] = data.Weapon;
            equippedItems[EquipmentSlotType.Armor] = data.Armor;
            equippedItems[EquipmentSlotType.FirstTrinket] = data.FirstTrinket;
            equippedItems[EquipmentSlotType.SecondTrinket] = data.SecondTrinket;
        }
    }

    public void Begin(Entity owner)
    {
        if (this.owner != null)
        {
            throw new Exception("EquipmentSystem is already owned by another entity.");
        }

        this.owner = owner;

        EquippedWeapon?.Equip(owner);
        EquippedArmor?.Equip(owner);
        EquippedFirstTrinket?.Equip(owner);
        EquippedSecondTrinket?.Equip(owner);

        OnEquipmentChanged?.Invoke();
    }

    public void End()
    {
        if (owner == null)
        {
            return;
        }

        EquippedWeapon?.Unequip();
        EquippedArmor?.Unequip();
        EquippedFirstTrinket?.Unequip();
        EquippedSecondTrinket?.Unequip();

        owner = null;

        OnEquipmentChanged?.Invoke();
    }

    public void EquipWeapon(EquipmentItem weapon)
    {
        if (EquippedWeapon == weapon)
        {
            return;
        }

        if (weapon.IsEquipped)
        {
            return;
        }

        EquipItem(EquippedWeapon, weapon);
        equippedItems[EquipmentSlotType.Weapon] = weapon;
        OnEquipmentChanged?.Invoke();
    }

    public void EquipArmor(EquipmentItem armor)
    {
        if (EquippedArmor == armor)
        {
            return;
        }

        if (armor.IsEquipped)
        {
            return;
        }

        EquipItem(EquippedArmor, armor);
        equippedItems[EquipmentSlotType.Armor] = armor;
        OnEquipmentChanged?.Invoke();
    }

    public void EquipFirstTrinket(EquipmentItem trinket)
    {
        if (EquippedFirstTrinket == trinket)
        {
            return;
        }

        if (trinket.IsEquipped)
        {
            return;
        }

        EquipItem(EquippedFirstTrinket, trinket);
        equippedItems[EquipmentSlotType.FirstTrinket] = trinket;
        OnEquipmentChanged?.Invoke();
    }

    public void EquipSecondTrinket(EquipmentItem trinket)
    {
        if (EquippedSecondTrinket == trinket)
        {
            return;
        }

        if (trinket.IsEquipped)
        {
            return;
        }

        EquipItem(EquippedSecondTrinket, trinket);
        equippedItems[EquipmentSlotType.SecondTrinket] = trinket;
        OnEquipmentChanged?.Invoke();
    }

    public void EquipItemByCategory(EquipmentItem item)
    {
        switch (item.ItemData.EquipmentCategory)
        {
            case EquipmentCategory.Weapon:
                EquipWeapon(item);
                break;
            case EquipmentCategory.Armor:
                EquipArmor(item);
                break;
            case EquipmentCategory.Trinket:
                if (EquippedFirstTrinket == null)
                {
                    EquipFirstTrinket(item);
                }
                else if (EquippedSecondTrinket == null)
                {
                    EquipSecondTrinket(item);
                }
                else
                {
                    EquipFirstTrinket(item);
                }
                break;
            default:
                throw new Exception("Unknown equipment category.");
        }
    }

    private void EquipItem(EquipmentItem prevItem, EquipmentItem targetItem)
    {
        if (prevItem != null)
        {
            prevItem.Unequip();
        }

        targetItem.Equip(owner);
    }

    public void UnequipWeapon()
    {
        if (EquippedWeapon == null)
        {
            return;
        }
        UnequipItem(EquippedWeapon);
        equippedItems[EquipmentSlotType.Weapon] = null;
        OnEquipmentChanged?.Invoke();
    }

    public void UnequipArmor()
    {
        if (EquippedArmor == null)
        {
            return;
        }
        UnequipItem(EquippedArmor);
        equippedItems[EquipmentSlotType.Armor] = null;
        OnEquipmentChanged?.Invoke();
    }

    public void UnequipFirstTrinket()
    {
        if (EquippedFirstTrinket == null)
        {
            return;
        }
        UnequipItem(EquippedFirstTrinket);
        equippedItems[EquipmentSlotType.FirstTrinket] = null;
        OnEquipmentChanged?.Invoke();
    }

    public void UnequipSecondTrinket()
    {
        if (EquippedSecondTrinket == null)
        {
            return;
        }
        UnequipItem(EquippedSecondTrinket);
        equippedItems[EquipmentSlotType.SecondTrinket] = null;
        OnEquipmentChanged?.Invoke();
    }

    private void UnequipItem(EquipmentItem item)
    {
        item.Unequip();
    }
}