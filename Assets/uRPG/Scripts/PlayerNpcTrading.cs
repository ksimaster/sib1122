// for players to trade with npcs
// (not in NpcTrading script because we need to receive drag and drop)
using UnityEngine;

public class PlayerNpcTrading : MonoBehaviour
{
    public Health health;
    public PlayerInventory inventory;

    public void BuyItem(int index, int amount, NpcTrading npc)
    {
        if (health.current > 0 &&
            0 <= index && index < npc.saleItems.Length)
        {
            // valid amount?
            Item item = new Item(npc.saleItems[index]);
            if (1 <= amount && amount <= item.maxStack)
            {
                long price = item.buyPrice * amount;

                // player has enough gold and enough space in inventory?
                if (inventory.gold >= price && inventory.CanAdd(item, amount))
                {
                    // pay for it, add to inventory
                    inventory.gold -= price;
                    inventory.Add(item, amount);
                }
            }
        }
    }

    public void SellItem(int index, int amount, NpcTrading npc)
    {
        if (health.current > 0 &&
            0 <= index && index < inventory.slots.Count)
        {
            // sellable?
            ItemSlot slot = inventory.slots[index];
            if (slot.amount > 0 && slot.item.sellable)
            {
                // valid amount?
                if (1 <= amount && amount <= slot.amount)
                {
                    // sell the amount
                    long price = slot.item.sellPrice * amount;
                    inventory.gold += price;
                    slot.DecreaseAmount(amount);
                    inventory.slots[index] = slot;
                }
            }
        }
    }

    // drag & drop /////////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_NpcSellSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        ItemSlot slot = inventory.slots[slotIndices[0]];
        if (slot.item.sellable)
        {
            UINpcTrading.singleton.sellIndex = slotIndices[0];
            UINpcTrading.singleton.sellAmountInput.text = slot.amount.ToString();
        }
    }

    void OnDragAndClear_NpcSellSlot(int slotIndex)
    {
        UINpcTrading.singleton.sellIndex = -1;
    }
}
