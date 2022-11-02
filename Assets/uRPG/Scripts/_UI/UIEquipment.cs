using UnityEngine;
using UnityEngine.UI;

public class UIEquipment : MonoBehaviour
{
    public UIEquipmentSlot slotPrefab;
    public Transform content;

    void Update()
    {
        Player player = Player.player;
        if (!player) return;

        // instantiate/destroy enough slots
        UIUtils.BalancePrefabs(slotPrefab.gameObject, player.equipment.slots.Count, content);

        // refresh all
        for (int i = 0; i < player.equipment.slots.Count; ++i)
        {
            UIEquipmentSlot slot = content.GetChild(i).GetComponent<UIEquipmentSlot>();
            slot.dragAndDropable.name = i.ToString(); // drag and drop slot
            ItemSlot itemSlot = player.equipment.slots[i];

            // set category overlay in any case. we use the last noun in the
            // category string, for example EquipmentWeaponBow => Bow
            // (disabled if no category, e.g. for archer shield slot)
            slot.categoryOverlay.SetActive(player.equipment.slotInfo[i].requiredCategory != "");
            string overlay = player.equipment.slotInfo[i].requiredCategory;
            slot.categoryText.text = overlay != "" ? overlay : "?";

            if (itemSlot.amount > 0)
            {
                // refresh valid item
                slot.tooltip.enabled = true;
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                if (slot.tooltip.IsVisible())
                    slot.tooltip.text = itemSlot.ToolTip();
                slot.dragAndDropable.dragable = true;
                slot.image.color = Color.white;
                slot.image.sprite = itemSlot.item.image;
                // cooldown if usable item
                if (itemSlot.item.data is UsableItem usable2)
                {
                    float cooldown = player.GetItemCooldown(usable2.cooldownCategory);
                    slot.cooldownCircle.fillAmount = usable2.cooldown > 0 ? cooldown / usable2.cooldown : 0;
                }
                else slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(itemSlot.amount > 1);
                slot.amountText.text = itemSlot.amount.ToString();
            }
            else
            {
                // refresh invalid item
                slot.tooltip.enabled = false;
                slot.dragAndDropable.dragable = false;
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(false);
            }
        }
    }
}
