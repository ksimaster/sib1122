using UnityEngine;

[RequireComponent(typeof(Collider))] // needed for interaction raycasts
public class GoldDrop : MonoBehaviour, Interactable
{
    // the amount of gold that this drop contains
    public long gold;
    public string currencyName = "Coins";

    // interactable ////////////////////////////////////////////////////////////
    public bool IsInteractable() { return true; }

    public string GetInteractionText()
    {
        return currencyName + " x " + gold;
    }

    public void OnInteract(Player player)
    {
        // try to add it to the inventory, destroy drop if it worked
        if (gold > 0)
        {
            player.inventory.gold += gold;

            // clear drop's item slot too so it can't be looted again
            // before truly destroyed
            gold = 0;
            Destroy(gameObject);
        }
    }
}
