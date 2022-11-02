using UnityEngine;
using UnityEngine.UI;

public class UIHotkeyInfo : MonoBehaviour
{
    public Text text;

    void Update()
    {
        // hide while not in the game world
        text.enabled = Player.player != null;
    }
}
