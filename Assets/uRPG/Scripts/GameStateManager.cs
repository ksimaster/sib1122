// We use a custom NetworkManager that also takes care of login, character
// selection, character creation and more.
//
// We don't use the playerPrefab, instead all available player classes should be
// dragged into the spawnable objects property.
//
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Text.RegularExpressions;

// we need a clearly defined state to know if we are offline/in world/in lobby
// otherwise UICharacterSelection etc. never know 100% if they should be visible
// or not.
public enum GameState {StartMenu, CharacterCreation, World}

public class GameStateManager : MonoBehaviour
{
    // singleton for easier access from other scripts
    public static GameStateManager singleton;

    // current network manager state on client
    public GameState state = GameState.StartMenu;

    [Header("Spawn")]
    public Transform startPosition;

    [Header("Database")]
    public int characterNameMaxLength = 16;

    [Header("Player Classes")]
    public List<GameObject> playerClasses;

    // name checks /////////////////////////////////////////////////////////////
    public bool IsAllowedCharacterName(string characterName)
    {
        // not too long?
        // only contains letters, number and underscore and not empty (+)?
        // (important for database safety etc.)
        return characterName.Length <= characterNameMaxLength &&
               Regex.IsMatch(characterName, @"^[a-zA-Z0-9_]+$");
    }

    // events //////////////////////////////////////////////////////////////////
    void Awake()
    {
        singleton = this;
    }

    // start & stop ////////////////////////////////////////////////////////////
    public void StartNewGame()
    {
        // delete old database file (if any)
        SaveGame.singleton.Delete();

        // go to character creation
        state = GameState.CharacterCreation;
    }

    public void CreateCharacter(string characterName, GameObject prefab)
    {
        // create new character based on the prefab.
        // -> we also assign default items and equipment for new characters
        // (instantiate temporary player)
        print("creating character: " + characterName + " prefab=" + prefab);
        GameObject player = Instantiate(prefab, startPosition.position, Quaternion.identity);
        player.name = characterName;
        player.GetComponent<Player>().className = prefab.name;
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        for (int i = 0; i < inventory.size; ++i)
        {
            // add empty slot or default item if any
            inventory.slots.Add(i < inventory.defaultItems.Length ? new ItemSlot(new Item(inventory.defaultItems[i].item), inventory.defaultItems[i].amount) : new ItemSlot());
        }
        PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();
        for (int i = 0; i < equipment.slotInfo.Length; ++i)
        {
            // add empty slot or default item if any
            EquipmentInfo info = equipment.slotInfo[i];
            equipment.slots.Add(info.defaultItem.item != null ? new ItemSlot( new Item(info.defaultItem.item), info.defaultItem.amount) : new ItemSlot());
        }
        // fill all energies (after equipment in case of boni)
        foreach (Energy energy in player.GetComponents<Energy>())
            energy.current = energy.max;

        // save the player once
        SaveGame.singleton.Save(player);
        Destroy(player);

        // join world now
        JoinWorld();
    }

    // called from start menu
    public void JoinWorld()
    {
        // load everything
        SaveGame.singleton.Load(playerClasses);

        // set state
        state = GameState.World;
    }

    // called from start menu
    public void LeaveWorld()
    {
        // the best way to fully leave the world 100% and reset EVERYTHING is to
        // reload the scene
        // -> otherwise we might forget to reset monsters, etc.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // universal quit function for editor & build
    public static void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
