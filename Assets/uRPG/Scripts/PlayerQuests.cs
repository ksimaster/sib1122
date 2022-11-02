using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerQuests : MonoBehaviour
{
    // components to be assigned in inspector
    [Header("Components")]
    public Player player;
    public Health health;
    public Level level;
    public Experience experience;
    public PlayerInventory inventory;

    // contains active and completed quests (=all)
    [HideInInspector] // slots are created on start. don't modify manually.
    public List<Quest> quests = new List<Quest>();

    public int GetQuestIndexByName(string questName)
    {
        return quests.FindIndex(quest => quest.name == questName);
    }

    // helper function to check if the player has completed a quest before
    public bool HasCompleted(string questName)
    {
        return quests.Any(q => q.name == questName && q.completed);
    }

    // helper function to check if a player has an active (not completed) quest
    public bool HasActive(string questName)
    {
        return quests.Any(q => q.name == questName && !q.completed);
    }

    // helper function to check if the player can accept a new quest
    // note: no quest.completed check needed because we have a'not accepted yet'
    //       check
    public bool CanAccept(ScriptableQuest quest)
    {
        // has required level?
        // not accepted yet?
        // has finished predecessor quest (if any)?
        return level.current >= quest.requiredLevel &&  // has required level?
               GetQuestIndexByName(quest.name) == -1 && // not accepted yet?
               (quest.predecessor == null || HasCompleted(quest.predecessor.name));
    }

    public void Accept(ScriptableQuest quest)
    {
        // validate
        if (health.current > 0 && CanAccept(quest))
            quests.Add(new Quest(quest));
    }

    // helper function to check if the player can complete a quest
    public bool CanComplete(string questName)
    {
        // has the quest and not completed yet?
        int index = GetQuestIndexByName(questName);
        if (index != -1 && !quests[index].completed)
        {
            // fulfilled?
            Quest quest = quests[index];
            if(quest.IsFulfilled(player))
            {
                // enough space for reward item (if any)?
                return quest.rewardItem == null || inventory.CanAdd(new Item(quest.rewardItem), 1);
            }
        }
        return false;
    }

    public void Complete(ScriptableQuest questData)
    {
        // validate
        if (health.current > 0)
        {
            int index = GetQuestIndexByName(questData.name);
            if (index != -1)
            {
                // can complete it? (also checks inventory space for reward, if any)
                Quest quest = quests[index];
                if (CanComplete(quest.name))
                {
                    // call quest.OnCompleted to remove quest items from
                    // inventory, etc.
                    quest.OnCompleted(player);

                    // gain rewards
                    inventory.gold += quest.rewardGold;
                    experience.current += quest.rewardExperience;
                    if (quest.rewardItem != null)
                        inventory.Add(new Item(quest.rewardItem), 1);

                    // complete quest
                    quest.completed = true;
                    quests[index] = quest;
                }
            }
        }
    }

    // events //////////////////////////////////////////////////////////////////
    public void OnKilledEnemy(Entity enemy)
    {
        // call OnKilled in all active (not completed) quests
        for (int i = 0; i < quests.Count; ++i)
            if (!quests[i].completed)
                quests[i].OnKilled(player, i, enemy);
    }

    void OnTriggerEnter(Collider col)
    {
        // quest location?
        if (col.CompareTag("QuestLocation"))
        {
            // call OnLocation in all active (not completed) quests
            for (int i = 0; i < quests.Count; ++i)
                if (!quests[i].completed)
                    quests[i].OnLocation(player, i, col);
        }
    }
}
