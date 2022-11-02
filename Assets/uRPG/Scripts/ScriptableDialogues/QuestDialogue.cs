// this should probably be created at runtime for a quest etc.
// (via ScriptableObject.CreateInstance<QuestDialogue>())
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName="uRPG Dialogue/Quest Dialogue", order=999)]
public class QuestDialogue : ScriptableDialogue
{
    public ScriptableQuest quest;
    public string acceptText = "Accept";
    public string completeText = "Complete";
    public string rejectText = "Close";

    public override string GetText(Player player)
    {
        // find quest index in player quest list
        int questIndex = player.quests.GetQuestIndexByName(quest.name);

        // running quest: shows description with current progress
        if (questIndex != -1)
        {
            Quest quest = player.quests.quests[questIndex];
            return quest.ToolTip(player);
        }
        // new quest
        else
        {
            return new Quest(quest).ToolTip(player);
        }
    }

    public override List<DialogueChoice> GetChoices(Player player)
    {
        List<DialogueChoice> result = new List<DialogueChoice>();

        // accept button if we can accept it
        if (player.quests.CanAccept(quest))
        {
            result.Add(new DialogueChoice(
                acceptText,
                true,
                (() => {
                    player.quests.Accept(quest);
                    UINpcDialogue.singleton.Hide();
                })));
        }

        // complete button if we have this quest
        if (player.quests.HasActive(quest.name))
        {
            result.Add(new DialogueChoice(
                completeText,
                player.quests.CanComplete(quest.name),
                (() => {
                    player.quests.Complete(quest);
                    UINpcDialogue.singleton.Hide();
                })));
        }

        // reject
        result.Add(new DialogueChoice(rejectText, true, UINpcDialogue.singleton.Hide));

        return result;
    }
}
