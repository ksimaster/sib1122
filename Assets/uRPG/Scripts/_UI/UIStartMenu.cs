// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;

public class UIStartMenu : MonoBehaviour
{
    public GameStateManager manager;
    public GameObject panel;
    public Button newGameButton;
    public Button loadGameButton;
    public Button saveGameButton;
    public Button leaveGameButton;
    public Button quitButton;
    public KeyCode hotKey = KeyCode.Escape;

    void Update()
    {
        // trigger visibility with hotkey while ingame
        // (not if not in game yet, otherwise we can hide the main UI and see nothing)
        if (manager.state == GameState.World && Input.GetKeyDown(hotKey))
            panel.SetActive(!panel.activeSelf);

        // buttons
        newGameButton.interactable = manager.state == GameState.StartMenu;
        newGameButton.onClick.SetListener(() => {
            manager.StartNewGame();
            panel.SetActive(false);
        });

        loadGameButton.interactable = manager.state == GameState.StartMenu && SaveGame.singleton.Exists();
        loadGameButton.onClick.SetListener(() => {
            manager.JoinWorld();
            panel.SetActive(false);
        });

        saveGameButton.interactable = manager.state == GameState.World;
        saveGameButton.onClick.SetListener(() => {
            SaveGame.singleton.Save(Player.player.gameObject);
            panel.SetActive(false);
        });

        leaveGameButton.interactable = manager.state == GameState.World;
        leaveGameButton.onClick.SetListener(() => {
            manager.LeaveWorld();
        });

        quitButton.onClick.SetListener(() => { GameStateManager.Quit(); });
    }
}
