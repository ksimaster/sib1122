using UnityEngine;

[CreateAssetMenu(menuName="uRPG Skill/Self Buff", order=999)]
public class SelfBuffSkill : BuffSkill
{
    // (has corrected target already)
    public override void Apply(Entity caster, int skillLevel, Vector3 lookAt)
    {
        // get components
        Health health = caster.health;
        Skills skills = caster.GetComponent<Skills>();

        // can't buff dead people
        if (health.current > 0)
        {
            // add buff or replace if already in there
            skills.AddOrRefreshBuff(new Buff(this, skillLevel));

            // show effect on self
            SpawnEffect(caster, caster);
        }
    }
}
