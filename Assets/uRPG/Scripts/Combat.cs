using System;
using UnityEngine;
using UnityEngine.Events;

// inventory, attributes etc. can influence max health
public interface ICombatBonus
{
    int GetDamageBonus();
    int GetDefenseBonus();
}

[Serializable] public class UnityEventGameObjectInt : UnityEvent<GameObject, int> {}

[RequireComponent(typeof(Level))]
public class Combat : MonoBehaviour
{
    // components to be assigned in the inspector
    [Header("Components")]
    public Entity entity;
    public Level level;

    // invincibility is useful for GMs etc.
    [Header("Stats")]
    public bool invincible;
    public LevelBasedInt baseDamage = new LevelBasedInt{baseValue=1};
    public LevelBasedInt baseDefense = new LevelBasedInt{baseValue=1};
    public GameObject onDamageEffect;

    // events
    public UnityEventEntityInt onReceivedDamage;
    public UnityEventEntity onKilledEnemy;

    // cache components that give a bonus (attributes, inventory, etc.)
    ICombatBonus[] bonusComponents;
    void Awake()
    {
        bonusComponents = GetComponentsInChildren<ICombatBonus>();
    }

    // calculate damage
    public int damage
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            for (int i = 0; i < bonusComponents.Length; ++i)
                bonus += bonusComponents[i].GetDamageBonus();
            return baseDamage.Get(level.current) + bonus;
        }
    }

    // calculate defense
    public int defense
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            for (int i = 0; i < bonusComponents.Length; ++i)
                bonus += bonusComponents[i].GetDefenseBonus();
            return baseDefense.Get(level.current) + bonus;
        }
    }

    // deal damage while acknowledging the target's defense etc.
    public void DealDamageAt(Entity victim, int amount, Vector3 hitPoint, Vector3 hitNormal, Collider hitCollider)
    {
        // not dead yet? and not invincible?
        if (victim.health.current > 0 && !victim.combat.invincible)
        {
            // extra damage on that collider? (e.g. on head)
            DamageArea damageArea = hitCollider.GetComponent<DamageArea>();
            float multiplier = damageArea != null ? damageArea.multiplier : 1;
            int amountMultiplied = Mathf.RoundToInt(amount * multiplier);

            // subtract defense (but leave at least 1 damage, otherwise
            // it may be frustrating for weaker players)
            int damageDealt = Mathf.Max(amountMultiplied - victim.combat.defense, 1);

            // deal the damage
            victim.health.current -= damageDealt;

            // show effect on the other end
            victim.combat.ShowDamageEffect(damageDealt, hitPoint, hitNormal);

            // call OnReceivedDamage event on the target
            // -> can be used for monsters to pull aggro
            // -> can be used by equipment to decrease durability etc.
            victim.combat.onReceivedDamage.Invoke(entity, damageDealt);

            // killed it? then call OnKilledEnemy(other)
            if (victim.health.current == 0)
                onKilledEnemy.Invoke(victim);
        }
    }

    public void ShowDamageEffect(int amount, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (onDamageEffect)
            Instantiate(onDamageEffect, hitPoint, Quaternion.LookRotation(-hitNormal));
    }
}