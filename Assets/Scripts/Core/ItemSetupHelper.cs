#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using SurvivalGame.Items;

namespace SurvivalGame.Core.Editor
{
    /// <summary>
    /// Editor helper: Creates example ItemDefs and ItemDatabase for testing.
    /// Now auto-assigns SEP prefabs and sprites when available.
    /// Menu: SurvivalGame → Create Example Items
    /// </summary>
    public static class ItemSetupHelper
    {
        private const string ITEM_PATH = "Assets/ScriptableObjects/Items/";
        private const string DB_PATH = "Assets/ScriptableObjects/";

        // SEP Asset paths
        private const string SEP_PREFABS = "Assets/PolymindGames/SEP/Prefabs/";
        private const string SEP_SPRITES = "Assets/PolymindGames/SEP/Sprites/";

        [MenuItem("SurvivalGame/Create Example Items (M2)")]
        public static void CreateExampleItems()
        {
            // Ensure folders exist
            EnsureFolder("Assets/ScriptableObjects");
            EnsureFolder(ITEM_PATH);

            // ── Delete old items first to force fresh creation ──
            string[] oldAssets = AssetDatabase.FindAssets("t:ItemDef", new[] { ITEM_PATH.TrimEnd('/') });
            foreach (string guid in oldAssets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"  [Cleanup] Deleted old: {path}");
            }
            // Delete old database
            if (AssetDatabase.LoadAssetAtPath<ItemDatabase>(DB_PATH + "ItemDatabase.asset") != null)
            {
                AssetDatabase.DeleteAsset(DB_PATH + "ItemDatabase.asset");
                Debug.Log("  [Cleanup] Deleted old ItemDatabase");
            }
            AssetDatabase.Refresh();

            // ── Resources ──
            var wood = CreateItem<ItemDef>("item_wood", "Wood", "Basic wood log. Used for building and crafting.", 
                20, 1f, ItemRarity.Common, new[] { "resource", "wood" });
            AssignSEPAssets(wood, "ResourceItems/SEP_Log", "ResourceItems/SEP_WoodLog", null,
                new Vector3(0f, 0.02f, 0.01f), new Vector3(0f, 0f, 15f), 0.25f);
            
            var stone = CreateItem<ItemDef>("item_stone", "Stone", "A chunk of stone. Used for tools and building.", 
                20, 2f, ItemRarity.Common, new[] { "resource", "stone" });
            AssignSEPAssets(stone, "ResourceItems/SEP_Stone", "ResourceItems/SEP_StoneShard", null,
                new Vector3(0f, 0.02f, 0f), Vector3.zero, 0.3f);
            
            var ironOre = CreateItem<ItemDef>("item_iron_ore", "Iron Ore", "Raw iron ore. Smelt at a campfire.", 
                10, 3f, ItemRarity.Uncommon, new[] { "resource", "ore" });
            AssignSEPAssets(ironOre, "ResourceItems/SEP_StoneShard", "ResourceItems/SEP_MetalOre", null,
                new Vector3(0f, 0.02f, 0f), Vector3.zero, 0.3f);
            
            var ironIngot = CreateItem<ItemDef>("item_iron_ingot", "Iron Ingot", "Smelted iron. Used for advanced crafting.", 
                10, 4f, ItemRarity.Uncommon, new[] { "resource", "metal" });
            AssignSEPAssets(ironIngot, "ResourceItems/SEP_MetalSheet", "ResourceItems/SEP_IronIngot", null,
                new Vector3(0f, 0.02f, 0f), Vector3.zero, 0.25f);
            
            var plank = CreateItem<ItemDef>("item_plank", "Plank", "Processed wood plank. Used for building.", 
                20, 0.8f, ItemRarity.Common, new[] { "resource", "wood", "processed" });
            AssignSEPAssets(plank, "ResourceItems/SEP_Stick", "ResourceItems/SEP_WoodPlank", null,
                new Vector3(0f, 0.02f, 0.01f), new Vector3(0f, 0f, 10f), 0.25f);
            
            var fiber = CreateItem<ItemDef>("item_fiber", "Plant Fiber", "Fibrous plant material. Used for rope and cloth.", 
                30, 0.2f, ItemRarity.Common, new[] { "resource", "fiber" });
            AssignSEPAssets(fiber, "ResourceItems/SEP_Cloth", "ResourceItems/SEP_Cloth", null,
                new Vector3(0f, 0.02f, 0f), Vector3.zero, 0.3f);

            var leather = CreateItem<ItemDef>("item_leather", "Leather", "Tanned animal hide.", 
                10, 0.5f, ItemRarity.Uncommon, new[] { "resource", "leather" });
            AssignSEPAssets(leather, "ResourceItems/SEP_Leather", "ResourceItems/SEP_Leather", null,
                new Vector3(0f, 0.02f, 0f), Vector3.zero, 0.3f);

