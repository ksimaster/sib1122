using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    public UIInventorySlot slotPrefab;
    public Transform content;
    public Text goldText;

    void Update()
    {
        Player player = Player.player;
        if (!player) return;

        // instantiate/destroy enough slots
        UIUtils.BalancePrefabs(slotPrefab.gameObject, player.inventory.slots.Count, content);

        // refresh all items
        for (int i = 0; i < player.inventory.slots.Count; ++i)
        {
            UIInventorySlot slot = content.GetChild(i).GetComponent<UIInventorySlot>();
            slot.dragAndDropable.name = i.ToString(); // drag and drop index
            ItemSlot itemSlot = player.inventory.slots[i];

            if (itemSlot.amount > 0)
            {
                // refresh valid item
                int icopy = i; // needed for lambdas, otherwise i is Count
                slot.button.onClick.SetListener(() => {
                    if (itemSlot.item.data is UsableItem usable &&
                        usable.CanUseInventory(player, icopy) == Usability.Usable)
                        player.inventory.UseItem(icopy);
                });
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
                slot.button.onClick.RemoveAllListeners();
                slot.tooltip.enabled = false;
                slot.dragAndDropable.dragable = false;
                slot.image.color = Color.clear;
                slot.image.sprite = null;
                slot.cooldownCircle.fillAmount = 0;
                slot.amountOverlay.SetActive(false);
            }
        }

        // gold
        goldText.text = player.inventory.gold.ToString();
    }
}
