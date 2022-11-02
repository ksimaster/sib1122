// Intelligence Attribute that grants extra mana.
using System;
using UnityEngine;

public class Intelligence : PlayerAttribute, IManaBonus
{
    // 1 point means 1% of max bonus
    public float manaBonusPercentPerPoint = 0.01f;

    public int GetManaBonus(int baseMana)
    {
        return Convert.ToInt32(baseMana * (value * manaBonusPercentPerPoint));
    }
    public int GetManaRecoveryBonus() { return 0; }
}
