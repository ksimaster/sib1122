using UnityEngine;
using UnityEngine.UI;

public class UIRespawn : MonoBehaviour
{
    public GameObject panel;
    public Text timeText;

    void Update()
    {
        Player player = Player.player;
        if (!player) return;

        // player dead or alive?
        if (player.health.current == 0)
        {
            panel.SetActive(true);

            // calculate the respawn time remaining for the client
            double remaining = player.respawning.respawnTimeEnd - Time.time;
            timeText.text = remaining.ToString("F0");
        }
        else panel.SetActive(false);
    }
}
