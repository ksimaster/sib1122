using UnityEngine;

public class NpcDialogue : MonoBehaviour, Interactable
{
    public ScriptableDialogue dialogue;

    // interactable ////////////////////////////////////////////////////////////
    public bool IsInteractable() { return true; }

    public string GetInteractionText()
    {
        return "Talk to " + name;
    }

    public void OnInteract(Player player)
    {
        UINpcDialogue.singleton.Show(dialogue, player);
    }
}
