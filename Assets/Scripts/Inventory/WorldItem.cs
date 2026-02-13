using UnityEngine;
using SurvivalGame.Core;
using SurvivalGame.Items;

namespace SurvivalGame.Inventory
{
    /// <summary>
    /// A physical item in the world that can be picked up via interaction.
    /// Implements IInteractable for the raycast interact system.
    /// 
    /// Spawned when player drops items, or placed manually in the scene.
    /// Self-destructs after pickup.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WorldItem : MonoBehaviour, IInteractable
    {
        [Header("Item Data (set in Inspector or via Initialize)")]
        [SerializeField] private ItemDef _itemDef;
        [SerializeField] private int _amount = 1;
        [SerializeField] private float _durability = -1f;

        [Header("Settings")]
        [SerializeField] private float _pickupCooldown = 0.5f; // Prevents instant re-pickup after drop
        [SerializeField] private float _despawnTime = 300f; // 5 minutes, 0 = never

        private float _spawnTime;
        private bool _isInitialized;

        // IInteractable
        public string InteractionPrompt
        {
            get
            {
                if (_itemDef == null) return "Pick up ???";
                string amountStr = _amount > 1 ? $" x{_amount}" : "";
                return $"Pick up {_itemDef.displayName}{amountStr} [E]";
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            if (_itemDef == null) return false;
            if (Time.time - _spawnTime < _pickupCooldown) return false;
            return true;
        }

        public void Interact(GameObject interactor)
        {
            var inventory = interactor.GetComponent<InventoryController>();
            if (inventory == null)
            {
                Debug.LogWarning("[WorldItem] Interactor has no InventoryController!");
                return;
            }

            // Try to add to inventory
            int overflow = inventory.AddItem(_itemDef, _amount, _durability);

            if (overflow <= 0)
            {
                // All picked up
                Destroy(gameObject);
            }
            else
            {
                // Partially picked up
                _amount = overflow;
                Debug.Log($"[WorldItem] Partially picked up. {overflow} remaining.");
            }
        }

        /// <summary>
        /// Initialize from code (when dropping from inventory).
        /// </summary>
        public void Initialize(ItemDef itemDef, int amount, float durability = -1f)
        {
            _itemDef = itemDef;
            _amount = amount;
            _durability = durability;
            _isInitialized = true;
            _spawnTime = Time.time;
        }

        private void Awake()
        {
            if (!_isInitialized)
            {
                _spawnTime = Time.time;
            }

            // Ensure we have a collider
            var col = GetComponent<Collider>();
            if (col == null)
            {
                var box = gameObject.AddComponent<BoxCollider>();
                box.size = Vector3.one * 0.3f;
            }

            // Apply visual representation based on item type
            if (_itemDef != null)
            {
                WorldItemVisuals.ApplyVisuals(gameObject, _itemDef);
            }
        }

        private void Update()
        {
            // Despawn timer
            if (_despawnTime > 0 && Time.time - _spawnTime > _despawnTime)
            {
                Debug.Log($"[WorldItem] {_itemDef?.displayName} despawned (timeout).");
                Destroy(gameObject);
            }
        }

        // ── Editor visualization ──
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }
}
