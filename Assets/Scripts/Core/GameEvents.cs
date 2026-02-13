using System;
using UnityEngine;

namespace SurvivalGame.Core
{
    /// <summary>
    /// Central event bus for decoupled communication between systems.
    /// All events are static – subscribe in OnEnable, unsubscribe in OnDisable.
    /// </summary>
    public static class GameEvents
    {
        // ── Interaction ──
        public static event Action<GameObject> OnInteractableFound;
        public static event Action OnInteractableLost;
        public static event Action<GameObject> OnInteract;

        public static void RaiseInteractableFound(GameObject target) => OnInteractableFound?.Invoke(target);
        public static void RaiseInteractableLost() => OnInteractableLost?.Invoke();
        public static void RaiseInteract(GameObject target) => OnInteract?.Invoke(target);

        // ── Inventory (prepared for M2) ──
        public static event Action<string, int> OnItemAdded;       // itemId, amount
        public static event Action<string, int> OnItemRemoved;     // itemId, amount
        public static event Action OnInventoryChanged;

        public static void RaiseItemAdded(string itemId, int amount) => OnItemAdded?.Invoke(itemId, amount);
        public static void RaiseItemRemoved(string itemId, int amount) => OnItemRemoved?.Invoke(itemId, amount);
        public static void RaiseInventoryChanged() => OnInventoryChanged?.Invoke();

        // ── Player ──
        public static event Action<int> OnPlayerLevelUp;           // newLevel
        public static event Action<float, float> OnPlayerHealthChanged; // current, max

        public static void RaisePlayerLevelUp(int newLevel) => OnPlayerLevelUp?.Invoke(newLevel);
        public static void RaisePlayerHealthChanged(float current, float max) => OnPlayerHealthChanged?.Invoke(current, max);

        // ── Debug ──
        public static event Action<string> OnDebugMessage;
        public static void RaiseDebugMessage(string msg) => OnDebugMessage?.Invoke(msg);
    }
}
