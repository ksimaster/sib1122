// common type for all kinds of weapons.
using System.Text;
using UnityEngine;

public abstract class WeaponItem : EquipmentItem
{
    [Header("Weapon")]
    public float attackRange = 20; // attack range
    public int damage = 10;
    public string upperBodyAnimationParameter;

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{ATTACKRANGE}", attackRange.ToString());
        tip.Replace("{DAMAGE}", damage.ToString());
        return tip.ToString();
    }
}
