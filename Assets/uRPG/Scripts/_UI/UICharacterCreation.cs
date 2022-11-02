using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UICharacterCreation : MonoBehaviour
{
    public GameStateManager manager;
    public GameObject panel;
    public InputField nameInput;
    public Dropdown classDropdown;
    public Button createButton;
    public Button cancelButton;

    void Update()
    {
        // still in lobby?
        if (manager.state == GameState.CharacterCreation)
        {
            Show();

            // copy player classes to class selection
            classDropdown.options = manager.playerClasses.Select(
                p => new Dropdown.OptionData(p.name)
            ).ToList();

            // create
            createButton.interactable = manager.IsAllowedCharacterName(nameInput.text);
            createButton.onClick.SetListener(() => {
                manager.CreateCharacter(nameInput.text, manager.playerClasses[classDropdown.value]);
                Hide();
            });

            // cancel
            cancelButton.onClick.SetListener(() => {
                nameInput.text = "";
                Hide();
            });
        }
        else Hide();
    }

    public void Hide() { panel.SetActive(false); }
    public void Show() { panel.SetActive(true); }
}
