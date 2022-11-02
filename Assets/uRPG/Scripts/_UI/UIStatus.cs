// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;

public class UIStatus : MonoBehaviour
{
    public Slider healthSlider;
    public Text healthStatus;
    public Slider manaSlider;
    public Text manaStatus;
    public Slider enduranceSlider;
    public Text enduranceStatus;

    public Text levelText;
    public Text damageText;
    public Text defenseText;

    void Update()
    {
        Player player = Player.player;
        if (!player) return;

        healthSlider.value = player.health.Percent();
        healthStatus.text = player.health.current + " / " + player.health.max;

        manaSlider.value = player.mana.Percent();
        manaStatus.text = player.mana.current + " / " + player.mana.max;

        enduranceSlider.value = player.endurance.Percent();
        enduranceStatus.text = player.endurance.current + " / " + player.endurance.max;

        levelText.text = player.level.current.ToString();
        damageText.text = player.combat.damage.ToString();
        defenseText.text = player.combat.defense.ToString();
    }
}
