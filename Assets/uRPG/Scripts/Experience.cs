using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(Level))]
public class Experience : MonoBehaviour
{
    // Used components. Assign in Inspector. Easier than GetComponent caching.
    public Level level;

    [Range(0, 1)] public float deathLossPercent = 0.05f;

    // current (int is not enough, we can have > 2 mil. easily)
    [SerializeField] long _current = 0;
    public long current
    {
        get { return _current; }
        set
        {
            if (value <= _current)
            {
                // decrease
                _current = Math.Max(value, 0);
            }
            else
            {
                // increase with level ups
                // set the new value (which might be more than expMax)
                _current = value;

                // now see if we leveled up (possibly more than once too)
                // (can't level up if already max level)
                while (_current >= max && level.current < level.max)
                {
                    // subtract current level's required exp, then level up
                    _current -= max;
                    ++level.current;

                    // call event
                    onLevelUp.Invoke();
                }

                // set to expMax if there is still too much exp remaining
                if (_current > max) _current = max;
            }
        }
    }
    [SerializeField] protected LevelBasedLong _max = new LevelBasedLong{baseValue=10, bonusPerLevel=10};
    public long max { get { return _max.Get(level.current); } }

    [Header("Events")]
    public UnityEvent onLevelUp;

    public float Percent()
    {
        return (current != 0 && max != 0) ? (float)current / (float)max : 0;
    }

    // players gain exp depending on their level. if a player has a lower level
    // than the monster, then he gains more exp (up to 100% more) and if he has
    // a higher level, then he gains less exp (up to 100% less)
    // -> test with monster level 20 and expreward of 100:
    //   BalanceReward( 1, 20, 100)); => 200
    //   BalanceReward( 9, 20, 100)); => 200
    //   BalanceReward(10, 20, 100)); => 200
    //   BalanceReward(11, 20, 100)); => 190
    //   BalanceReward(12, 20, 100)); => 180
    //   BalanceReward(13, 20, 100)); => 170
    //   BalanceReward(14, 20, 100)); => 160
    //   BalanceReward(15, 20, 100)); => 150
    //   BalanceReward(16, 20, 100)); => 140
    //   BalanceReward(17, 20, 100)); => 130
    //   BalanceReward(18, 20, 100)); => 120
    //   BalanceReward(19, 20, 100)); => 110
    //   BalanceReward(20, 20, 100)); => 100
    //   BalanceReward(21, 20, 100)); =>  90
    //   BalanceReward(22, 20, 100)); =>  80
    //   BalanceReward(23, 20, 100)); =>  70
    //   BalanceReward(24, 20, 100)); =>  60
    //   BalanceReward(25, 20, 100)); =>  50
    //   BalanceReward(26, 20, 100)); =>  40
    //   BalanceReward(27, 20, 100)); =>  30
    //   BalanceReward(28, 20, 100)); =>  20
    //   BalanceReward(29, 20, 100)); =>  10
    //   BalanceReward(30, 20, 100)); =>   0
    //   BalanceReward(31, 20, 100)); =>   0
    public static long BalanceReward(long reward, int attackerLevel, int victimLevel)
    {
        int levelDifference = Mathf.Clamp(victimLevel - attackerLevel, -10, 10);
        float multiplier = 1 + levelDifference * 0.1f;
        return Convert.ToInt64(reward * multiplier);
    }

    public void OnKilledEnemy(Entity enemy)
    {
        // gain experience
        ExperienceReward reward = enemy.GetComponent<ExperienceReward>();
        if (reward != null)
            current += BalanceReward(reward.amount, level.current, enemy.GetComponent<Level>().current);
    }

    public void OnDeath()
    {
        // lose experience
        long loss = Convert.ToInt64(max * deathLossPercent);
        current -= loss;
    }
}