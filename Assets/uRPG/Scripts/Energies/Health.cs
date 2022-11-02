using UnityEngine;

// inventory, attributes etc. can influence max health
public interface IHealthBonus
{
    int GetHealthBonus(int baseHealth);
    int GetHealthRecoveryBonus();
}

[RequireComponent(typeof(Level))]
[DisallowMultipleComponent]
public class Health : Energy
{
    public Level level;

    public int baseRecoveryPerTick = 0;
    public LevelBasedInt baseHealth = new LevelBasedInt{baseValue=100};

    // cache components that give a bonus (attributes, inventory, etc.)
    // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
    IHealthBonus[] _bonusComponents;
    IHealthBonus[] bonusComponents
    {
        get { return _bonusComponents ?? (_bonusComponents = GetComponents<IHealthBonus>()); }
    }

    // calculate max
    public override int max
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int baseThisLevel = baseHealth.Get(level.current);
            int bonus = 0;
            for (int i = 0; i < bonusComponents.Length; ++i)
                bonus += bonusComponents[i].GetHealthBonus(baseThisLevel);
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
                bonus += bonusComponents[i].GetHealthRecoveryBonus();
            return baseRecoveryPerTick + bonus;
        }
    }

    public void OnLevelUp()
    {
        // fill health on level up.
        // this is a great positive feedback for players/pets/etc.
        // otherwise low health indicators might start if we go from 100% to 90%
        current = max;
    }
}