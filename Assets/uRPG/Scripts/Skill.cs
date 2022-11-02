// The Skill struct only contains the dynamic skill properties, so that the
// static properties can be read from the scriptable object. The benefits are
// low bandwidth and easy Player database saving (saves always refer to the
// scriptable skill, so we can change that any time).
//
// Skills have to be structs in order to work with SyncLists.
//
// We implemented the cooldowns in a non-traditional way. Instead of counting
// and increasing the elapsed time since the last cast, we simply set the
// 'end' Time variable to NetworkTime.time + cooldown after casting each time.
// This way we don't need an extra Update method that increases the elapsed time
// for each skill all the time.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public partial struct Skill
{
    // the name to reference the template
    public string name;

    // dynamic stats (cooldowns etc.)
    public int level; // 0 if not learned, >0 if learned
    public float castTimeEnd;
    public float cooldownEnd;

    // constructors
    public Skill(ScriptableSkill data)
    {
        name = data.name;

        // learned only if learned by default
        level = data.learnDefault ? 1 : 0;

        // ready immediately
        castTimeEnd = cooldownEnd = Time.time;
    }

    // wrappers for easier access
    public ScriptableSkill data
    {
        get
        {
            // show a useful error message if the key can't be found
            // note: ScriptableSkill.OnValidate 'is in resource folder' check
            //       causes Unity SendMessage warnings and false positives.
            //       this solution is a lot better.
            if (!ScriptableSkill.dict.ContainsKey(name))
                throw new KeyNotFoundException("There is no ScriptableSkill with name=" + name + ". Make sure that all ScriptableSkills are in the Resources folder so they are loaded properly.");
            return ScriptableSkill.dict[name];
        }
    }
    public float castTime => data.castTime.Get(level);
    public float cooldown => data.cooldown.Get(level);
    public float castRange => data.castRange.Get(level);
    public int manaCosts => data.manaCosts.Get(level);
    public Sprite image => data.image;
    public bool learnDefault => data.learnDefault;
    public bool showCastBar => data.showCastBar;
    public int maxLevel => data.maxLevel;
    public ScriptableSkill predecessor => data.predecessor;
    public int predecessorLevel => data.predecessorLevel;
    public bool requiresWeapon => data.requiresWeapon;
    public int upgradeRequiredLevel => data.requiredLevel.Get(level+1);
    public long upgradeRequiredSkillExperience => data.requiredSkillExperience.Get(level+1);

    // casting
    public bool CanCast(Entity caster)
    {
        return IsReady() && data.CanCast(caster, level);
    }
   public void Apply(Entity caster, Vector3 lookAt) { data.Apply(caster, level, lookAt); }

    // tooltip - dynamic part
    public string ToolTip(bool showRequirements = false)
    {
        // unlearned skills (level 0) should still show tooltip for level 1
        int showLevel = Mathf.Max(level, 1);

        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(data.ToolTip(showLevel, showRequirements));

        // only show upgrade if learned and not max level yet
        if (0 < level && level < maxLevel)
        {
            tip.Append("\n<i>Upgrade:</i>\n" +
                       "<i>  Required Level: " + upgradeRequiredLevel + "</i>\n" +
                       "<i>  Required Skill Exp.: " + upgradeRequiredSkillExperience + "</i>\n");
        }

        return tip.ToString();
    }

    public float CastTimeRemaining()
    {
        // how much time remaining until the casttime ends? (using server time)
        return Time.time >= castTimeEnd ? 0 : castTimeEnd - Time.time;
    }

    public bool IsCasting()
    {
        // we are casting a skill if the casttime remaining is > 0
        return CastTimeRemaining() > 0;
    }

    public float CooldownRemaining()
    {
        // how much time remaining until the cooldown ends? (using server time)
        return Time.time >= cooldownEnd ? 0 : cooldownEnd - Time.time;
    }

    public bool IsReady()
    {
        return CastTimeRemaining() == 0 && CooldownRemaining() == 0;
    }
}

