using UnityEngine;

namespace SurvivalGame.Items
{
    /// <summary>
    /// Tool type determines what resource nodes this tool can harvest.
    /// </summary>
    public enum ToolType
    {
        None,
        Axe,       // Trees, wood
        Pickaxe,   // Rocks, ore
        Hammer,    // Building, repair
        Sword,     // Combat
        Knife,     // Skinning, food prep
        Shovel     // Digging
    }

    /// <summary>
    /// Extended ItemDef for tools (axes, pickaxes, swords, etc.)
    /// Adds tool-specific stats: type, power, durability, efficiency.
    /// 
    /// Create via: Right-click → SurvivalGame → Items → Tool Definition
    /// </summary>
    [CreateAssetMenu(fileName = "NewTool", menuName = "SurvivalGame/Items/Tool Definition")]
    public class ToolDef : ItemDef
    {
        [Header("Tool Stats")]
        [Tooltip("What type of tool is this? Determines harvestable node types.")]
        public ToolType toolType = ToolType.None;

        [Tooltip("Base damage/harvest power per swing")]
        [Min(1f)]
        public float power = 10f;

        [Tooltip("Maximum durability. Tool breaks at 0.")]
        [Min(1f)]
        public float durabilityMax = 100f;

        [Tooltip("Gathering efficiency multiplier. Higher = more yield.")]
        [Min(0.1f)]
        public float efficiency = 1f;

        [Tooltip("Seconds between swings (attack speed)")]
        [Min(0.1f)]
        public float swingInterval = 1f;

        private void OnValidate()
        {
            // Auto-set some defaults for tools
            if (maxStack > 1) maxStack = 1; // Tools don't stack
            if (!HasTag("tool"))
            {
                var tagList = new System.Collections.Generic.List<string>(tags);
                tagList.Add("tool");
                tags = tagList.ToArray();
            }
        }
    }
}
