using UnityEngine;

public class Mount : Entity, Interactable
{
    [Header("Components")]
    public Level level;
    public Animator animator;

    // state
    public string state = "IDLE";

    [Header("Owner")]
    [HideInInspector] public Player owner;

    [Header("Death")]
    public float deathTime = 2; // enough for animation
    double deathTimeEnd; // double for long term precision

    [Header("Seat Position")]
    public Transform seat;

    void Start()
    {
        // change to dead if we spawned with 0 health
        if (health.current == 0) state = "DEAD";
    }

    // copy owner's position and rotation. no need for NetworkTransform.
    void CopyOwnerPositionAndRotation()
    {
        if (owner != null)
        {
            transform.position = owner.transform.position;
            transform.rotation = owner.transform.rotation;
        }
    }

    // finite state machine events /////////////////////////////////////////////
    bool EventOwnerDied()
    {
        return owner != null && owner.health.current == 0;
    }

    bool EventDied()
    {
        return health.current == 0;
    }

    bool EventDeathTimeElapsed()
    {
        return state == "DEAD" && Time.time >= deathTimeEnd;
    }

    // finite state machine ////////////////////////////////////////////////////
    string UpdateIDLE()
    {
        // copy owner's position and rotation. no need for NetworkTransform.
        CopyOwnerPositionAndRotation();

        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDied())
        {
            // die if owner died, so the mount doesn't stand around there forever
            health.current = 0;
        }
        if (EventDied())
        {
            // we died.
            OnDeath();
            return "DEAD";
        }
        if (EventDeathTimeElapsed()) {} // don't care

        return "IDLE"; // nothing interesting happened
    }

    string UpdateDEAD()
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDeathTimeElapsed())
        {
            // we were lying around dead for long enough now.
            // hide while respawning, or disappear forever
            Destroy(gameObject);
            return "DEAD";
        }
        if (EventOwnerDied()) {} // don't care
        if (EventDied()) {} // don't care, of course we are dead

        return "DEAD"; // nothing interesting happened
    }

    void Update()
    {
        if (state == "IDLE") state = UpdateIDLE();
        else if (state == "DEAD") state = UpdateDEAD();
        else Debug.LogError("invalid state:" + state);
    }

    void LateUpdate()
    {
        // pass parameters to animation state machine
        // => passing the states directly is the most reliable way to avoid all
        //    kinds of glitches like movement sliding, attack twitching, etc.
        // => make sure to import all looping animations like idle/run/attack
        //    with 'loop time' enabled, otherwise the client might only play it
        //    once
        // => only play moving animation while the agent is actually moving. the
        //    MOVING state might be delayed to due latency or we might be in
        //    MOVING while a path is still pending, etc.

        // use owner's moving state for maximum precision (not if dead)
        animator.SetFloat("DirZ", 0);
        if (owner != null)
        {
            Vector3 localVelocity = owner.transform.InverseTransformDirection(owner.movement.controller.velocity);
            animator.SetFloat("DirZ", localVelocity.z);
            animator.SetBool("SWIMMING", owner.movement.state == MoveState.MOUNTED_SWIMMING);
        }
        animator.SetBool("DEAD", state == "DEAD");
    }

    // death ///////////////////////////////////////////////////////////////////
    public void OnDeath()
    {
        // set death end time. we set it now to make sure that everything works
        // fine even if a pet isn't updated for a while. so as soon as it's
        // updated again, the death/respawn will happen immediately if current
        // time > end time.
        deathTimeEnd = Time.time + deathTime;
    }

    // interactable ////////////////////////////////////////////////////////////
    public bool IsInteractable()
    {
        // only interactable while no one is sitting on it
        return owner == null;
    }

    public string GetInteractionText()
    {
        return "Mount";
    }

    public void OnInteract(Player player)
    {
        if (!player.mountUsage.IsMounted())
        {
            // move player to mount position
            player.transform.position = transform.position;
            player.transform.rotation = transform.rotation;

            // mount it
            owner = player;
            player.mountUsage.mount = gameObject;
        }
    }
}

