// Strength Attribute that grants extra health.
using System;
using UnityEngine;

public class Strength : PlayerAttribute, IHealthBonus
{
    // 1 point means 1% of max bonus
    public float healthBonusPercentPerPoint = 0.01f;

    public int GetHealthBonus(int baseHealth)
    {
        return Convert.ToInt32(baseHealth * (value * healthBonusPercentPerPoint));
    }
    public int GetHealthRecoveryBonus() { return 0; }
}
