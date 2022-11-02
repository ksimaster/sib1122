// for health, mana, etc.
using UnityEngine;
using UnityEngine.Events;

public abstract class Energy : MonoBehaviour
{
    // current value
    // set & get: keep between min and max
    int _current = 0;
    public int current
    {
        get { return Mathf.Min(_current, max); }
        set
        {
            bool emptyBefore = _current == 0;
            _current = Mathf.Clamp(value, 0, max);
            if (_current == 0 && !emptyBefore) onEmpty.Invoke();
        }
    }

    // maximum value (may depend on level, buffs, items, etc.)
    public abstract int max { get; }

    // recovery per tick (may depend on buffs, items etc.)
    // -> 'recoveryRate' sounds like 1/s, but 'PerTick' makes it 100% clear
    public abstract int recoveryPerTick { get; }

    // don't recover while dead. all energy scripts need to check Health.
    public Health health;

    // recovery can swap over, for example:
    //   once breath is empty it decreases health (RPGs usually have swimming)
    public Energy overflowInto;
    public Energy underflowInto;

    // spawn with full energy? important for monsters, etc.
    public bool spawnFull = true;

    [Header("Events")]
    public UnityEvent onEmpty;

    protected virtual void Awake()
    {
        // spawn with full health if needed
        // (before Start because some components might do logic based on health
        //  in start already, like FSM)
        if (spawnFull) current = max;
    }

    void Start()
    {
        // recovery every second
        InvokeRepeating(nameof(Recover), 1, 1);
    }

    // get percentage
    public float Percent() =>
        (current != 0 && max != 0) ? (float)current / (float)max : 0;

    // recover once a second
    // note: when stopping the server with the networkmanager gui, it will
    //       generate warnings that Recover was called on client because some
    //       entites will only be disabled but not destroyed. let's not worry
    //       about that for now.
    public void Recover()
    {
        if (enabled && health.current > 0)
        {
            // calculate over/underflowing value (might be <0 or >max)
            int next = current + recoveryPerTick;

            // assign current in range [0,max]
            current = next;

            // apply underflow (if any) by '-n'
            // (if next=-3 then underflow+=(-3))
            if (next < 0 && underflowInto != null)
                underflowInto.current += next;
            // apply overflow (if any)
            // (if next is bigger max then diff to max is 'next-max')
            else if (next > max && overflowInto)
                overflowInto.current += (next - max);
        }
    }
}