            // ── Food ──
            var meat = CreateItem<ItemDef>("item_raw_meat", "Raw Meat", "Uncooked meat. Cook at a campfire!", 
                5, 0.5f, ItemRarity.Common, new[] { "food", "raw" });
            AssignSEPAssets(meat, "Food/SEP_RawMeat", "Food/SEP_RawMeat", null,
                new Vector3(0f, 0.02f, 0.01f), Vector3.zero, 0.3f);
            
            var cookedMeat = CreateItem<ItemDef>("item_cooked_meat", "Cooked Meat", "Delicious cooked meat. Restores health.", 
                5, 0.4f, ItemRarity.Common, new[] { "food", "cooked" });
            AssignSEPAssets(cookedMeat, "Food/SEP_CookedMeat", "Food/SEP_CookedMeat", null,
                new Vector3(0f, 0.02f, 0.01f), Vector3.zero, 0.3f);

            var berries = CreateItem<ItemDef>("item_berries", "Berries", "Wild berries. A small snack.", 
                20, 0.1f, ItemRarity.Common, new[] { "food" });
            AssignSEPAssets(berries, "Food/SEP_Apple", "Food/SEP_Blueberries", null,
                new Vector3(0f, 0.02f, 0f), Vector3.zero, 0.3f);

            // ── Tools ──
            var stoneAxe = CreateTool("tool_stone_axe", "Stone Axe", "A crude axe made of stone and wood.", 
                ItemRarity.Common, 2.5f, ToolType.Axe, 8f, 80f, 1f, 1.2f);
            AssignSEPAssets(stoneAxe, "Tools/SEP_StoneHatchet", "Tools/SEP_StoneHatchet",
                "Tools/SEP_StoneHatchet",
                new Vector3(0f, 0f, 0.02f), new Vector3(-30f, 90f, 0f), 0.35f);
            
            var stonePickaxe = CreateTool("tool_stone_pickaxe", "Stone Pickaxe", "A basic pickaxe for mining.", 
                ItemRarity.Common, 3f, ToolType.Pickaxe, 8f, 80f, 1f, 1.3f);
            AssignSEPAssets(stonePickaxe, "Tools/SEP_StonePickaxe", "Tools/SEP_StonePickaxe",
                "Tools/SEP_StonePickaxe",
                new Vector3(0f, 0f, 0.02f), new Vector3(-30f, 90f, 0f), 0.35f);
            
            var ironAxe = CreateTool("tool_iron_axe", "Iron Axe", "A sturdy iron axe. Chops trees faster.", 
                ItemRarity.Uncommon, 3f, ToolType.Axe, 15f, 150f, 1.5f, 1f);
            AssignSEPAssets(ironAxe, "Tools/SEP_MetalHatchet", "Tools/SEP_MetalHatchet",
                "Tools/SEP_Axe",
                new Vector3(0f, 0f, 0.02f), new Vector3(-30f, 90f, 0f), 0.35f);
            
            var ironPickaxe = CreateTool("tool_iron_pickaxe", "Iron Pickaxe", "An iron pickaxe. Mines efficiently.", 
                ItemRarity.Uncommon, 3.5f, ToolType.Pickaxe, 15f, 150f, 1.5f, 1.1f);
            AssignSEPAssets(ironPickaxe, "Tools/SEP_SteelPickaxe", "Tools/SEP_SteelPickaxe",
                "Tools/SEP_SteelPickaxe",
                new Vector3(0f, 0f, 0.02f), new Vector3(-30f, 90f, 0f), 0.35f);

            var stoneSword = CreateTool("tool_stone_sword", "Stone Sword", "A crude stone blade.", 
                ItemRarity.Common, 2f, ToolType.Sword, 12f, 60f, 1f, 0.8f);
            AssignSEPAssets(stoneSword, "Weapons/Melee/SEP_Machete", "Weapons/Melee/SEP_Machete",
                "Weapons/Melee/SEP_Machete",
                new Vector3(0f, 0.01f, 0.04f), new Vector3(-25f, 90f, 0f), 0.35f);

            var buildHammer = CreateTool("tool_build_hammer", "Build Hammer", "Used to build and repair structures.", 
                ItemRarity.Common, 1.5f, ToolType.Hammer, 5f, 200f, 1f, 1f);
            AssignSEPAssets(buildHammer, "Tools/SEP_BuildingHammer", "Tools/SEP_BuildingHammer",
                "Tools/SEP_BuildingHammer",
                new Vector3(0f, 0f, 0.02f), new Vector3(-30f, 90f, 0f), 0.35f);

