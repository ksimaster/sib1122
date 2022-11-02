// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINpcDialogue : MonoBehaviour
{
    public static UINpcDialogue singleton;

    public GameObject panel;
    public Text welcomeText;
    public Transform content;
    public UIDialogueChoice slotPrefab;

    void Awake() { singleton = this; }

    public void Show(ScriptableDialogue dialogue, Player player)
    {
        panel.SetActive(true);

        // show text
        welcomeText.text = dialogue.GetText(player);

        // show choices
        List<DialogueChoice> choices = dialogue.GetChoices(player);

        // instantiate/destroy enough slots
        UIUtils.BalancePrefabs(slotPrefab.gameObject, choices.Count, content);

        // refresh all choices
        for (int i = 0; i < choices.Count; ++i)
        {
            UIDialogueChoice slot = content.GetChild(i).GetComponent<UIDialogueChoice>();

            DialogueChoice choice = choices[i];
            slot.button.interactable = choice.interactable;
            slot.button.GetComponentInChildren<Text>().text = choice.text;
            slot.button.onClick.SetListener(choice.action);
        }
    }

    public void Hide() { panel.SetActive(false); }
}
