// The Item struct only contains the dynamic item properties so that the static
// properties can be read from the scriptable object and don't have to be synced
// over the network.
//
// Items have to be structs in order to work with SyncLists.
//
// Use .Equals to compare two items. Comparing the name is NOT enough for cases
// where dynamic stats differ. E.g. two pets with different levels shouldn't be
// merged.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public struct Item
{
    // the name to reference the template
    public string name;

    // constructors
    public Item(ScriptableItem data)
    {
        name = data.name;
    }

    // database item property access
    public ScriptableItem data
    {
        get
        {
            // show a useful error message if the key can't be found
            // note: ScriptableItem.OnValidate 'is in resource folder' check
            //       causes Unity SendMessage warnings and false positives.
            //       this solution is a lot better.
            if (!ScriptableItem.dict.ContainsKey(name))
                throw new KeyNotFoundException("There is no ScriptableItem with name=" + name + ". Make sure that all ScriptableItems are in the Resources folder so they are loaded properly.");
            return ScriptableItem.dict[name];
        }
    }
    public int maxStack => data.maxStack;
    public long buyPrice => data.buyPrice;
    public long sellPrice => data.sellPrice;
    public bool sellable => data.sellable;
    public Sprite image => data.image;

    // tooltip
    public string ToolTip()
    {
        // we use a StringBuilder so that addons can modify tooltips later too
        // ('string' itself can't be passed as a mutable object)
        StringBuilder tip = new StringBuilder(data.ToolTip());
        return tip.ToString();
    }
}