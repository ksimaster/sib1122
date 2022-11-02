// For attributes, but we can't call it 'Attribute' because of C#'s Attribute
using UnityEngine;
using System.Linq;

public abstract class PlayerAttribute : MonoBehaviour
{
    // components to be assigned in inspector
    public Level level;

    public int value;

    public int PointsSpendable()
    {
        // calculate the amount of attribute points that can still be spent
        // -> one point per level
        // -> we don't need to store the points in an extra variable, we can
        //    simply decrease the attribute points spent from the level
        PlayerAttribute[] attributes = GetComponents<PlayerAttribute>();
        return level.current - attributes.Sum(attr => attr.value);
    }
}
