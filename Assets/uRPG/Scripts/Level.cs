using UnityEngine;

[DisallowMultipleComponent]
public class Level : MonoBehaviour
{
    public int current = 1;
    public int max = 1;

    void OnValidate()
    {
        current = Mathf.Clamp(current, 1, max);
    }
}