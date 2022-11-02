// Buffs are like Skills, but for the Buffs list.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public partial struct Buff
{
    // the name to reference the template
    public string name;

    // dynamic stats (cooldowns etc.)
    public int level;
    public float buffTimeEnd;

    // constructors
    public Buff(BuffSkill data, int level)
    {
        name = data.name;
        this.level = level;
        buffTimeEnd = Time.time + data.buffTime.Get(level); // start buff immediately
    }

    // wrappers for easier access
    public BuffSkill data
    {
        get
        {
            // show a useful error message if the key can't be found
            // note: ScriptableSkill.OnValidate 'is in resource folder' check
            //       causes Unity SendMessage warnings and false positives.
            //       this solution is a lot better.
            if (!ScriptableSkill.dict.ContainsKey(name))
                throw new KeyNotFoundException("There is no ScriptableSkill with name=" + name + ". Make sure that all ScriptableSkills are in the Resources folder so they are loaded properly.");
            return (BuffSkill)ScriptableSkill.dict[name];
        }
    }
    public Sprite image { get { return data.image; } }
    public float buffTime { get { return data.buffTime.Get(level); } }
    public int bonusHealthMax { get { return data.bonusHealthMax.Get(level); } }
    public int bonusManaMax { get { return data.bonusManaMax.Get(level); } }
    public int bonusDamage { get { return data.bonusDamage.Get(level); } }
    public int bonusDefense { get { return data.bonusDefense.Get(level); } }
    public float bonusBlockChance { get { return data.bonusBlockChance.Get(level); } }
    public float bonusCriticalChance { get { return data.bonusCriticalChance.Get(level); } }
    public float bonusHealthPercentPerSecond { get { return data.bonusHealthPercentPerSecond.Get(level); } }
    public float bonusManaPercentPerSecond { get { return data.bonusManaPercentPerSecond.Get(level); } }
    public float bonusSpeed { get { return data.bonusSpeed.Get(level); } }
    public int maxLevel { get { return data.maxLevel; } }

    // tooltip - runtime part
    public string ToolTip()
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(data.ToolTip(level));
        return tip.ToString();
    }

    public float BuffTimeRemaining()
    {
        // how much time remaining until the buff ends? (using server time)
        return Time.time >= buffTimeEnd ? 0 : (buffTimeEnd - Time.time);
    }
}
