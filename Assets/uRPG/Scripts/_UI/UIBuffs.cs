using UnityEngine;
using UnityEngine.UI;

public class UIBuffs : MonoBehaviour
{
    public UIBuffSlot slotPrefab;

    void Update()
    {
        Player player = Player.player;
        if (!player) return;

        // instantiate/destroy enough slots
        UIUtils.BalancePrefabs(slotPrefab.gameObject, player.skills.buffs.Count, transform);

        // refresh all
        for (int i = 0; i < player.skills.buffs.Count; ++i)
        {
            UIBuffSlot slot = transform.GetChild(i).GetComponent<UIBuffSlot>();

            // refresh
            slot.image.color = Color.white;
            slot.image.sprite = player.skills.buffs[i].image;
            // only build tooltip while it's actually shown. this
            // avoids MASSIVE amounts of StringBuilder allocations.
            if (slot.tooltip.IsVisible())
                slot.tooltip.text = player.skills.buffs[i].ToolTip();
            slot.slider.maxValue = player.skills.buffs[i].buffTime;
            slot.slider.value = player.skills.buffs[i].BuffTimeRemaining();
        }
    }
}