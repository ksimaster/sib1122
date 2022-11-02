// interact with the storage
using UnityEngine;

public class PlayerStorageUsage : MonoBehaviour
{
    // components to be assigned in inspector
    public Health health;
    public PlayerLook look;
    public PlayerInteraction interaction;
    public PlayerInventory inventory;
    public PlayerEquipment equipment;
    public KeyCode[] splitKeys = { KeyCode.LeftShift, KeyCode.RightShift };

    // commands ////////////////////////////////////////////////////////////////
    public void SwapStorageStorage(GameObject storageGameObject, int fromIndex, int toIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and that they are not equal
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                // use head pos for range check because interactable raycast does too.
                // (transform.position isn't good when interacting above us!)
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= fromIndex && fromIndex < storage.slots.Count &&
                0 <= toIndex && toIndex < storage.slots.Count &&
                fromIndex != toIndex)
            {
                // swap them
                ItemSlot temp = storage.slots[fromIndex];
                storage.slots[fromIndex] = storage.slots[toIndex];
                storage.slots[toIndex] = temp;
            }
        }
    }

    public void StorageSplit(GameObject storageGameObject, int fromIndex, int toIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and that they are not equal
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                // use head pos for range check because interactable raycast does too.
                // (transform.position isn't good when interacting above us!)
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= fromIndex && fromIndex < storage.slots.Count &&
                0 <= toIndex && toIndex < storage.slots.Count &&
                fromIndex != toIndex)
            {
                // slotFrom needs at least two to split, slotTo has to be empty
                ItemSlot slotFrom = storage.slots[fromIndex];
                ItemSlot slotTo = storage.slots[toIndex];
                if (slotFrom.amount >= 2 && slotTo.amount == 0) {
                    // split them serversided (has to work for even and odd)
                    slotTo = slotFrom; // copy the value

                    slotTo.amount = slotFrom.amount / 2;
                    slotFrom.amount -= slotTo.amount; // works for odd too

                    // put back into the list
                    storage.slots[fromIndex] = slotFrom;
                    storage.slots[toIndex] = slotTo;
                }
            }
        }
    }

    public void StorageMerge(GameObject storageGameObject, int fromIndex, int toIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and that they are not equal
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                // use head pos for range check because interactable raycast does too.
                // (transform.position isn't good when interacting above us!)
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= fromIndex && fromIndex < storage.slots.Count &&
                0 <= toIndex && toIndex < storage.slots.Count &&
                fromIndex != toIndex)
            {
                // both items have to be valid
                ItemSlot slotFrom = storage.slots[fromIndex];
                ItemSlot slotTo = storage.slots[toIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    // make sure that items are the same type
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        // merge from -> to
                        // put as many as possible into 'To' slot
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);

                        // put back into the list
                        storage.slots[fromIndex] = slotFrom;
                        storage.slots[toIndex] = slotTo;
                    }
                }
            }
        }
    }

    public void SwapInventoryStorage(GameObject storageGameObject, int inventoryIndex, int storageIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the storage
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                // use head pos for range check because interactable raycast does too.
                // (transform.position isn't good when interacting above us!)
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= storageIndex && storageIndex < storage.slots.Count)
            {
                // swap them
                ItemSlot temp = storage.slots[storageIndex];
                storage.slots[storageIndex] = inventory.slots[inventoryIndex];
                inventory.slots[inventoryIndex] = temp;
            }
        }
    }

    public void MergeInventoryStorage(GameObject storageGameObject, int inventoryIndex, int storageIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the storage
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                // use head pos for range check because interactable raycast does too.
                // (transform.position isn't good when interacting above us!)
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= storageIndex && storageIndex < storage.slots.Count)
            {
                // both items have to be valid
                ItemSlot slotFrom = inventory.slots[inventoryIndex];
                ItemSlot slotTo = storage.slots[storageIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    // make sure that items are the same type
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        // merge from -> to
                        // put as many as possible into 'To' slot
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);

                        // put back into the lists
                        inventory.slots[inventoryIndex] = slotFrom;
                        storage.slots[storageIndex] = slotTo;
                    }
                }
            }
        }
    }

    public void MergeStorageInventory(GameObject storageGameObject, int storageIndex, int inventoryIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the storage
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                // use head pos for range check because interactable raycast does too.
                // (transform.position isn't good when interacting above us!)
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
                0 <= storageIndex && storageIndex < storage.slots.Count)
            {
                // both items have to be valid
                ItemSlot slotFrom = storage.slots[storageIndex];
                ItemSlot slotTo = inventory.slots[inventoryIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    // make sure that items are the same type
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        // merge from -> to
                        // put as many as possible into 'To' slot
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);

                        // put back into the lists
                        storage.slots[storageIndex] = slotFrom;
                        inventory.slots[inventoryIndex] = slotTo;
                    }
                }
            }
        }
    }

    public void MergeEquipStorage(GameObject storageGameObject, int equipmentIndex, int storageIndex)
    {
        // validate: make sure that the slots actually exist in the equipment
        // and in the storage
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                // use head pos for range check because interactable raycast does too.
                // (transform.position isn't good when interacting above us!)
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= equipmentIndex && equipmentIndex < equipment.slots.Count &&
                0 <= storageIndex && storageIndex < storage.slots.Count)
            {
                // both items have to be valid
                ItemSlot slotFrom = equipment.slots[equipmentIndex];
                ItemSlot slotTo = storage.slots[storageIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    // make sure that items are the same type
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        // merge from -> to
                        // put as many as possible into 'To' slot
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);

                        // put back into the lists
                        equipment.slots[equipmentIndex] = slotFrom;
                        storage.slots[storageIndex] = slotTo;
                    }
                }
            }
        }
    }

    public void MergeStorageEquip(GameObject storageGameObject, int storageIndex, int equipIndex)
    {
        // validate: make sure that the slots actually exist in the equipment
        // and in the storage
        if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                // use head pos for range check because interactable raycast does too.
                // (transform.position isn't good when interacting above us!)
                Vector3.Distance(look.headPosition, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= equipIndex && equipIndex < equipment.slots.Count &&
                0 <= storageIndex && storageIndex < storage.slots.Count)
            {
                // both items have to be valid
                ItemSlot slotFrom = storage.slots[storageIndex];
                ItemSlot slotTo = equipment.slots[equipIndex];
                if (slotFrom.amount > 0 && slotTo.amount > 0)
                {
                    // make sure that items are the same type
                    // note: .Equals because name AND dynamic variables matter (petLevel etc.)
                    if (slotFrom.item.Equals(slotTo.item))
                    {
                        // merge from -> to
                        // put as many as possible into 'To' slot
                        int put = slotTo.IncreaseAmount(slotFrom.amount);
                        slotFrom.DecreaseAmount(put);

                        // put back into the lists
                        storage.slots[storageIndex] = slotFrom;
                        equipment.slots[equipIndex] = slotTo;
                    }
                }
            }
        }
    }

    public void SwapStorageEquip(GameObject storageGameObject, int storageIndex, int equipmentIndex)
    {
        // TODO CanEquip needs a storage version

        // validate: make sure that the slots actually exist in the storage
        // and in the equipment
        /*if (storageGameObject != null)
        {
            Storage storage = storageGameObject.GetComponent<Storage>();
            if (storage != null &&
                Vector3.Distance(transform.position, storage.transform.position) <= interaction.range &&
                health.current > 0 &&
                0 <= storageIndex && storageIndex < storage.slots.Count &&
                0 <= equipmentIndex && equipmentIndex < equipment.slots.Count)
            {
                // item slot has to be empty (unequip) or equipable
                ItemSlot slot = storage.slots[storageIndex];
                if (slot.amount == 0 ||
                    slot.item.data is EquipmentItem &&
                    ((EquipmentItem)slot.item.data).CanEquip(this, storageIndex, equipmentIndex))
                {
                    // swap them
                    ItemSlot temp = equipment.slots[equipmentIndex];
                    equipment.slots[equipmentIndex] = slot;
                    storage.slots[storageIndex] = temp;
                }
            }
        }*/
    }

    // drag & drop /////////////////////////////////////////////////////////////
    void OnDragAndDrop_StorageSlot_StorageSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (interaction.current != null)
        {
            Storage storage = ((MonoBehaviour)interaction.current).GetComponent<Storage>();
            if (storage != null)
            {
                // merge? (just check equality, rest is done server sided)
                if (storage.slots[slotIndices[0]].amount > 0 && storage.slots[slotIndices[1]].amount > 0 &&
                    storage.slots[slotIndices[0]].item.Equals(storage.slots[slotIndices[1]].item))
                {
                    StorageMerge(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
                // split?
                else if (Utils.AnyKeyPressed(splitKeys))
                {
                    StorageSplit(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
                // swap?
                else
                {
                    SwapStorageStorage(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
            }
        }
    }

    void OnDragAndDrop_InventorySlot_StorageSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (interaction.current != null)
        {
            Storage storage = ((MonoBehaviour)interaction.current).GetComponent<Storage>();
            if (storage != null)
            {
                // merge? (just check equality, rest is done server sided)
                if (inventory.slots[slotIndices[0]].amount > 0 && storage.slots[slotIndices[1]].amount > 0 &&
                    inventory.slots[slotIndices[0]].item.Equals(storage.slots[slotIndices[1]].item))
                {
                    MergeInventoryStorage(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
                // swap?
                else
                {
                    SwapInventoryStorage(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
            }
        }
    }

    void OnDragAndDrop_StorageSlot_InventorySlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (interaction.current != null)
        {
            Storage storage = ((MonoBehaviour)interaction.current).GetComponent<Storage>();
            if (storage != null)
            {
                // merge? (just check equality, rest is done server sided)
                if (storage.slots[slotIndices[0]].amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
                    storage.slots[slotIndices[0]].item.Equals(inventory.slots[slotIndices[1]].item))
                {
                    MergeStorageInventory(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
                // swap?
                else
                {
                    SwapInventoryStorage(storage.gameObject, slotIndices[1], slotIndices[0]);
                }
            }
        }
    }

    void OnDragAndDrop_EquipmentSlot_StorageSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (interaction.current != null)
        {
            Storage storage = ((MonoBehaviour)interaction.current).GetComponent<Storage>();
            if (storage != null)
            {
                // merge? (just check equality, rest is done server sided)
                if (equipment.slots[slotIndices[0]].amount > 0 && storage.slots[slotIndices[1]].amount > 0 &&
                    equipment.slots[slotIndices[0]].item.Equals(storage.slots[slotIndices[1]].item))
                {
                    MergeEquipStorage(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
                // swap?
                else
                {
                    SwapStorageEquip(storage.gameObject, slotIndices[1], slotIndices[0]);
                }
            }
        }
    }

    void OnDragAndDrop_StorageSlot_EquipmentSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (interaction.current != null)
        {
            Storage storage = ((MonoBehaviour)interaction.current).GetComponent<Storage>();
            if (storage != null)
            {
                // merge? (just check equality, rest is done server sided)
                if (storage.slots[slotIndices[0]].amount > 0 && equipment.slots[slotIndices[1]].amount > 0 &&
                    storage.slots[slotIndices[0]].item.Equals(equipment.slots[slotIndices[1]].item))
                {
                    MergeStorageEquip(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
                // swap?
                else
                {
                    SwapStorageEquip(storage.gameObject, slotIndices[0], slotIndices[1]);
                }
            }
        }
    }
}
