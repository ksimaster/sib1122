using UnityEngine;

[CreateAssetMenu(menuName="uRPG Skill/Projectile", order=999)]
public class ProjectileSkill : DamageSkill
{
    [Header("Projectile")]
    public Projectile projectile; // Arrows, Bullets, Fireballs, ...

    public override void Apply(Entity caster, int skillLevel, Vector3 lookAt)
    {
        // spawn the skill effect. this can be used for anything ranging from
        // blood splatter to arrows to chain lightning.
        // -> we need to call an RPC anyway, it doesn't make much of a diff-
        //    erence if we use NetworkServer.Spawn for everything.
        // -> we try to spawn it at the weapon's projectile mount
        if (projectile != null)
        {
            Skills skills = caster.GetComponent<Skills>();

            GameObject go = Instantiate(projectile.gameObject, skills.effectMount.position, skills.effectMount.rotation);
            Projectile proj = go.GetComponent<Projectile>();
            proj.caster = caster.gameObject;
            proj.damage = damage.Get(skillLevel);
            proj.direction = lookAt - skills.effectMount.position;
        }
        else Debug.LogWarning(name + ": missing projectile");
    }
}
