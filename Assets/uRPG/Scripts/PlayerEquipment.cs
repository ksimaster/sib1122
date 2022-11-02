using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct EquipmentInfo
{
    public string requiredCategory;
    public Transform location;
    public ScriptableItemAndAmount defaultItem;
}

[RequireComponent(typeof(Animator))]
public class PlayerEquipment : Equipment
{
    // Used components. Assign in Inspector. Easier than GetComponent caching.
    public Animator animator;
    public Player player;
    public PlayerMovement movement;
    public PlayerLook look;
    public AudioSource audioSource;

    public EquipmentInfo[] slotInfo =
    {
        new EquipmentInfo{requiredCategory="LeftHand", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Head", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Chest", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Ammo", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="RightHand", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Legs", location=null, defaultItem=new ScriptableItemAndAmount()},
        new EquipmentInfo{requiredCategory="Feet", location=null, defaultItem=new ScriptableItemAndAmount()}
    };

    // punching: reusing 'melee weapon' makes sense because it's the same code anyway
    public MeleeWeaponItem hands;

    // left & right hand locations for easier access
    public Transform leftHandLocation
    {
        get
        {
            foreach (EquipmentInfo slot in slotInfo)
                if (slot.requiredCategory == "LeftHand")
                    return slot.location;
            return null;
        }
    }

    public Transform rightHandLocation
    {
        get
        {
            foreach (EquipmentInfo slot in slotInfo)
                if (slot.requiredCategory == "RightHand")
                    return slot.location;
            return null;
        }
    }

    // cached SkinnedMeshRenderer bones without equipment, by name
    Dictionary<string, Transform> skinBones = new Dictionary<string, Transform>();

    // helpers /////////////////////////////////////////////////////////////////
    // returns current tool or hands
    public UsableItem GetUsableItemOrHands(int index)
    {
        ItemSlot slot = slots[index];
        return slot.amount > 0 ? (UsableItem)slot.item.data : hands;
    }

    // returns current tool or hands
    public UsableItem GetCurrentUsableItemOrHands()
    {
        // find right hand slot
        int index = GetEquipmentTypeIndex("RightHand");
        return index != -1 ? GetUsableItemOrHands(index) : null;
    }

    // TODO this is weird to pass slotindex too. needed because hands option though.
    void TryUseItem(UsableItem itemData, int slotIndex)
    {
        // note: no .amount > 0 check because it's either an item or hands

        // use current item or hands

        // repeated or one time use while holding mouse down?
        if (itemData.keepUsingWhileButtonDown || Input.GetMouseButtonDown(0))
        {
            // get the exact look position on whatever object we aim at
            Vector3 lookAt = look.lookPositionRaycasted;

            // use it
            Usability usability = itemData.CanUseEquipment(player, slotIndex, lookAt);
            if (usability == Usability.Usable)
            {
                // attack by using the weapon item
                //Debug.DrawLine(Camera.main.transform.position, lookAt, Color.gray, 1);
                UseItem(slotIndex, lookAt);
            }
            else if (usability == Usability.Empty)
            {
                // play empty sound locally (if any)
                // -> feels best to only play it when clicking the mouse button once, not while holding
                if (Input.GetMouseButtonDown(0))
                {
                    if (itemData.emptySound)
                        audioSource.PlayOneShot(itemData.emptySound);
                }
            }
            // do nothing if on cooldown (just wait) or if not usable at all
        }
    }

    void Awake()
    {
        // cache all default SkinnedMeshRenderer bones without equipment
        // (we might have multiple SkinnedMeshRenderers e.g. on feet, legs, etc.
        //  so we need GetComponentsInChildren)
        foreach (SkinnedMeshRenderer skin in GetComponentsInChildren<SkinnedMeshRenderer>())
            foreach (Transform bone in skin.bones)
                skinBones[bone.name] = bone;

        // make sure that weaponmounts are empty transform without children.
        // if someone drags in the right hand, then all the fingers would be
        // destroyed by RefreshLocation.
        // => only check in awake once, because at runtime it will have children
        //    if a weapon is equipped (hence we don't check in OnValidate)
        if (leftHandLocation != null && leftHandLocation.childCount > 0)
            Debug.LogWarning(name + " PlayerEquipment.leftHandLocation should have no children, otherwise they will be destroyed.");
        if (rightHandLocation != null && rightHandLocation.childCount > 0)
            Debug.LogWarning(name + " PlayerEquipment.rightHandLocation should have no children, otherwise they will be destroyed.");
    }

    // update
    void Update()
    {
        // refresh equipment models all the time
        for (int i = 0; i < slots.Count; ++i)
            RefreshLocation(i);

        // left click to use weapon(s)
        if (Input.GetMouseButton(0) &&
            Cursor.lockState == CursorLockMode.Locked &&
            health.current > 0 &&
            movement.state != MoveState.CLIMBING &&
            !look.IsFreeLooking() &&
            !Utils.IsCursorOverUserInterface() &&
            Input.touchCount <= 1)
        {
            // find right hand item
            int index = GetEquipmentTypeIndex("RightHand");
            if (index != -1)
            {
                // use current weapon or hands
                TryUseItem(GetCurrentUsableItemOrHands(), index);
            }
        }
    }

