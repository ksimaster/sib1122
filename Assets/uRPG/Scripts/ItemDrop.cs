using UnityEngine;

[RequireComponent(typeof(Collider))] // needed for looting raycasting
public class ItemDrop : MonoBehaviour, Interactable
{
    // default itemData, can be assigned in Inspector
#pragma warning disable CS0649 // Field is never assigned to
    [SerializeField] ScriptableItem itemData; // not public, so that people use .item & .amount
#pragma warning restore CS0649 // Field is never assigned to

    // drops need a real Item + amount so that we can set dynamic stats too
    // note: we don't use 'ItemSlot' so that 'amount' can be assigned in
    // Inspector for default spawns
    public int amount = 1; // sometimes set on server, needs to sync
    [HideInInspector] public Item item;

    void Start()
    {
        // create slot from template, unless we assigned it manually already
        // (e.g. if an item spawner assigns it after instantiating it)
        if (string.IsNullOrWhiteSpace(item.name) && itemData != null)
            item = new Item(itemData);
    }

    // interactable ////////////////////////////////////////////////////////////
    public bool IsInteractable() { return true; }

    public string GetInteractionText()
    {
        if (Player.player != null && itemData != null && amount > 0)
            return amount > 1 ? item.name + " x " + amount : item.name;
        return "";
    }

    public void OnInteract(Player player)
    {
        // try to add it to the inventory, destroy drop if it worked
        if (amount > 0 && player.inventory.Add(item, amount))
        {
            // clear drop's item slot too so it can't be looted again
            // before truly destroyed
            amount = 0;
            Destroy(gameObject);
        }
    }
}
