// Inventory & Equip both use slots and some common functions. might as well
// abstract them to save code.
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemContainer : MonoBehaviour
{
    [HideInInspector] // slots are created on start. don't modify manually.
    public List<ItemSlot> slots = new List<ItemSlot>();

    // helper function to find an item in the slots
    public int GetItemIndexByName(string itemName)
    {
        // (avoid FindIndex to minimize allocations)
        for (int i = 0; i < slots.Count; ++i)
        {
            ItemSlot slot = slots[i];
            if (slot.amount > 0 && slot.item.name == itemName)
                return i;
        }
        return -1;
    }
}