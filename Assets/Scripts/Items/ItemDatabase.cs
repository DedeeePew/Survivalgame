using System.Collections.Generic;
using UnityEngine;

namespace SurvivalGame.Items
{
    /// <summary>
    /// Runtime registry of all ItemDefs. Loads all ItemDef assets from Resources or a list.
    /// Used for lookups by id (e.g. from save files, loot tables, crafting).
    /// Attach to GameManager or use as ScriptableObject singleton.
    /// 
    /// Design: For now uses a serialized list. Later can auto-load from Addressables.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "SurvivalGame/Items/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Tooltip("All item definitions in the game. Add them here.")]
        public List<ItemDef> allItems = new();

        private Dictionary<string, ItemDef> _lookup;

        /// <summary>Build the lookup dictionary. Call once on game start.</summary>
        public void Initialize()
        {
            _lookup = new Dictionary<string, ItemDef>();
            foreach (var item in allItems)
            {
                if (item == null) continue;
                if (_lookup.ContainsKey(item.id))
                {
                    Debug.LogWarning($"[ItemDatabase] Duplicate item id: '{item.id}' – skipping {item.name}");
                    continue;
                }
                _lookup[item.id] = item;
            }
            Debug.Log($"[ItemDatabase] Initialized with {_lookup.Count} items.");
        }

        /// <summary>Get an ItemDef by its unique id.</summary>
        public ItemDef GetById(string id)
        {
            if (_lookup == null) Initialize();
            if (_lookup.TryGetValue(id, out var item)) return item;
            Debug.LogWarning($"[ItemDatabase] Item not found: '{id}'");
            return null;
        }

        /// <summary>Check if an item id exists.</summary>
        public bool Exists(string id)
        {
            if (_lookup == null) Initialize();
            return _lookup.ContainsKey(id);
        }

        /// <summary>Get all items with a specific tag.</summary>
        public List<ItemDef> GetByTag(string tag)
        {
            var results = new List<ItemDef>();
            foreach (var item in allItems)
            {
                if (item != null && item.HasTag(tag))
                    results.Add(item);
            }
            return results;
        }
    }
}
