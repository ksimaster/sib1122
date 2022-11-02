using UnityEngine;
using UnityEngine.UI;

public class UINpcTrading : MonoBehaviour
{
    public static UINpcTrading singleton;

    public GameObject panel;
    public UINpcTradingSlot slotPrefab;
    public Transform content;
    public UIDragAndDropable buySlot;
    public InputField buyAmountInput;
    public Text buyCostsText;
    public Button buyButton;
    public UIDragAndDropable sellSlot;
    public InputField sellAmountInput;
    public Text sellCostsText;
    public Button sellButton;
    [HideInInspector] public int buyIndex = -1;
    [HideInInspector] public int sellIndex = -1;

    void Awake() { singleton = this; }

    void Update()
    {
        Player player = Player.player;
        if (!player) return;

        if (player.interaction.current != null &&
            ((MonoBehaviour)player.interaction.current).GetComponent<NpcTrading>() != null)
        {
            panel.SetActive(true);
            NpcTrading npc = ((MonoBehaviour)player.interaction.current).GetComponent<NpcTrading>();

            // items for sale
            UIUtils.BalancePrefabs(slotPrefab.gameObject, npc.saleItems.Length, content);
            for (int i = 0; i < npc.saleItems.Length; ++i)
            {
                UINpcTradingSlot slot = content.GetChild(i).GetComponent<UINpcTradingSlot>();
                ScriptableItem itemData = npc.saleItems[i];

                // show item in UI
                int icopy = i;
                slot.button.onClick.SetListener(() => {
                    buyIndex = icopy;
                });
                slot.image.color = Color.white;
                slot.image.sprite = itemData.image;
                slot.tooltip.enabled = true;
                slot.tooltip.text = new ItemSlot(new Item(itemData)).ToolTip(); // with slot for {AMOUNT}
            }

            // buy
            if (buyIndex != -1 && buyIndex < npc.saleItems.Length)
            {
                ScriptableItem itemData = npc.saleItems[buyIndex];

                // make valid amount, calculate price
                int amount = buyAmountInput.text.ToInt();
                amount = Mathf.Clamp(amount, 1, itemData.maxStack);
                long price = amount * itemData.buyPrice;

                // show buy panel with item in UI
                buyAmountInput.text = amount.ToString();
                buySlot.GetComponent<Image>().color = Color.white;
                buySlot.GetComponent<Image>().sprite = itemData.image;
                buySlot.GetComponent<UIShowToolTip>().enabled = true;
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                if (buySlot.GetComponent<UIShowToolTip>().IsVisible())
                    buySlot.GetComponent<UIShowToolTip>().text = new Item(itemData).ToolTip();
                buyCostsText.text = price.ToString();
                buyButton.interactable = amount > 0 && price <= player.inventory.gold &&
                                         player.inventory.CanAdd(new Item(itemData), amount);
                buyButton.onClick.SetListener(() => {
                    player.npcTrading.BuyItem(buyIndex, amount, npc);
                    buyIndex = -1;
                    buyAmountInput.text = "1";
                });
            }
            else
            {
                // show default buy panel in UI
                buySlot.GetComponent<Image>().color = Color.clear;
                buySlot.GetComponent<Image>().sprite = null;
                buySlot.GetComponent<UIShowToolTip>().enabled = false;
                buyCostsText.text = "0";
                buyButton.interactable = false;
            }

            // sell
            if (sellIndex != -1 && sellIndex < player.inventory.slots.Count &&
                player.inventory.slots[sellIndex].amount > 0)
            {
                ItemSlot itemSlot = player.inventory.slots[sellIndex];

                // make valid amount, calculate price
                int amount = sellAmountInput.text.ToInt();
                amount = Mathf.Clamp(amount, 1, itemSlot.amount);
                long price = amount * itemSlot.item.sellPrice;

                // show sell panel with item in UI
                sellAmountInput.text = amount.ToString();
                sellSlot.GetComponent<Image>().color = Color.white;
                sellSlot.GetComponent<Image>().sprite = itemSlot.item.image;
                sellSlot.GetComponent<UIShowToolTip>().enabled = true;
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                if (sellSlot.GetComponent<UIShowToolTip>().IsVisible())
                    sellSlot.GetComponent<UIShowToolTip>().text = itemSlot.ToolTip();
                sellCostsText.text = price.ToString();
                sellButton.interactable = amount > 0;
                sellButton.onClick.SetListener(() => {
                    player.npcTrading.SellItem(sellIndex, amount, npc);
                    sellIndex = -1;
                    sellAmountInput.text = "1";
                });
            }
            else
            {
                // show default sell panel in UI
                sellSlot.GetComponent<Image>().color = Color.clear;
                sellSlot.GetComponent<Image>().sprite = null;
                sellSlot.GetComponent<UIShowToolTip>().enabled = false;
                sellCostsText.text = "0";
                sellButton.interactable = false;
            }
        }
        else panel.SetActive(false);
    }
}
