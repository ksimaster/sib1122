using UnityEngine;

// inventory, attributes etc. can influence max
public interface IManaBonus
{
    int GetManaBonus(int baseMana);
    int GetManaRecoveryBonus();
}

[RequireComponent(typeof(Level))]
[DisallowMultipleComponent]
public class Mana : Energy
{
    public Level level;

    public int baseRecoveryPerTick = 1;
    public LevelBasedInt baseMana = new LevelBasedInt{baseValue=100};

    // cache components that give a bonus (attributes, inventory, etc.)
    // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
    IManaBonus[] _bonusComponents;
    IManaBonus[] bonusComponents
    {
        get { return _bonusComponents ?? (_bonusComponents = GetComponents<IManaBonus>()); }
    }

    // calculate max
    public override int max
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int baseThisLevel = baseMana.Get(level.current);
            int bonus = 0;
            for (int i = 0; i < bonusComponents.Length; ++i)
                bonus += bonusComponents[i].GetManaBonus(baseThisLevel);
            return baseThisLevel + bonus;
        }
    }

    public override int recoveryPerTick
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            for (int i = 0; i < bonusComponents.Length; ++i)
                bonus += bonusComponents[i].GetManaRecoveryBonus();
            return baseRecoveryPerTick + bonus;
        }
    }
}