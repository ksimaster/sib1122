using UnityEngine;
using UnityEngine.Events;

public class PlayerRespawning : MonoBehaviour
{
    // Used components. Assign in Inspector. Easier than GetComponent caching.
    public Health health;

    public float respawnTime = 10;
    [HideInInspector] public float respawnTimeEnd;

    [Header("Events")]
    public UnityEvent onRespawn;

    void Update()
    {
        if (health.current ==
            0 && Time.time >= respawnTimeEnd)
            onRespawn.Invoke();
    }

    public void OnDeath()
    {
        // set respawn end time
        respawnTimeEnd = Time.time + respawnTime;
    }

    public void OnRespawn()
    {
        print(name + " respawned");

        // go to start position without interpolation
        transform.position = GameStateManager.singleton.startPosition.position;

        // revive to closest spawn, with full energies, then go to idle
        foreach (Energy energy in GetComponents<Energy>())
            energy.current = energy.max;
    }
}