            // ── Create Database ──
            var db = ScriptableObject.CreateInstance<ItemDatabase>();
            db.allItems = new System.Collections.Generic.List<ItemDef>
            {
                wood, stone, ironOre, ironIngot, plank, fiber, leather,
                meat, cookedMeat, berries,
                stoneAxe, stonePickaxe, ironAxe, ironPickaxe, stoneSword, buildHammer
            };

            string dbAssetPath = DB_PATH + "ItemDatabase.asset";
            AssetDatabase.CreateAsset(db, dbAssetPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ItemSetupHelper] ✅ Created {db.allItems.Count} items + ItemDatabase with SEP assets!");
            EditorUtility.DisplayDialog("Items Created",
                $"{db.allItems.Count} Items erstellt!\n\n" +
                "SEP Prefabs & Sprites zugewiesen!\n\n" +
                "ScriptableObjects: Assets/ScriptableObjects/Items/\n" +
                "ItemDatabase: Assets/ScriptableObjects/ItemDatabase\n\n" +
                "Nächster Schritt:\n" +
                "SurvivalGame → Setup Test Scene (M2)",
                "OK");
        }

        /// <summary>
        /// Assigns SEP prefab and sprite assets to an ItemDef.
        /// Tries worldPrefab, icon sprite, and optionally heldPrefab from SEP paths.
        /// </summary>
        private static void AssignSEPAssets(ItemDef item, string worldPrefabPath, string spritePath,
            string heldPrefabPath, Vector3 heldPosOffset, Vector3 heldRotOffset, float heldScale)
        {
            // World Prefab
            if (!string.IsNullOrEmpty(worldPrefabPath))
            {
                string fullPath = SEP_PREFABS + worldPrefabPath + ".prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
                if (prefab != null)
                {
                    item.worldPrefab = prefab;
                    Debug.Log($"  [SEP] {item.id} → worldPrefab: {worldPrefabPath}");
                }
                else
                {
                    Debug.LogWarning($"  [SEP] Prefab not found: {fullPath}");
                }
            }

            // Held Prefab (use separate prefab or fall back to world prefab)
            if (!string.IsNullOrEmpty(heldPrefabPath))
            {
                string fullPath = SEP_PREFABS + heldPrefabPath + ".prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
                if (prefab != null)
                {
                    item.heldPrefab = prefab;
                    Debug.Log($"  [SEP] {item.id} → heldPrefab: {heldPrefabPath}");
                }
            }

            // Sprite/Icon
            if (!string.IsNullOrEmpty(spritePath))
            {
                string fullPath = SEP_SPRITES + spritePath + ".png";
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
                
                // Try PNG extension (case insensitive check)
                if (sprite == null)
                {
                    fullPath = SEP_SPRITES + spritePath + ".PNG";
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
                }

                if (sprite != null)
                {
                    item.icon = sprite;
                    Debug.Log($"  [SEP] {item.id} → icon: {spritePath}");
                }
                else
                {
                    Debug.LogWarning($"  [SEP] Sprite not found: {SEP_SPRITES + spritePath}");
                }
            }

            // Held item grip data
            item.heldPositionOffset = heldPosOffset;
            item.heldRotationOffset = heldRotOffset;
            item.heldScale = heldScale;

            EditorUtility.SetDirty(item);
        }

        private static ItemDef CreateItem<T>(string id, string displayName, string desc,
            int maxStack, float weight, ItemRarity rarity, string[] tags) where T : ItemDef
        {
            var item = ScriptableObject.CreateInstance<T>();
            item.id = id;
            item.displayName = displayName;
            item.description = desc;
            item.maxStack = maxStack;
            item.weight = weight;
            item.rarity = rarity;
            item.tags = tags;

            string path = ITEM_PATH + id + ".asset";
            AssetDatabase.CreateAsset(item, path);
            return item;
        }

        private static ToolDef CreateTool(string id, string displayName, string desc,
            ItemRarity rarity, float weight, ToolType toolType, float power, 
            float durability, float efficiency, float swingInterval)
        {
            var tool = ScriptableObject.CreateInstance<ToolDef>();
            tool.id = id;
            tool.displayName = displayName;
            tool.description = desc;
            tool.maxStack = 1;
            tool.weight = weight;
            tool.rarity = rarity;
            tool.tags = new[] { "tool" };
            tool.toolType = toolType;
            tool.power = power;
            tool.durabilityMax = durability;
            tool.efficiency = efficiency;
            tool.swingInterval = swingInterval;

            string path = ITEM_PATH + id + ".asset";
            AssetDatabase.CreateAsset(tool, path);
            return tool;
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
#endif
