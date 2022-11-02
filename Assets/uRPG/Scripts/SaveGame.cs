// Saves Character Data in a SQLite database.
//
// Tools to open sqlite database files:
//   Windows/OSX program: http://sqlitebrowser.org/
//   Firefox extension: https://addons.mozilla.org/de/firefox/addon/sqlite-manager/
//   Webhost: Adminer/PhpLiteAdmin
//
// Build notes:
// - requires Player settings to be set to '.NET' instead of '.NET Subset',
//   otherwise System.Data.dll causes ArgumentException.
// - requires sqlite3.dll x86 and x64 version for standalone (windows/mac/linux)
//   => found on sqlite.org website
// - requires libsqlite3.so x86 and armeabi-v7a for android
//   => compiled from sqlite.org amalgamation source with android ndk r9b linux
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite; // copied from Unity/Mono/lib/mono/2.0 to Plugins

public class SaveGame : MonoBehaviour
{
    // singleton for easier access
    public static SaveGame singleton;

    // file name
    public string saveFile = "SaveGame.sqlite";
    string path;

    // initialization///////////////////////////////////////////////////////////
    void Awake()
    {
        // initialize singleton
        if (singleton == null) singleton = this;

        // database path: Application.dataPath is always relative to the project,
        // but we don't want it inside the Assets folder in the Editor (git etc.),
        // instead we put it above that.
        // we also use Path.Combine for platform independent paths
        // and we need persistentDataPath on android
#if UNITY_EDITOR
        path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, saveFile);
#elif UNITY_ANDROID
        path = Path.Combine(Application.persistentDataPath, saveFile);
#elif UNITY_IOS
        path = Path.Combine(Application.persistentDataPath, saveFile);
#else
        path = Path.Combine(Application.dataPath, saveFile);
#endif
    }

    // simple public functions: exists/delete/load/save ////////////////////////
    public bool Exists()
    {
        return File.Exists(path);
    }

    public void Delete()
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    public void Load(List<GameObject> playerClasses)
    {
        // connect to database
        SqliteConnection connection = Connect();

        // load character
        LoadCharacter(connection, playerClasses);

        // load storages
        foreach (KeyValuePair<string, Storage> kvp in Storage.storages)
            LoadStorage(connection, kvp.Value);

        // close connection so we don't block the file forever
        connection.Close();
    }

    public void Save(GameObject player)
    {
        // connect to database
        SqliteConnection connection = Connect();

        // save character
        SaveCharacter(connection, player);

        // save storages
        SaveStorages(connection, Storage.storages.Values.ToList());

        // close connection so we don't block the file forever
        connection.Close();
    }

    ////////////////////////////////////////////////////////////////////////////
    // create the database, or connect if already exists
    SqliteConnection Connect()
    {
        // create database file if it doesn't exist yet
        if(!File.Exists(path))
            SqliteConnection.CreateFile(path);

        // open connection
        SqliteConnection connection = new SqliteConnection("URI=file:" + path);
        connection.Open();

        // create tables if they don't exist yet or were deleted
        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(connection,
                        @"CREATE TABLE IF NOT EXISTS character (
                            name TEXT NOT NULL PRIMARY KEY,
                            class TEXT NOT NULL,
                            x REAL NOT NULL,
                            y REAL NOT NULL,
                            z REAL NOT NULL,
                            yrotation REAL NOT NULL,
                            level INTEGER NOT NULL,
                            experience INTEGER NOT NULL,
                            skillExperience INTEGER NOT NULL,
                            health INTEGER NOT NULL,
                            mana INTEGER NOT NULL,
                            endurance INTEGER NOT NULL,
                            gold INTEGER NOT NULL)");

        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(connection,
                        @"CREATE TABLE IF NOT EXISTS character_attributes (
                            character TEXT NOT NULL,
                            name TEXT NOT NULL,
                            value INTEGER NOT NULL,
                            PRIMARY KEY(character, name))");

        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(connection,
                        @"CREATE TABLE IF NOT EXISTS character_inventory (
                            character TEXT NOT NULL,
                            slot INTEGER NOT NULL,
                            name TEXT NOT NULL,
                            amount INTEGER NOT NULL,
                            PRIMARY KEY(character, slot))");

        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(connection,
                        @"CREATE TABLE IF NOT EXISTS character_equipment (
                            character TEXT NOT NULL,
                            slot INTEGER NOT NULL,
                            name TEXT NOT NULL,
                            amount INTEGER NOT NULL,
                            PRIMARY KEY(character, slot))");

        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(connection,
                        @"CREATE TABLE IF NOT EXISTS character_skills (
                            character TEXT NOT NULL,
                            name TEXT NOT NULL,
                            level INTEGER NOT NULL,
                            castTimeEnd REAL NOT NULL,
                            cooldownEnd REAL NOT NULL,
                            PRIMARY KEY(character, name))");

        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(connection,
                        @"CREATE TABLE IF NOT EXISTS character_buffs (
                            character TEXT NOT NULL,
                            name TEXT NOT NULL,
                            level INTEGER NOT NULL,
                            buffTimeEnd REAL NOT NULL,
                            PRIMARY KEY(character, name))");

        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(connection,
                        @"CREATE TABLE IF NOT EXISTS character_quests (
                            character TEXT NOT NULL,
                            name TEXT NOT NULL,
                            field0 INTEGER NOT NULL,
                            completed INTEGER NOT NULL,
                            PRIMARY KEY(character, name))");

        // [PRIMARY KEY is important for performance: O(log n) instead of O(n)]
        ExecuteNonQuery(connection,
                        @"CREATE TABLE IF NOT EXISTS storages (
                            storage TEXT NOT NULL,
                            slot INTEGER NOT NULL,
                            name TEXT NOT NULL,
                            amount INTEGER NOT NULL,
                            PRIMARY KEY(storage, slot))");

        Debug.Log("connected to database");

        return connection;
    }

    // helper functions ////////////////////////////////////////////////////////
    // run a query that doesn't return anything
    public void ExecuteNonQuery(SqliteConnection connection, string sql, params SqliteParameter[] args)
    {
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            foreach (SqliteParameter param in args)
                command.Parameters.Add(param);
            command.ExecuteNonQuery();
        }
    }

    // run a query that returns a single value
    public object ExecuteScalar(SqliteConnection connection, string sql, params SqliteParameter[] args)
    {
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            foreach (SqliteParameter param in args)
                command.Parameters.Add(param);
            return command.ExecuteScalar();
        }
    }

    // run a query that returns several values
    // note: sqlite has long instead of int, so use Convert.ToInt32 etc.
    public List< List<object> > ExecuteReader(SqliteConnection connection, string sql, params SqliteParameter[] args)
    {
        List< List<object> > result = new List< List<object> >();

        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            foreach (SqliteParameter param in args)
                command.Parameters.Add(param);

            using (SqliteDataReader reader = command.ExecuteReader())
            {
                // the following code causes a SQL EntryPointNotFoundException
                // because sqlite3_column_origin_name isn't found on OSX and
                // some other platforms. newer mono versions have a workaround,
                // but as long as Unity doesn't update, we will have to work
                // around it manually. see also GetSchemaTable function:
                // https://github.com/mono/mono/blob/master/mcs/class/Mono.Data.Sqlite/Mono.Data.Sqlite_2.0/SQLiteDataReader.cs
                //
                //result.Load(reader); (DataTable)
                //
                // UPDATE: DataTable.Load(reader) works in Net 4.X now, but it's
                //         20x slower than the current approach.
                //         select * from character_inventory x 1000:
                //           425ms before
                //          7303ms with DataRow
                while (reader.Read())
                {
                    object[] buffer = new object[reader.FieldCount];
                    reader.GetValues(buffer);
                    result.Add(buffer.ToList());
                }
            }
        }

        return result;
    }

    // character data //////////////////////////////////////////////////////////
    void LoadAttributes(SqliteConnection connection, Player player)
    {
        PlayerAttribute[] attributes = player.GetComponents<PlayerAttribute>();
        foreach (PlayerAttribute attribute in attributes)
        {
            List< List<object> > table = ExecuteReader(connection, "SELECT value FROM character_attributes WHERE character=@character AND name=@name", new SqliteParameter("@character", player.name), new SqliteParameter("@name", attribute.GetType().ToString()));
            if (table.Count == 1)
            {
                List<object> mainrow = table[0];
                attribute.value = Convert.ToInt32((long)mainrow[0]);
            }
        }
    }

    void LoadInventory(SqliteConnection connection, PlayerInventory inventory)
    {
        // fill all slots first
        for (int i = 0; i < inventory.size; ++i)
            inventory.slots.Add(new ItemSlot());

        // then load valid items and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        List< List<object> > table = ExecuteReader(connection, "SELECT name, slot, amount FROM character_inventory WHERE character=@character", new SqliteParameter("@character", inventory.name));
        foreach (List<object> row in table)
        {
            string itemName = (string)row[0];
            int slot = Convert.ToInt32((long)row[1]);
            if (slot < inventory.size)
            {
                if (ScriptableItem.dict.TryGetValue(itemName, out ScriptableItem itemData))
                {
                    Item item = new Item(itemData);
                    int amount = Convert.ToInt32((long)row[2]);
                    inventory.slots[slot] = new ItemSlot(item, amount);
                }
                else Debug.LogWarning("LoadInventory: skipped item " + itemName + " for " + inventory.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
            }
            else Debug.LogWarning("LoadInventory: skipped slot " + slot + " for " + inventory.name + " because it's bigger than size " + inventory.size);
        }
    }

    void LoadEquipment(SqliteConnection connection, PlayerEquipment equipment)
    {
        // fill all slots first
        for (int i = 0; i < equipment.slotInfo.Length; ++i)
            equipment.slots.Add(new ItemSlot());

        // then load valid equipment and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        List< List<object> > table = ExecuteReader(connection, "SELECT name, slot, amount FROM character_equipment WHERE character=@character", new SqliteParameter("@character", equipment.name));
        foreach (List<object> row in table)
        {
            string itemName = (string)row[0];
            int slot = Convert.ToInt32((long)row[1]);
            if (slot < equipment.slotInfo.Length)
            {
                if (ScriptableItem.dict.TryGetValue(itemName, out ScriptableItem itemData))
                {
                    Item item = new Item(itemData);
                    int amount = Convert.ToInt32((long)row[2]);
                    equipment.slots[slot] = new ItemSlot(item, amount);
                }
                else Debug.LogWarning("LoadEquipment: skipped item " + itemName + " for " + equipment.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
            }
            else Debug.LogWarning("LoadEquipment: skipped slot " + slot + " for " + equipment.name + " because it's bigger than size " + equipment.slotInfo.Length);
        }
    }

    void LoadSkills(SqliteConnection connection, PlayerSkills skills)
    {
        // load skills based on skill templates (the others don't matter)
        // -> this way any skill changes in a prefab will be applied
        //    to all existing players every time (unlike item templates
        //    which are only for newly created characters)

        // fill all slots first
        foreach (ScriptableSkill skillData in skills.defaultSkills)
            skills.skills.Add(new Skill(skillData));

        // then load learned skills and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        List< List<object> > table = ExecuteReader(connection, "SELECT name, level, castTimeEnd, cooldownEnd FROM character_skills WHERE character=@character", new SqliteParameter("@character", skills.name));
        foreach (List<object> row in table)
        {
            string skillName = (string)row[0];
            int index = skills.skills.FindIndex(skill => skill.name == skillName);
            if (index != -1)
            {
                Skill skill = skills.skills[index];
                // make sure that 1 <= level <= maxlevel (in case we removed a skill
                // level etc)
                skill.level = Mathf.Clamp(Convert.ToInt32((long)row[1]), 1, skill.maxLevel);
                // make sure that 1 <= level <= maxlevel (in case we removed a skill
                // level etc)
                // castTimeEnd and cooldownEnd are based on Time.time
                // which will be different when restarting a server, hence why
                // we saved them as just the remaining times. so let's convert
                // them back again.
                skill.castTimeEnd = (float)row[2] + Time.time;
                skill.cooldownEnd = (float)row[3] + Time.time;

                skills.skills[index] = skill;
            }
        }
    }

    void LoadBuffs(SqliteConnection connection, PlayerSkills skills)
    {
        // load buffs
        // note: no check if we have learned the skill for that buff
        //       since buffs may come from other people too
        List< List<object> > table = ExecuteReader(connection, "SELECT name, level, buffTimeEnd FROM character_buffs WHERE character=@character", new SqliteParameter("@character", skills.name));
        foreach (List<object> row in table)
        {
            string buffName = (string)row[0];
            if (ScriptableSkill.dict.TryGetValue(buffName, out ScriptableSkill skillData))
            {
                // make sure that 1 <= level <= maxlevel (in case we removed a skill
                // level etc)
                int level = Mathf.Clamp(Convert.ToInt32((long)row[1]), 1, skillData.maxLevel);
                Buff buff = new Buff((BuffSkill)skillData, level);
                // buffTimeEnd is based on Time.time, which will be
                // different when restarting a server, hence why we saved
                // them as just the remaining times. so let's convert them
                // back again.
                buff.buffTimeEnd = (float)row[2] + Time.time;
                skills.buffs.Add(buff);
            }
            else Debug.LogWarning("LoadBuffs: skipped buff " + skillData.name + " for " + skills.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
        }
    }

    void LoadQuests(SqliteConnection connection, PlayerQuests quests)
    {
        // load quests
        List< List<object> > table = ExecuteReader(connection, "SELECT name, field0, completed FROM character_quests WHERE character=@character", new SqliteParameter("@character", quests.name));
        foreach (List<object> row in table)
        {
            string questName = (string)row[0];
            if (ScriptableQuest.dict.TryGetValue(questName, out ScriptableQuest questData))
            {
                Quest quest = new Quest(questData);
                quest.field0 = Convert.ToInt32((long)row[1]);
                quest.completed = ((long)row[2]) != 0; // sqlite has no bool
                quests.quests.Add(quest);
            }
            else Debug.LogWarning("LoadQuests: skipped quest " + questData.name + " for " + quests.name + " because it doesn't exist anymore. If it wasn't removed intentionally then make sure it's in the Resources folder.");
        }
    }

    GameObject LoadCharacter(SqliteConnection connection, List<GameObject> prefabs)
    {
        List< List<object> > table = ExecuteReader(connection, "SELECT * FROM character");
        if (table.Count == 1)
        {
            List<object> mainrow = table[0];

            // instantiate based on the class name
            string className = (string)mainrow[1];
            GameObject prefab = prefabs.Find(p => p.name == className);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab.gameObject);
                Player player = go.GetComponent<Player>();

                player.name                   = (string)mainrow[0];
                player.className              = (string)mainrow[1];
                float x                       = (float)mainrow[2];
                float y                       = (float)mainrow[3];
                float z                       = (float)mainrow[4];
                player.transform.position     = new Vector3(x, y, z);
                float yRotation               = (float)mainrow[5];
                player.transform.rotation     = Quaternion.Euler(0, yRotation, 0);
                player.level.current          = Convert.ToInt32((long)mainrow[6]);
                player.experience.current     = (long)mainrow[7];
                player.skills.skillExperience = (long)mainrow[8];
                int health                    = Convert.ToInt32((long)mainrow[9]);
                int mana                      = Convert.ToInt32((long)mainrow[10]);
                int endurance                 = Convert.ToInt32((long)mainrow[11]);
                player.inventory.gold         = (long)mainrow[12];

                LoadAttributes(connection, player);
                LoadInventory(connection, player.inventory);
                LoadEquipment(connection, player.equipment);
                LoadSkills(connection, player.skills);
                LoadBuffs(connection, player.skills);
                LoadQuests(connection, player.quests);

                // assign health / hydration etc. after max values were fully loaded
                // (they depend on equipment)
                player.health.current = health;
                player.mana.current = mana;
                player.endurance.current = endurance;

                return go;
            }
            else Debug.LogError("no prefab found for class: " + className);
        }
        return null;
    }

    void SaveAttributes(SqliteConnection connection, GameObject player)
    {
        PlayerAttribute[] attributes = player.GetComponents<PlayerAttribute>();

        // remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        ExecuteNonQuery(connection, "DELETE FROM character_attributes WHERE character=@character", new SqliteParameter("@character", player.name));
        foreach (PlayerAttribute attribute in attributes)
        {
            ExecuteNonQuery(connection,
                            "INSERT INTO character_attributes VALUES (@character, @name, @value)",
                            new SqliteParameter("@character", player.name),
                            new SqliteParameter("@name", attribute.GetType().ToString()),
                            new SqliteParameter("@value", attribute.value));
        }
    }

    void SaveInventory(SqliteConnection connection, PlayerInventory inventory)
    {
        // inventory: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        ExecuteNonQuery(connection, "DELETE FROM character_inventory WHERE character=@character", new SqliteParameter("@character", inventory.name));
        for (int i = 0; i < inventory.slots.Count; ++i)
        {
            ItemSlot slot = inventory.slots[i];
            if (slot.amount > 0) // only relevant items to save queries/storage/time
                ExecuteNonQuery(connection,
                                "INSERT INTO character_inventory VALUES (@character, @slot, @name, @amount)",
                                new SqliteParameter("@character", inventory.name),
                                new SqliteParameter("@slot", i),
                                new SqliteParameter("@name", slot.item.name),
                                new SqliteParameter("@amount", slot.amount));
        }
    }

    void SaveEquipment(SqliteConnection connection, PlayerEquipment equipment)
    {
        // equipment: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        ExecuteNonQuery(connection, "DELETE FROM character_equipment WHERE character=@character", new SqliteParameter("@character", equipment.name));
        for (int i = 0; i < equipment.slots.Count; ++i)
        {
            ItemSlot slot = equipment.slots[i];
            if (slot.amount > 0) // only relevant equip to save queries/storage/time
                ExecuteNonQuery(connection,
                                "INSERT INTO character_equipment VALUES (@character, @slot, @name, @amount)",
                                new SqliteParameter("@character", equipment.name),
                                new SqliteParameter("@slot", i),
                                new SqliteParameter("@name", slot.item.name),
                                new SqliteParameter("@amount", slot.amount));
        }
    }

    void SaveSkills(SqliteConnection connection, PlayerSkills skills)
    {
        // skills: remove old entries first, then add all new ones
        ExecuteNonQuery(connection, "DELETE FROM character_skills WHERE character=@character", new SqliteParameter("@character", skills.name));
        foreach (Skill skill in skills.skills)
            if (skill.level > 0) // only learned skills to save queries/storage/time
                // castTimeEnd and cooldownEnd are based on Time.time,
                // which will be different when restarting the server, so let's
                // convert them to the remaining time for easier save & load
                // note: this does NOT work when trying to save character data
                //       shortly before closing the editor or game because
                //       Time.time is 0 then.
                ExecuteNonQuery(connection,
                                "INSERT INTO character_skills VALUES (@character, @name, @level, @castTimeEnd, @cooldownEnd)",
                                new SqliteParameter("@character", skills.name),
                                new SqliteParameter("@name", skill.name),
                                new SqliteParameter("@level", skill.level),
                                new SqliteParameter("@castTimeEnd", skill.CastTimeRemaining()),
                                new SqliteParameter("@cooldownEnd", skill.CooldownRemaining()));
    }

    void SaveBuffs(SqliteConnection connection, PlayerSkills skills)
    {
        // buffs: remove old entries first, then add all new ones
        ExecuteNonQuery(connection, "DELETE FROM character_buffs WHERE character=@character", new SqliteParameter("@character", skills.name));
        foreach (Buff buff in skills.buffs)
            // buffTimeEnd is based on Time.time, which will be different
            // when restarting the server, so let's convert them to the
            // remaining time for easier save & load
            // note: this does NOT work when trying to save character data
            //       shortly before closing the editor or game because Time.time
            //       is 0 then.
            ExecuteNonQuery(connection,
                            "INSERT INTO character_buffs VALUES (@character, @name, @level, @buffTimeEnd)",
                            new SqliteParameter("@character", skills.name),
                            new SqliteParameter("@name", buff.name),
                            new SqliteParameter("@level", buff.level),
                            new SqliteParameter("@buffTimeEnd", buff.BuffTimeRemaining()));
    }

    void SaveQuests(SqliteConnection connection, PlayerQuests quests)
    {
        // quests: remove old entries first, then add all new ones
        ExecuteNonQuery(connection, "DELETE FROM character_quests WHERE character=@character", new SqliteParameter("@character", quests.name));
        foreach (Quest quest in quests.quests)
            ExecuteNonQuery(connection, "INSERT INTO character_quests VALUES (@character, @name, @field0, @completed)",
                            new SqliteParameter("@character", quests.name),
                            new SqliteParameter("@name", quest.name),
                            new SqliteParameter("@field0", quest.field0),
                            new SqliteParameter("@completed", Convert.ToInt32(quest.completed)));
    }

    // adds or overwrites character data in the database
    public void SaveCharacter(SqliteConnection connection, GameObject player)
    {
        // use transaction for performance
        ExecuteNonQuery(connection, "BEGIN");

        ExecuteNonQuery(connection,
                        "INSERT OR REPLACE INTO character VALUES (@name, @class, @x, @y, @z, @yrotation, @level, @experience, @skillExperience, @health, @mana, @endurance, @gold)",
                        new SqliteParameter("@name", player.name),
                        new SqliteParameter("@class", player.GetComponent<Player>().className),
                        new SqliteParameter("@x", player.transform.position.x),
                        new SqliteParameter("@y", player.transform.position.y),
                        new SqliteParameter("@z", player.transform.position.z),
                        new SqliteParameter("@yrotation", player.transform.rotation.eulerAngles.y),
                        new SqliteParameter("@level", player.GetComponent<Level>().current),
                        new SqliteParameter("@experience", player.GetComponent<Experience>().current),
                        new SqliteParameter("@skillExperience", player.GetComponent<PlayerSkills>().skillExperience),
                        new SqliteParameter("@health", player.GetComponent<Health>().current),
                        new SqliteParameter("@mana", player.GetComponent<Mana>().current),
                        new SqliteParameter("@endurance", player.GetComponent<Endurance>().current),
                        new SqliteParameter("@gold", player.GetComponent<PlayerInventory>().gold));

        SaveAttributes(connection, player);
        SaveInventory(connection, player.GetComponent<PlayerInventory>());
        SaveEquipment(connection, player.GetComponent<PlayerEquipment>());
        SaveSkills(connection, player.GetComponent<PlayerSkills>());
        SaveBuffs(connection, player.GetComponent<PlayerSkills>());
        SaveQuests(connection, player.GetComponent<PlayerQuests>());

        // end transaction
        ExecuteNonQuery(connection, "END");
    }

    // storage /////////////////////////////////////////////////////////////////
    public void LoadStorage(SqliteConnection connection, Storage storage)
    {
        // fill all slots first
        for (int i = 0; i < storage.size; ++i)
            storage.slots.Add(new ItemSlot());

        // then load valid items and put into their slots
        // (one big query is A LOT faster than querying each slot separately)
        List< List<object> > table = ExecuteReader(connection, "SELECT name, slot, amount FROM storages WHERE storage=@storage", new SqliteParameter("@storage", storage.name));
        foreach (List<object> row in table)
        {
            string itemName = (string)row[0];
            int slot = Convert.ToInt32((long)row[1]);
            if (slot < storage.size && ScriptableItem.dict.TryGetValue(itemName, out ScriptableItem itemData))
            {
                Item item = new Item(itemData);
                int amount = Convert.ToInt32((long)row[2]);
                storage.slots[slot] = new ItemSlot(item, amount);
            }
        }
    }

    void SaveStorage(SqliteConnection connection, Storage storage, bool useTransaction = true)
    {
        // only use a transaction if not called within SaveMany transaction
        if (useTransaction) ExecuteNonQuery(connection, "BEGIN");

        // storage: remove old entries first, then add all new ones
        // (we could use UPDATE where slot=... but deleting everything makes
        //  sure that there are never any ghosts)
        ExecuteNonQuery(connection, "DELETE FROM storages WHERE storage=@storage", new SqliteParameter("@storage", storage.name));
        for (int i = 0; i < storage.slots.Count; ++i)
        {
            ItemSlot slot = storage.slots[i];
            if (slot.amount > 0) // only relevant items to save queries/storage/time
                ExecuteNonQuery(connection,
                                "INSERT INTO storages VALUES (@storage, @slot, @name, @amount)",
                                new SqliteParameter("@storage", storage.name),
                                new SqliteParameter("@slot", i),
                                new SqliteParameter("@name", slot.item.name),
                                new SqliteParameter("@amount", slot.amount));
        }

        if (useTransaction) ExecuteNonQuery(connection, "END");
    }

    // save multiple storages at once (useful for ultra fast transactions)
    public void SaveStorages(SqliteConnection connection, List<Storage> storages)
    {
        ExecuteNonQuery(connection, "BEGIN"); // transaction for performance
        foreach (Storage storage in storages) SaveStorage(connection, storage, false);
        ExecuteNonQuery(connection, "END");
    }
}