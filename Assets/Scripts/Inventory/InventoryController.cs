using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Items;

namespace SurvivalGame.Inventory
{
    /// <summary>
    /// MonoBehaviour bridge between InventoryModel and the game world.
    /// Attach to the Player. Manages the player's inventory, 
    /// handles dropping items, and provides public API for other systems.
    /// 
    /// Design: The model is pure data, this controller connects it to Unity.
    /// </summary>
    public class InventoryController : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int _slotCount = 24;
        [SerializeField] private float _maxWeight = 100f; // kg, 0 = unlimited

        [Header("Drop Settings")]
        [SerializeField] private float _dropForce = 3f;
        [SerializeField] private float _dropUpForce = 2f;
        [SerializeField] private float _dropDistance = 1.5f;

        [Header("References")]
        [SerializeField] private ItemDatabase _itemDatabase;

        private InventoryModel _model;
        private Camera _cam;

        // Public access
        public InventoryModel Model => _model;
        public ItemDatabase Database => _itemDatabase;

        private void Awake()
        {
            _model = new InventoryModel(_slotCount, _maxWeight);
            _cam = GetComponentInChildren<Camera>();
            if (_cam == null) _cam = Camera.main;

            ServiceLocator.Register(this);

            if (_itemDatabase != null)
            {
                _itemDatabase.Initialize();
                ServiceLocator.Register(_itemDatabase);
            }
            else
            {
                Debug.LogWarning("[InventoryController] No ItemDatabase assigned! Items won't work.");
            }

            Debug.Log($"[InventoryController] Initialized: {_slotCount} slots, {_maxWeight}kg max weight");
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<InventoryController>();
            if (_itemDatabase != null)
                ServiceLocator.Unregister<ItemDatabase>();
        }

        // ── Public API ──

        /// <summary>Add item by ItemDef reference. Returns overflow.</summary>
        public int AddItem(ItemDef itemDef, int amount, float durability = -1f)
        {
            int overflow = _model.AddItem(itemDef, amount, durability);
            if (overflow < amount)
            {
                int added = amount - overflow;
                Debug.Log($"[Inventory] +{added}x {itemDef.displayName}");
                GameEvents.RaiseDebugMessage($"+{added}x {itemDef.displayName}");
            }
            if (overflow > 0)
            {
                Debug.Log($"[Inventory] No room for {overflow}x {itemDef.displayName}");
            }
            return overflow;
        }

        /// <summary>Add item by string id. Returns overflow.</summary>
        public int AddItem(string itemId, int amount, float durability = -1f)
        {
            if (_itemDatabase == null) return amount;
            var itemDef = _itemDatabase.GetById(itemId);
            if (itemDef == null) return amount;
            return AddItem(itemDef, amount, durability);
        }

        /// <summary>Remove items by id. Returns amount actually removed.</summary>
        public int RemoveItem(string itemId, int amount)
        {
            return _model.RemoveItem(itemId, amount);
        }

        /// <summary>Check if player has enough of an item.</summary>
        public bool HasItem(string itemId, int amount = 1)
        {
            return _model.HasItem(itemId, amount);
        }

        /// <summary>Drop an entire slot into the world.</summary>
        public void DropSlot(int slotIndex)
        {
            var stack = _model.RemoveSlot(slotIndex);
            if (stack == null) return;
            SpawnWorldItem(stack);
            Debug.Log($"[Inventory] Dropped {stack}");
        }

        /// <summary>Drop a specific amount from a slot.</summary>
        public void DropFromSlot(int slotIndex, int amount)
        {
            var stack = _model.RemoveFromSlot(slotIndex, amount);
            if (stack == null) return;
            SpawnWorldItem(stack);
            Debug.Log($"[Inventory] Dropped {stack}");
        }

        // ── World Item Spawning ──

        private void SpawnWorldItem(ItemStack stack)
        {
            if (stack == null || stack.IsEmpty) return;

            // Spawn position: in front of player
            if (_cam == null) _cam = GetComponentInChildren<Camera>();
            Vector3 forward = _cam != null ? _cam.transform.forward : transform.forward;
            Vector3 spawnPos = transform.position + forward * _dropDistance 
                             + Vector3.up * 0.5f;

            // Use the item's worldPrefab or create a default cube
            GameObject go;
            if (stack.ItemDef.worldPrefab != null)
            {
                go = Instantiate(stack.ItemDef.worldPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = spawnPos;
                go.transform.localScale = Vector3.one * 0.3f;

                // Color by rarity
                var renderer = go.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = stack.ItemDef.GetRarityColor();
            }

            go.name = $"WorldItem_{stack.ItemDef.id}";

            // Add WorldItem component
            var worldItem = go.GetComponent<WorldItem>();
            if (worldItem == null)
                worldItem = go.AddComponent<WorldItem>();
            worldItem.Initialize(stack.ItemDef, stack.Amount, stack.Durability);

            // Apply proper visuals (textures, shapes)
            WorldItemVisuals.ApplyVisuals(go, stack.ItemDef);

            // Add Rigidbody for physics
            var rb = go.GetComponent<Rigidbody>();
            if (rb == null)
                rb = go.AddComponent<Rigidbody>();
            rb.mass = stack.TotalWeight;

            // Throw forward
            Vector3 throwDir = forward * _dropForce + Vector3.up * _dropUpForce;
            rb.AddForce(throwDir, ForceMode.Impulse);
        }

        // ── Debug ──

        /// <summary>Give test items (for debugging). Call from console or debug UI.</summary>
        public void DebugAddTestItems()
        {
            if (_itemDatabase == null || _itemDatabase.allItems.Count == 0)
            {
                Debug.LogWarning("[InventoryController] No items in database to add.");
                return;
            }

            foreach (var item in _itemDatabase.allItems)
            {
                AddItem(item, 5);
            }
            Debug.Log("[InventoryController] Debug items added.");
        }
    }
}
