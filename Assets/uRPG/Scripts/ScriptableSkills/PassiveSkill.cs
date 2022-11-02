using UnityEngine;

[CreateAssetMenu(menuName="uRPG Skill/Passive Skill", order=999)]
public class PassiveSkill : BonusSkill
{
    public override void Apply(Entity caster, int skillLevel, Vector3 lookAt) {}
}
