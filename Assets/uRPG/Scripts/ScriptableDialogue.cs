// Abstract dialogue. Can be used in all kinds of ways:
// * can inherit and generate choices on the fly
// * can inherit and have public array of choices, etc.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DialogueChoice
{
    public string text = "";
    public bool interactable = true;
    public UnityAction action;
    public DialogueChoice(string text, bool interactable, UnityAction action)
    {
        this.text = text;
        this.interactable = interactable;
        this.action = action;
    }
}

public abstract class ScriptableDialogue : ScriptableObject
{
    // this is what the npc says
    // (might depend on player level, or available quests, etc.)
    public abstract string GetText(Player player);

    // get choices for a player
    public abstract List<DialogueChoice> GetChoices(Player player);
}
