using UnityEngine;
using UnityEngine.UI;

public class UIMainPanel : MonoBehaviour
{
    // singleton to access it from player scripts without FindObjectOfType
    public static UIMainPanel singleton;

    public KeyCode hotKey = KeyCode.Tab;
    public GameObject panel;

    void Awake()
    {
        singleton = this;
    }

    void Update()
    {
        Player player = Player.player;
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);
    }

    public void Show()
    {
        panel.SetActive(true);
    }
}
