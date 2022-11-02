// used to store items
using System.Collections.Generic;
using UnityEngine;

public class Storage : Inventory, Interactable
{
    public int size = 22;

    // cache all storages on the server to save lots of computations
    // (otherwise we'd have to iterate NetworkServer.objects all the time)
    // -> differentiate them by name. make sure to not use the same name for
    //    two storages.
    //    (sceneId is not a good alternative because it changes when changing
    //     the hierarchy)
    public static Dictionary<string, Storage> storages = new Dictionary<string, Storage>();

    void Start()
    {
        // add to storages if none with the same name already exists
        if (!storages.ContainsKey(name))
        {
            storages[name] = this;
        }
        else Debug.LogWarning("A Storage with name " + name + " already exists. Use a different name for each Storage, otherwise it won't be saved to the Database.");
    }

    void OnDestroy()
    {
        storages.Remove(name);
    }

    // interactable ////////////////////////////////////////////////////////////
    public bool IsInteractable() { return true; }

    public string GetInteractionText()
    {
        return "Open";
    }

    public void OnInteract(Player player)
    {
        UIMainPanel.singleton.Show();
    }
}
