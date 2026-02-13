using UnityEngine;

namespace SurvivalGame.Items
{
    /// <summary>
    /// Rarity levels for items. Affects loot roll weights and UI coloring.
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Base ScriptableObject defining an item type.
    /// Create via: Right-click → SurvivalGame → Items → Item Definition
    /// 
    /// Design decisions:
    /// - id is a unique string (not int) for readability in save files
    /// - tags allow flexible filtering (e.g. "resource", "tool", "food", "building")
    /// - weight is per single item, total = weight * stack count
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "SurvivalGame/Items/Item Definition")]
    public class ItemDef : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique string ID. Must be unique across all items.")]
        public string id = "item_unnamed";

        [Tooltip("Display name shown in UI")]
        public string displayName = "Unnamed Item";

        [TextArea(2, 4)]
        [Tooltip("Description shown in tooltip")]
        public string description = "";

        [Tooltip("Icon for inventory UI (can be null for now)")]
        public Sprite icon;

        [Header("Stacking & Weight")]
        [Tooltip("Maximum stack size. 1 = not stackable")]
        [Min(1)]
        public int maxStack = 20;

        [Tooltip("Weight per single item in kg")]
        [Min(0f)]
        public float weight = 0.5f;

        [Header("Classification")]
        public ItemRarity rarity = ItemRarity.Common;

        [Tooltip("Flexible tags for filtering: resource, tool, food, building, weapon, armor")]
        public string[] tags = new string[0];

        [Header("World")]
        [Tooltip("Prefab spawned when item is dropped in world. If null, uses default cube.")]
        public GameObject worldPrefab;

        [Tooltip("Prefab used for first-person held view. If null, uses worldPrefab or procedural fallback.")]
        public GameObject heldPrefab;

        [Header("Held Item Grip")]
        [Tooltip("Position offset when held in hand (relative to grip point).")]
        public Vector3 heldPositionOffset = Vector3.zero;
        [Tooltip("Rotation offset when held in hand.")]
        public Vector3 heldRotationOffset = Vector3.zero;
        [Tooltip("Scale when held in hand.")]
        public float heldScale = 1f;

        /// <summary>Check if this item has a specific tag.</summary>
        public bool HasTag(string tag)
        {
            for (int i = 0; i < tags.Length; i++)
            {
                if (tags[i] == tag) return true;
            }
            return false;
        }

        /// <summary>Get the UI color for this item's rarity.</summary>
        public Color GetRarityColor()
        {
            return rarity switch
            {
                ItemRarity.Common => Color.white,
                ItemRarity.Uncommon => Color.green,
                ItemRarity.Rare => new Color(0.3f, 0.5f, 1f),  // Blue
                ItemRarity.Epic => new Color(0.6f, 0.2f, 0.8f), // Purple
                ItemRarity.Legendary => new Color(1f, 0.65f, 0f), // Orange
                _ => Color.white
            };
        }
    }
}
