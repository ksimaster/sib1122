using UnityEngine;
using UnityEngine.UI;

public class UIExperienceBar : MonoBehaviour
{
    public GameObject panel;
    public Slider slider;
    public Text statusText;

    void Update()
    {
        Player player = Player.player;
        panel.SetActive(player != null); // hide while not in the game world
        if (!player) return;

        slider.value = player.experience.Percent();
        statusText.text = "Lv." + player.level.current + " (" + (player.experience.Percent() * 100).ToString("F2") + "%)";
    }
}