    bool CanReplaceAllBones(SkinnedMeshRenderer equipmentSkin)
    {
        // are all equipment SkinnedMeshRenderer bones in the player bones?
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        foreach (Transform bone in equipmentSkin.bones)
            if (!skinBones.ContainsKey(bone.name))
                return false;
        return true;
    }

    // replace all equipment SkinnedMeshRenderer bones with the original player
    // bones so that the equipment animation works with IK too
    // (make sure to check CanReplaceAllBones before)
    void ReplaceAllBones(SkinnedMeshRenderer equipmentSkin)
    {
        // get equipment bones
        Transform[] bones = equipmentSkin.bones;

        // replace each one
        for (int i = 0; i < bones.Length; ++i)
        {
            string boneName = bones[i].name;
            if (!skinBones.TryGetValue(boneName, out bones[i]))
                Debug.LogWarning(equipmentSkin.name + " bone " + boneName + " not found in original player bones. Make sure to check CanReplaceAllBones before.");
        }

        // reassign bones
        equipmentSkin.bones = bones;
    }

    void RebindAnimators()
    {
        foreach (Animator anim in GetComponentsInChildren<Animator>())
            anim.Rebind();
    }

    public void RefreshLocation(Transform location, ItemSlot slot)
    {
        // valid item, not cleared?
        if (slot.amount > 0)
        {
            EquipmentItem itemData = (EquipmentItem)slot.item.data;
            // new model? (don't do anything if it's the same model, which
            // happens after only Item.ammo changed, etc.)
            // note: we compare .name because the current object and prefab
            // will never be equal
            if (location.childCount == 0 || itemData.modelPrefab == null ||
                location.GetChild(0).name != itemData.modelPrefab.name)
            {
                // delete old model (if any)
                if (location.childCount > 0)
                    Destroy(location.GetChild(0).gameObject);

                // use new model (if any)
                if (itemData.modelPrefab != null)
                {
                    // instantiate and parent
                    GameObject go = Instantiate(itemData.modelPrefab, location, false);
                    go.name = itemData.modelPrefab.name; // avoid "(Clone)"

                    // skinned mesh and all bones can be be replaced?
                    // then replace all. this way the equipment can follow IK
                    // too (if any).
                    // => this is the RECOMMENDED method for animated equipment.
                    //    name all equipment bones the same as player bones and
                    //    everything will work perfectly
                    // => this is the ONLY way for equipment to follow IK, e.g.
                    //    in games where arms aim up/down.
                    SkinnedMeshRenderer equipmentSkin = go.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (equipmentSkin != null && CanReplaceAllBones(equipmentSkin))
                        ReplaceAllBones(equipmentSkin);

                    // animator? then replace controller to follow player's
                    // animations
                    // => this is the ALTERNATIVE method for animated equipment.
                    //    add the Animator and use the player's avatar. works
                    //    for animated pants, etc. but not for IK.
                    // => this is NECESSARY for 'external' equipment like wings,
                    //    staffs, etc. that should be animated but don't contain
                    //    the same bones as the player.
                    // is it a skinned mesh with an animator?
                    Animator anim = go.GetComponent<Animator>();
                    if (anim != null)
                    {
                        // assign main animation controller to it
                        anim.runtimeAnimatorController = animator.runtimeAnimatorController;

                        // restart all animators, so that skinned mesh equipment will be
                        // in sync with the main animation
                        RebindAnimators();
                    }
                }
            }
        }
        else
        {
            // empty now. delete old model (if any)
            if (location.childCount > 0)
                Destroy(location.GetChild(0).gameObject);
        }
    }
    // note: lookAt is available in PlayerLook, but we still pass the exact
    // uncompressed Vector3 here, because it needs to be PRECISE when shooting,
    // building structures, etc.
    public void UseItem(int index, Vector3 lookAt)
    {
        // validate
        if (0 <= index && index < slots.Count &&
            health.current > 0)
        {
            // use item at index, or hands
            // note: we don't decrease amount / destroy in all cases because
            // some items may swap to other slots in .Use()
            UsableItem itemData = GetUsableItemOrHands(index);
            if (itemData.CanUseEquipment(player, index, lookAt) == Usability.Usable)
            {
                // use it
                itemData.UseEquipment(player, index, lookAt);

                // call OnUsed
                OnUsedItem(itemData, lookAt);
            }
            else
            {
                // CanUse is checked locally before calling this Cmd, so if we
                // get here then either our prediction is off (in which case we
                // really should show a message for easier debugging), or someone
                // tried to cheat, or there's some networking issue, etc.
                Debug.LogWarning("UseItem rejected for: " + name + " item=" + itemData.name + "@" + Time.time);
            }
        }
    }

