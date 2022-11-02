// note: 'Skills.skills' is a bit weird, but we will inherit from it anyway so
//       it's usually 'PlayerSkills.skills', which is fine.
//       (calling it 'SkillSystem' is just kinda weird, compared to the other
//        components)
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class UnityEventSkill : UnityEvent<Skill> {}

public abstract class Skills : MonoBehaviour, IHealthBonus, IManaBonus, ICombatBonus
{
    [Header("Components")]
    public Entity entity;
    public Health health;
    public Mana mana;

    public ScriptableSkill[] defaultSkills;

    [HideInInspector] // slots are created on start. don't modify manually.
    public List<Skill> skills = new List<Skill>();

    [HideInInspector] // slots are created on start. don't modify manually.
    public List<Buff> buffs = new List<Buff>();

    public Transform effectMount;

    [Header("Events")]
    public UnityEventSkill onSkillCastStarted;
    public UnityEventSkill onSkillCastFinished;

    [HideInInspector] public int current = -1;

    // skill boni //////////////////////////////////////////////////////////////
    public int GetHealthBonus(int baseHealth)
    {
        int passiveBonus = (from skill in skills
                            where skill.level > 0 && skill.data is PassiveSkill
                            select ((PassiveSkill)skill.data).bonusHealthMax.Get(skill.level)).Sum();
        int buffBonus = buffs.Sum(buff => buff.bonusHealthMax);
        return passiveBonus + buffBonus;
    }

    public int GetHealthRecoveryBonus()
    {
        float passivePercent = (from skill in skills
                                where skill.level > 0 && skill.data is PassiveSkill
                                select ((PassiveSkill)skill.data).bonusHealthPercentPerSecond.Get(skill.level)).Sum();
        float buffPercent = buffs.Sum(buff => buff.bonusHealthPercentPerSecond);
        return Convert.ToInt32(passivePercent * health.max) + Convert.ToInt32(buffPercent * health.max);
    }

    public int GetManaBonus(int baseMana)
    {
        int passiveBonus = (from skill in skills
                            where skill.level > 0 && skill.data is PassiveSkill
                            select ((PassiveSkill)skill.data).bonusManaMax.Get(skill.level)).Sum();
        int buffBonus = buffs.Sum(buff => buff.bonusManaMax);
        return passiveBonus + buffBonus;
    }

    public int GetManaRecoveryBonus()
    {
        float passivePercent = (from skill in skills
                                where skill.level > 0 && skill.data is PassiveSkill
                                select ((PassiveSkill)skill.data).bonusManaPercentPerSecond.Get(skill.level)).Sum();
        float buffPercent = buffs.Sum(buff => buff.bonusManaPercentPerSecond);
        return Convert.ToInt32(passivePercent * mana.max) + Convert.ToInt32(buffPercent * mana.max);
    }

    public int GetDamageBonus()
    {
        int passiveBonus = (from skill in skills
                            where skill.level > 0 && skill.data is PassiveSkill
                            select ((PassiveSkill)skill.data).bonusDamage.Get(skill.level)).Sum();
        int buffBonus = buffs.Sum(buff => buff.bonusDamage);
        return passiveBonus + buffBonus;
    }
    public int GetDefenseBonus()
    {
        int passiveBonus = (from skill in skills
                            where skill.level > 0 && skill.data is PassiveSkill
                            select ((PassiveSkill)skill.data).bonusDefense.Get(skill.level)).Sum();
        int buffBonus = buffs.Sum(buff => buff.bonusDefense);
        return passiveBonus + buffBonus;
    }

    // update //////////////////////////////////////////////////////////////////
    void Update()
    {
        // casting a skill?
        if (current != -1)
        {
            // cast time elapsed?
            if (skills[current].CastTimeRemaining() == 0)
            {
                FinishCast(current);
                current = -1;
            }
        }

        // remove old buffs (if any)
        CleanupBuffs();
    }

    // skill system ////////////////////////////////////////////////////////////
    // helper function to find a skill index
    public int GetSkillIndexByName(string skillName)
    {
        return skills.FindIndex(skill => skill.name == skillName);
    }

    // starts casting
    public void StartCast(int skillIndex)
    {
        Skill skill = skills[skillIndex];
        current = skillIndex;

        // start casting and set the casting end time
        skill.castTimeEnd = Time.time + skill.castTime;

        // save modifications
        skills[current] = skill;

        // call OnCastStarted for effects etc.
        skill.data.OnCastStarted(entity);

        // maybe some other component needs to know about it too
        onSkillCastStarted.Invoke(skill);
    }

    // this depends on player/monster/etc.
    // player uses PlayerLook script. Monster uses direction to target, etc.
    protected abstract Vector3 GetLookAt();

    // finishes casting. casting and waiting has to be done in the state machine
    void FinishCast(int skillIndex)
    {
        // still alive?
        if (health.current > 0)
        {
            Skill skill = skills[skillIndex];

            // get the exact look position on whatever object we aim at
            Vector3 lookAt = GetLookAt();

            // let the skill template handle the action
            skill.Apply(entity, lookAt);

            // call scriptableskill event
            skill.data.OnCastFinished(entity);

            // maybe some other component needs to know about it too
            onSkillCastFinished.Invoke(skill);

            // decrease mana in any case
            mana.current -= skill.manaCosts;

            // start the cooldown (and save it in the struct)
            skill.cooldownEnd = Time.time + skill.cooldown;

            // save any skill modifications in any case
            skills[current] = skill;
        }
    }

    // buffs ///////////////////////////////////////////////////////////////////
    // helper function to find a buff index
    public int GetBuffIndexByName(string buffName)
    {
        return buffs.FindIndex(buff => buff.name == buffName);
    }

    // helper function to add or refresh a buff
    public void AddOrRefreshBuff(Buff buff)
    {
        // reset if already in buffs list, otherwise add
        int index = buffs.FindIndex(b => b.name == buff.name);
        if (index != -1) buffs[index] = buff;
        else buffs.Add(buff);
    }

    // helper function to remove all buffs that ended
    void CleanupBuffs()
    {
        for (int i = 0; i < buffs.Count; ++i)
        {
            if (buffs[i].BuffTimeRemaining() == 0)
            {
                buffs.RemoveAt(i);
                --i;
            }
        }
    }
}