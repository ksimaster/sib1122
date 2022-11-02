// Saves the skill info in a ScriptableObject that can be used ingame by
// referencing it from a MonoBehaviour. It only stores an skill's static data.
//
// We also add each one to a dictionary automatically, so that all of them can
// be found by name without having to put them all in a database. Note that we
// have to put them all into the Resources folder and use Resources.LoadAll to
// load them. This is important because some skills may not be referenced by any
// entity ingame (e.g. after a special event). But all skills should still be
// loadable from the database, even if they are not referenced by anyone
// anymore. So we have to use Resources.Load. (before we added them to the dict
// in OnEnable, but that's only called for those that are referenced in the
// game. All others will be ignored by Unity.)
//
// Entity animation controllers will need one bool parameter for each skill name
// and they can use the same animation for different skill templates by using
// multiple transitions. (this is way easier than keeping track of a skillindex)
//
// A Skill can be created by right clicking the Resources folder and selecting
// Create -> uRPG Skill. Existing skills can be found in the Resources folder
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract partial class ScriptableSkill : ScriptableObject
{
    [Header("Info")]
    [SerializeField, TextArea(1, 30)] protected string toolTip; // not public, use ToolTip()
    public Sprite image;
    public bool learnDefault; // normal attack etc.
    public bool showCastBar;

    [Header("Requirements")]
    public ScriptableSkill predecessor; // this skill has to be learned first
    public int predecessorLevel = 1; // level of predecessor skill that is required
    public bool requiresWeapon; // some might need empty-handed casting
    public LevelBasedInt requiredLevel; // required player level
    public LevelBasedLong requiredSkillExperience;

    [Header("Properties")]
    public int maxLevel = 1;
    public LevelBasedInt manaCosts;
    public LevelBasedFloat castTime;
    public LevelBasedFloat cooldown;
    public LevelBasedFloat castRange;

    [Header("Sound")]
    public AudioClip castSound;

    // casting /////////////////////////////////////////////////////////////////
    // overwrite for custom cast checks like distance, etc.
    public virtual bool CanCast(Entity caster, int skillLevel)
    {
        return caster.health.current > 0 &&
               caster.GetComponent<Mana>().current > manaCosts.Get(skillLevel);
    }

    // for effects
    public virtual void OnCastStarted(Entity caster)
    {
        AudioSource audioSource = caster.GetComponent<AudioSource>();
        if (audioSource != null && castSound != null)
            audioSource.PlayOneShot(castSound);
    }

    // for effects
    public virtual void OnCastFinished(Entity caster) {}

    // apply skill: deal damage, heal, launch projectiles, etc.
    // (this is called after casting has finished)
    public abstract void Apply(Entity caster, int skillLevel, Vector3 lookAt);

    // tooltip /////////////////////////////////////////////////////////////////
    // fill in all variables into the tooltip
    // this saves us lots of ugly string concatenation code.
    // (dynamic ones are filled in Skill.cs)
    // -> note: each tooltip can have any variables, or none if needed
    // -> example usage:
    /*
    <b>{NAME}</b>
    Description here...

    Damage: {DAMAGE}
    Cast Time: {CASTTIME}
    Cooldown: {COOLDOWN}
    Cast Range: {CASTRANGE}
    AoE Radius: {AOERADIUS}
    Mana Costs: {MANACOSTS}
    */
    public virtual string ToolTip(int level, bool showRequirements = false)
    {
        StringBuilder tip = new StringBuilder(toolTip);
        tip.Replace("{NAME}", name);
        tip.Replace("{LEVEL}", level.ToString());
        tip.Replace("{CASTTIME}", Utils.PrettySeconds(castTime.Get(level)));
        tip.Replace("{COOLDOWN}", Utils.PrettySeconds(cooldown.Get(level)));
        tip.Replace("{CASTRANGE}", castRange.Get(level).ToString());
        tip.Replace("{MANACOSTS}", manaCosts.Get(level).ToString());

        // only show requirements if necessary
        if (showRequirements)
        {
            tip.Append("\n<b><i>Required Level: " + requiredLevel.Get(1) + "</i></b>\n" +
                       "<b><i>Required Skill Exp.: " + requiredSkillExperience.Get(1) + "</i></b>\n");
            if (predecessor != null)
                tip.Append("<b><i>Required Skill: " + predecessor.name + " Lv. " + predecessorLevel + " </i></b>\n");
        }

        return tip.ToString();
    }

    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    // -> we save the hash so the dynamic item part doesn't have to contain and
    //    sync the whole name over the network
    static Dictionary<string, ScriptableSkill> cache;
    public static Dictionary<string, ScriptableSkill> dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableSkills in resources
                ScriptableSkill[] skills = Resources.LoadAll<ScriptableSkill>("");

                // check for duplicates, then add to cache
                List<string> duplicates = skills.ToList().FindDuplicates(skill => skill.name);
                if (duplicates.Count == 0)
                {
                    cache = skills.ToDictionary(skill => skill.name, skill => skill);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources folder contains multiple ScriptableSkills with the name " + duplicate + ". If you are using subfolders like 'Warrior/NormalAttack' and 'Archer/NormalAttack', then rename them to 'Warrior/(Warrior)NormalAttack' and 'Archer/(Archer)NormalAttack' instead.");
                }
            }
            return cache;
        }
    }
}
