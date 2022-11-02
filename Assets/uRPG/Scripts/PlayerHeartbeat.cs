using UnityEngine;

public class PlayerHeartbeat : MonoBehaviour
{
    public AudioSource audioSource;
    public Health health;

    void Update()
    {
        audioSource.volume = 1 - health.Percent();
    }
}
