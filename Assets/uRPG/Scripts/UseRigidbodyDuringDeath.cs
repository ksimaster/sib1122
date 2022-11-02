// there is an old game development trick where a Rigidbody is only enabled as
// soon as the object died. this can be used for trees that aren't supposed to
// fall until they actually die.
using UnityEngine;

public class UseRigidbodyDuringDeath : MonoBehaviour
{
    public Rigidbody rigidBody;
    public float applyForce = 1000;

    // remember original position and rotation
    public bool resetPositionWhenRespawning = true;
    Vector3 startPosition;
    Quaternion startRotation;
    bool dirty;

    // needs to be known on client and server
    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    // on death events /////////////////////////////////////////////////////////
    public void OnDeath()
    {
        // fall on server and client (via rpc)
        StartFall();
    }

    public void OnDeathTimeElapsed()
    {
        StopFall();
    }

    public void OnRespawn()
    {
        // reset position after fall
        if (resetPositionWhenRespawning)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
        }
    }

    // falling /////////////////////////////////////////////////////////////////
    void StartFall()
    {
        rigidBody.isKinematic = false;

        // the tree won't fall if it stands perfectly straight, so let's add a
        // small force to make it fall
        rigidBody.AddForce(transform.forward * applyForce);
    }

    void StopFall()
    {
        rigidBody.isKinematic = true;
    }
}