    // used by local simulation and Rpc, so we might as well put it in a function
    void OnUsedItem(UsableItem itemData, Vector3 lookAt)
    {
        // call OnUsed
        itemData.OnUsedEquipment(player, lookAt);

        // trigger upperbody usage animation
        // (trigger works best for usage, especially for repeated usage to)
        // (only for weapons, not for potions until we can hold potions in hand
        //  later on)
        if (itemData is WeaponItem)
            animator.SetTrigger("UPPERBODY_USED");
    }

    void RefreshLocation(int index)
    {
        ItemSlot slot = slots[index];
        EquipmentInfo info = slotInfo[index];

        // valid category and valid location? otherwise don't bother
        if (info.requiredCategory != "" && info.location != null)
            RefreshLocation(info.location, slot);
    }

    public void SwapInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (health.current > 0 &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            // item slot has to be empty (unequip) or equipable
            ItemSlot slot = inventory.slots[inventoryIndex];
            if (slot.amount == 0 ||
                slot.item.data is EquipmentItem &&
                ((EquipmentItem)slot.item.data).CanEquip(this, inventoryIndex, equipmentIndex))
            {
                // swap them
                ItemSlot temp = slots[equipmentIndex];
                slots[equipmentIndex] = slot;
                inventory.slots[inventoryIndex] = temp;
            }
        }
    }

    public void MergeInventoryEquip(int inventoryIndex, int equipmentIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (health.current > 0 &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            // both items have to be valid
            ItemSlot slotFrom = inventory.slots[inventoryIndex];
            ItemSlot slotTo = slots[equipmentIndex];
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
                    slots[equipmentIndex] = slotTo;
                }
            }
        }
    }

    public void MergeEquipInventory(int equipmentIndex, int inventoryIndex)
    {
        // validate: make sure that the slots actually exist in the inventory
        // and in the equipment
        if (health.current > 0 &&
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count &&
            0 <= equipmentIndex && equipmentIndex < slots.Count)
        {
            // both items have to be valid
            ItemSlot slotFrom = slots[equipmentIndex];
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
                    slots[equipmentIndex] = slotFrom;
                    inventory.slots[inventoryIndex] = slotTo;
                }
            }
        }
    }

    // helpers for easier slot access //////////////////////////////////////////
    // GetEquipmentTypeIndex("Chest") etc.
    public int GetEquipmentTypeIndex(string category)
    {
        // avoid Linq because it is HEAVY(!) on GC and performance
        for (int i = 0; i < slotInfo.Length; ++i)
            if (slotInfo[i].requiredCategory == category)
                return i;
        return -1;
    }

    // death & respawn /////////////////////////////////////////////////////////
    public void DropItemAndClearSlot(int index)
    {
        // drop and remove from inventory
        ItemSlot slot = slots[index];
        ((PlayerInventory)inventory).DropItem(slot.item, slot.amount);
        slot.amount = 0;
        slots[index] = slot;
    }

    public void DropItem(int index)
    {
        // validate
        if (health.current > 0 &&
            0 <= index && index < slots.Count && slots[index].amount > 0)
        {
            DropItemAndClearSlot(index);
        }
    }

    // drop all equipment on death, so others can loot us
    public void OnDeath()
    {
        for (int i = 0; i < slots.Count; ++i)
            if (slots[i].amount > 0)
                DropItemAndClearSlot(i);
    }

    // we don't clear items on death so that others can still loot us. we clear
    // them on respawn.
    public void OnRespawn()
    {
        // for each slot: make empty slot or default item if any
        for (int i = 0; i < slotInfo.Length; ++i)
            slots[i] = slotInfo[i].defaultItem.item != null ? new ItemSlot(new Item(slotInfo[i].defaultItem.item), slotInfo[i].defaultItem.amount) : new ItemSlot();
    }

    // drag & drop /////////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_EquipmentSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? (just check equality, rest is done server sided)
        if (inventory.slots[slotIndices[0]].amount > 0 && slots[slotIndices[1]].amount > 0 &&
            inventory.slots[slotIndices[0]].item.Equals(slots[slotIndices[1]].item))
        {
            MergeInventoryEquip(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            SwapInventoryEquip(slotIndices[0], slotIndices[1]);
        }
    }

    void OnDragAndDrop_EquipmentSlot_InventorySlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? (just check equality, rest is done server sided)
        if (slots[slotIndices[0]].amount > 0 && inventory.slots[slotIndices[1]].amount > 0 &&
            slots[slotIndices[0]].item.Equals(inventory.slots[slotIndices[1]].item))
        {
            MergeEquipInventory(slotIndices[0], slotIndices[1]);
        }
        // swap?
        else
        {
            SwapInventoryEquip(slotIndices[1], slotIndices[0]);
        }
    }

    void OnDragAndClear_EquipmentSlot(int slotIndex)
    {
        DropItem(slotIndex);
    }

    // validation //////////////////////////////////////////////////////////////
    void OnValidate()
    {
        // it's easy to set a default item and forget to set amount from 0 to 1
        // -> let's do this automatically.
        for (int i = 0; i < slotInfo.Length; ++i)
            if (slotInfo[i].defaultItem.item != null && slotInfo[i].defaultItem.amount == 0)
                slotInfo[i].defaultItem.amount = 1;
    }
}