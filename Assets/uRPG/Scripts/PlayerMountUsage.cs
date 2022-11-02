using UnityEngine;

public class PlayerMountUsage : MonoBehaviour
{
    public Transform meshToOffsetWhenMounted;
    public float seatOffsetY = -1;
    public KeyCode dismountKey = KeyCode.F;
    public Vector3 dismountOffset = Vector3.right;

    // the current mount (if any)
    [HideInInspector] public GameObject mount;

    // were we mounted last frame?
    bool wasMounted;

    public bool IsMounted()
    {
        return mount != null && mount.GetComponent<Health>().current > 0;
    }

    void Update()
    {
        // dismount if key pressed
        // BUT ignore if this is the first Update where we are mounted.
        // otherwise we would dismount immediately after mounting if the key is
        // the same as the interaction key.
        if (IsMounted() &&
            wasMounted &&
            Input.GetKeyDown(dismountKey))
        {
            // position the player next to it, not inside of it (looks better)
            transform.position = mount.transform.position + mount.transform.rotation * dismountOffset;

            // dismount
            mount.GetComponent<Mount>().owner = null;
            mount = null;
        }
        wasMounted = IsMounted();
    }

    void ApplyMountSeatOffset()
    {
        if (meshToOffsetWhenMounted != null)
        {
            // apply seat offset if on mount (not a dead one), reset otherwise
            if (mount != null && mount.GetComponent<Health>().current > 0)
                meshToOffsetWhenMounted.transform.position = mount.GetComponent<Mount>().seat.position + Vector3.up * seatOffsetY;
            else
                meshToOffsetWhenMounted.transform.localPosition = Vector3.zero;
        }
    }

    void LateUpdate()
    {
        // follow mount's seat position if mounted
        ApplyMountSeatOffset();
    }
}
