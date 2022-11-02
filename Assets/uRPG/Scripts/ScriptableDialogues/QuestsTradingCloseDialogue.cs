using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="uRPG Dialogue/Quests, Trading and Close Dialogue", order=999)]
public class QuestsTradingCloseDialogue : ScriptableDialogue
{
    [TextArea(1, 30)] public string welcomeText;
    public ScriptableQuest[] quests;
    public string tradeText = "Trade";
    public string closeText = "Close";

    public override string GetText(Player player) { return welcomeText; }

    public override List<DialogueChoice> GetChoices(Player player)
    {
        List<DialogueChoice> result = new List<DialogueChoice>();

        // quests
        foreach (ScriptableQuest quest in quests)
        {
            // can we accept this yet, or did we accept it already?
            // (don't show quests for level 50 if we are level 1, etc.)
            if (player.quests.CanAccept(quest) || player.quests.HasActive(quest.name))
            {
                result.Add(new DialogueChoice(
                    quest.name,
                    true,
                    (() => {
                        // construct a new QuestDialogue for this quest and show it
                        // (need to create runtime scriptable objects via
                        //  ScriptableObject.CreateInstance)
                        QuestDialogue dialogue = CreateInstance<QuestDialogue>();
                        dialogue.quest = quest;
                        UINpcDialogue.singleton.Show(dialogue, player);
                    })
                ));
            }
        }

        // trade
        result.Add(new DialogueChoice(
            tradeText,
            true,
            (() => {
                UIMainPanel.singleton.Show();
                UINpcDialogue.singleton.Hide();
            })
        ));

        // close
        result.Add(new DialogueChoice(closeText, true, UINpcDialogue.singleton.Hide));

        return result;
    }
}
