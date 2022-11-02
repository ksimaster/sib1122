// a simple script that hides the GameObject on death and shows it again later
using UnityEngine;
using UnityEngine.Events;

public class OnDeathRespawn : MonoBehaviour
{
    [Header("Death")]
    public float deathTime = 30; // enough for animation & looting

    [Header("Respawn")]
    public float respawnTime = 10;

    [Header("Events")]
    public UnityEvent onRespawn;
    public UnityEvent onDeathTimeElapsed;

    public void OnDeath()
    {
        // be dead for a while, then disappear
        Invoke(nameof(Disappear), deathTime);
    }

    void SetVisibility(bool vis)
    {
        // enable/disable renderers
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            rend.enabled = vis;

        // enable/disable colliders so they don't block itemdrops inside the
        // monster, etc.
        foreach (Collider co in GetComponentsInChildren<Collider>())
            co.enabled = vis;
    }

    void Disappear()
    {
        // hide
        SetVisibility(false);

        // reappear in a while
        Invoke(nameof(Reappear), respawnTime);
    }

    void Reappear()
    {
        // show again
        SetVisibility(true);

        // refill all energies
        foreach (Energy energy in GetComponents<Energy>())
            energy.current = energy.max;

        // call respawn event in case other components need to know about it
        onRespawn.Invoke();
    }
}
