using UnityEngine;
using UnityEngine.UI;

public class UIHud : MonoBehaviour
{
    public GameObject panel;
    public Slider healthSlider;
    public Text healthStatus;
    public Slider manaSlider;
    public Text manaStatus;
    public Slider enduranceSlider;
    public Text enduranceStatus;

    void Update()
    {
        Player player = Player.player;
        panel.SetActive(player != null); // hide while not in the game world
        if (!player) return;

        // health
        healthSlider.value = player.health.Percent();
        healthStatus.text = player.health.current + " / " + player.health.max;

        // mana
        manaSlider.value = player.mana.Percent();
        manaStatus.text = player.mana.current + " / " + player.mana.max;

        // endurance
        enduranceSlider.value = player.endurance.Percent();
        enduranceStatus.text = player.endurance.current + " / " + player.endurance.max;
    }
}
