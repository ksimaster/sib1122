using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Inventory))]
public abstract class Equipment : ItemContainer, IHealthBonus, IManaBonus, ICombatBonus
{
    // used components. Assign in Inspector. Easier than GetComponent caching.
    public Health health;
    public Inventory inventory;

    // energy boni
    public int GetHealthBonus(int baseHealth)
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int bonus = 0;
        for (int i = 0; i < slots.Count; ++i)
        {
            ItemSlot slot = slots[i];
            if (slot.amount > 0)
                bonus += ((EquipmentItem)slot.item.data).healthBonus;
        }
        return bonus;
    }
    public int GetHealthRecoveryBonus()
    {
        return 0;
    }
    public int GetManaBonus(int baseMana)
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int bonus = 0;
        for (int i = 0; i < slots.Count; ++i)
        {
            ItemSlot slot = slots[i];
            if (slot.amount > 0)
                bonus += ((EquipmentItem)slot.item.data).manaBonus;
        }
        return bonus;
    }
    public int GetManaRecoveryBonus()
    {
        return 0;
    }

    // combat boni
    public int GetDamageBonus()
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int bonus = 0;
        for (int i = 0; i < slots.Count; ++i)
        {
            ItemSlot slot = slots[i];
            if (slot.amount > 0)
                bonus += ((EquipmentItem)slot.item.data).damageBonus;
        }
        return bonus;
    }
    public int GetDefenseBonus()
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int bonus = 0;
        for (int i = 0; i < slots.Count; ++i)
        {
            ItemSlot slot = slots[i];
            if (slot.amount > 0)
                bonus += ((EquipmentItem)slot.item.data).defenseBonus;
        }
        return bonus;
    }
}