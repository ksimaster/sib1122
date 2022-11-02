using UnityEngine;
using UnityEngine.UI;

public class UIStorage : MonoBehaviour
{
    public GameObject panel;
    public UIStorageSlot slotPrefab;
    public Transform content;

    void Update()
    {
        Player player = Player.player;
        if (!player) return;

        PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
        if (interaction.current != null &&
            ((MonoBehaviour)interaction.current).GetComponent<Storage>() != null)
        {
            panel.SetActive(true);

            Storage storage = ((MonoBehaviour)interaction.current).GetComponent<Storage>();

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, storage.slots.Count, content);

            // refresh all items
            for (int i = 0; i < storage.slots.Count; ++i)
            {
                UIStorageSlot slot = content.GetChild(i).GetComponent<UIStorageSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                ItemSlot itemSlot = storage.slots[i];

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
                    slot.amountOverlay.SetActive(false);
                }
            }
        }
        else panel.SetActive(false);
    }
}
