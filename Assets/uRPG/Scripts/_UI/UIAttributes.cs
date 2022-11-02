using UnityEngine;
using UnityEngine.UI;

public class UIAttributes : MonoBehaviour
{
    public UIAttributeSlot slotPrefab;
    public Transform content;
    public Text remainingPointsText;

    void Update()
    {
        Player player = Player.player;
        if (!player) return;

        PlayerAttribute[] attributes = player.GetComponents<PlayerAttribute>();

        // calculate remaining points (requires just one component)
        int remaining = attributes.Length > 0 ? attributes[0].PointsSpendable() : 0;

        // show remaining points
        remainingPointsText.text = remaining > 0 ? "(+" + remaining + ")" : "";

        // instantiate/destroy enough slots
        UIUtils.BalancePrefabs(slotPrefab.gameObject, attributes.Length, content);

        // refresh all items
        for (int i = 0; i < attributes.Length; ++i)
        {
            UIAttributeSlot slot = content.GetChild(i).GetComponent<UIAttributeSlot>();
            slot.nameText.text = attributes[i].GetType().ToString();
            slot.valueText.text = attributes[i].value.ToString();
            slot.button.interactable = player.health.current > 0 && remaining > 0;
            int icopy = i;
            slot.button.onClick.SetListener(() => {
                ++attributes[icopy].value;
            });
        }
    }
}
