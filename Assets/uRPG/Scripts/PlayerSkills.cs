// only players need skill experience, skill level upgrades, etc.
using UnityEngine;
using System.Linq;

public class PlayerSkills : Skills
{
    // components to be assigned in inspector
    public Level level;
    public PlayerLook look;

    public long skillExperience;

    public bool HasLearned(string skillName)
    {
        return skills.Any(skill => skill.name == skillName && skill.level > 0);
    }

    public bool HasLearnedWithLevel(string skillName, int skillLevel)
    {
        return skills.Any(skill => skill.name == skillName && skill.level >= skillLevel);
    }

    // helper function for command and UI
    // -> this is for learning and upgrading!
    public bool CanUpgrade(Skill skill)
    {
        return skill.level < skill.maxLevel &&
               level.current >= skill.upgradeRequiredLevel &&
               skillExperience >= skill.upgradeRequiredSkillExperience &&
               (skill.predecessor == null || (HasLearnedWithLevel(skill.predecessor.name, skill.predecessorLevel)));
    }

    // this is for learning and upgrading!
    public void Upgrade(int skillIndex)
    {
        // validate
        if (health.current > 0)
        {
            // can be upgraded?
            Skill skill = skills[skillIndex];
            if (CanUpgrade(skill))
            {
                // decrease skill experience
                skillExperience -= skill.upgradeRequiredSkillExperience;

                // upgrade
                ++skill.level;
                skills[skillIndex] = skill;
            }
        }
    }

    protected override Vector3 GetLookAt() { return look.lookPositionRaycasted; }

    public void OnKilledEnemy(Entity enemy)
    {
        // gain skill experience
        SkillExperienceReward reward = enemy.GetComponent<SkillExperienceReward>();
        if (reward != null)
            skillExperience += Experience.BalanceReward(reward.amount,  level.current, enemy.GetComponent<Level>().current);
    }
}
