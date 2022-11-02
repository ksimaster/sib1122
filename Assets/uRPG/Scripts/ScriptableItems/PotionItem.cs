using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName="uRPG Item/Potion", order=999)]
public class PotionItem : UsableItem
{
    [Header("Potion")]
    public int usageHealth;
    public int usageMana;

    // note: no need to overwrite CanUse functions. simply check cooldowns in base.

    void ApplyEffects(Player player)
    {
        player.health.current += usageHealth;
        player.mana.current += usageMana;
    }

    public override void UseInventory(Player player, int inventoryIndex)
    {
        // call base function to start cooldown
        base.UseInventory(player, inventoryIndex);

        ApplyEffects(player);

        // decrease amount
        ItemSlot slot = player.inventory.slots[inventoryIndex];
        slot.DecreaseAmount(1);
        player.inventory.slots[inventoryIndex] = slot;
    }
    public override void UseEquipment(Player player, int equipmentIndex, Vector3 lookAt)
    {
        // call base function to start cooldown
        base.UseEquipment(player, equipmentIndex, lookAt);

        ApplyEffects(player);

        // decrease amount
        ItemSlot slot = player.equipment.slots[equipmentIndex];
        slot.DecreaseAmount(1);
        player.equipment.slots[equipmentIndex] = slot;
    }

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{USAGEHEALTH}", usageHealth.ToString());
        tip.Replace("{USAGEMANA}", usageMana.ToString());
        return tip.ToString();
    }
}
