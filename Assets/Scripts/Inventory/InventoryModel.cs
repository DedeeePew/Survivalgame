using System;
using System.Collections.Generic;
using UnityEngine;
using SurvivalGame.Items;
using SurvivalGame.Core;

namespace SurvivalGame.Inventory
{
    /// <summary>
    /// Pure data model for an inventory. No MonoBehaviour, no UI.
    /// Manages a fixed number of slots, each holding an ItemStack or null.
    /// Supports: Add, Remove, Move, Split, Merge, weight calculation.
    /// 
    /// Design decisions:
    /// - Slot-based (array of nullable ItemStack)
    /// - Weight limit is optional (0 = unlimited)
    /// - Events fired for UI updates
    /// - All mutations go through this class (single source of truth)
    /// </summary>
    [Serializable]
    public class InventoryModel
    {
        public int SlotCount { get; private set; }
        public float MaxWeight { get; set; } // 0 = unlimited

        private ItemStack[] _slots;

        // Events
        public event Action OnChanged;
        public event Action<int> OnSlotChanged; // slot index

        public InventoryModel(int slotCount, float maxWeight = 0f)
        {
            SlotCount = slotCount;
            MaxWeight = maxWeight;
            _slots = new ItemStack[slotCount];
        }

        // ── Accessors ──

        public ItemStack GetSlot(int index)
        {
            if (index < 0 || index >= SlotCount) return null;
            var stack = _slots[index];
            return (stack != null && !stack.IsEmpty) ? stack : null;
        }

        public bool IsSlotEmpty(int index)
        {
            return GetSlot(index) == null;
        }

        public float CurrentWeight
        {
            get
            {
                float total = 0;
                for (int i = 0; i < SlotCount; i++)
                {
                    if (_slots[i] != null && !_slots[i].IsEmpty)
                        total += _slots[i].TotalWeight;
                }
                return total;
            }
        }

        public bool IsOverWeight => MaxWeight > 0 && CurrentWeight > MaxWeight;

        public int UsedSlots
        {
            get
            {
                int count = 0;
                for (int i = 0; i < SlotCount; i++)
                {
                    if (_slots[i] != null && !_slots[i].IsEmpty) count++;
                }
                return count;
            }
        }

        // ── Add Item ──

        /// <summary>
        /// Add items to inventory. First tries to stack, then uses empty slots.
        /// Returns the overflow amount (items that didn't fit).
        /// </summary>
        public int AddItem(ItemDef itemDef, int amount, float durability = -1f)
        {
            if (itemDef == null || amount <= 0) return amount;

            int remaining = amount;

            // Phase 1: Try to fill existing stacks (only for stackable, non-durability items)
            if (itemDef.maxStack > 1 && durability < 0)
            {
                for (int i = 0; i < SlotCount && remaining > 0; i++)
                {
                    var slot = _slots[i];
                    if (slot != null && !slot.IsEmpty && slot.ItemDef == itemDef && !slot.IsFull)
                    {
                        int overflow = slot.Add(remaining);
                        int added = remaining - overflow;
                        remaining = overflow;

                        if (added > 0) NotifySlotChanged(i);
                    }
                }
            }

            // Phase 2: Use empty slots
            while (remaining > 0)
            {
                int emptySlot = FindEmptySlot();
                if (emptySlot < 0) break; // No space

                int toPlace = Math.Min(remaining, itemDef.maxStack);
                _slots[emptySlot] = new ItemStack(itemDef, toPlace, durability);
                remaining -= toPlace;
                NotifySlotChanged(emptySlot);
            }

            if (remaining < amount)
            {
                GameEvents.RaiseItemAdded(itemDef.id, amount - remaining);
                NotifyChanged();
            }

            return remaining; // overflow
        }

        // ── Remove Item ──

        /// <summary>
        /// Remove a total amount of a specific item from the inventory.
        /// Returns the amount actually removed.
        /// </summary>
        public int RemoveItem(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return 0;

            int remaining = amount;

            for (int i = 0; i < SlotCount && remaining > 0; i++)
            {
                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty && slot.ItemId == itemId)
                {
                    int removed = slot.Remove(remaining);
                    remaining -= removed;

                    if (slot.IsEmpty) _slots[i] = null;
                    NotifySlotChanged(i);
                }
            }

            int totalRemoved = amount - remaining;
            if (totalRemoved > 0)
            {
                GameEvents.RaiseItemRemoved(itemId, totalRemoved);
                NotifyChanged();
            }

            return totalRemoved;
        }

        /// <summary>Remove all items from a specific slot. Returns the removed stack.</summary>
        public ItemStack RemoveSlot(int index)
        {
            if (index < 0 || index >= SlotCount) return null;
            var stack = _slots[index];
            if (stack == null || stack.IsEmpty) return null;

            _slots[index] = null;
            NotifySlotChanged(index);
            NotifyChanged();
            return stack;
        }

        /// <summary>Remove a specific amount from a slot. Returns a new stack with removed items.</summary>
        public ItemStack RemoveFromSlot(int index, int amount)
        {
            var slot = GetSlot(index);
            if (slot == null || amount <= 0) return null;

            if (amount >= slot.Amount)
            {
                return RemoveSlot(index);
            }

            var split = slot.Split(amount);
            if (split != null)
            {
                NotifySlotChanged(index);
                NotifyChanged();
            }
            return split;
        }

        // ── Move / Swap ──

        /// <summary>Move/swap items between two slots in this inventory.</summary>
        public void MoveSlot(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return;
            if (fromIndex < 0 || fromIndex >= SlotCount) return;
            if (toIndex < 0 || toIndex >= SlotCount) return;

            var from = _slots[fromIndex];
            var to = _slots[toIndex];

            // Empty source – nothing to do
            if (from == null || from.IsEmpty) return;

            // Empty target – simple move
            if (to == null || to.IsEmpty)
            {
                _slots[toIndex] = from;
                _slots[fromIndex] = null;
                NotifySlotChanged(fromIndex);
                NotifySlotChanged(toIndex);
                NotifyChanged();
                return;
            }

            // Same item type – try merge
            if (to.CanMergeWith(from))
            {
                int overflow = to.Add(from.Amount);
                if (overflow <= 0)
                {
                    _slots[fromIndex] = null;
                }
                else
                {
                    from.SetAmount(overflow);
                }
                NotifySlotChanged(fromIndex);
                NotifySlotChanged(toIndex);
                NotifyChanged();
                return;
            }

            // Different items – swap
            _slots[fromIndex] = to;
            _slots[toIndex] = from;
            NotifySlotChanged(fromIndex);
            NotifySlotChanged(toIndex);
            NotifyChanged();
        }

        // ── Split ──

        /// <summary>Split a stack: take splitAmount from slot and put in first empty slot.</summary>
        public bool SplitSlot(int slotIndex, int splitAmount)
        {
            var slot = GetSlot(slotIndex);
            if (slot == null || splitAmount <= 0 || splitAmount >= slot.Amount) return false;

            int emptySlot = FindEmptySlot();
            if (emptySlot < 0) return false; // No space

            var newStack = slot.Split(splitAmount);
            if (newStack == null) return false;

            _slots[emptySlot] = newStack;
            NotifySlotChanged(slotIndex);
            NotifySlotChanged(emptySlot);
            NotifyChanged();
            return true;
        }

        // ── Queries ──

        /// <summary>Count total amount of a specific item across all slots.</summary>
        public int CountItem(string itemId)
        {
            int total = 0;
            for (int i = 0; i < SlotCount; i++)
            {
                var slot = _slots[i];
                if (slot != null && !slot.IsEmpty && slot.ItemId == itemId)
                    total += slot.Amount;
            }
            return total;
        }

        /// <summary>Check if inventory has at least the given amount of an item.</summary>
        public bool HasItem(string itemId, int amount = 1)
        {
            return CountItem(itemId) >= amount;
        }

        /// <summary>Check if there's room to add at least one of this item.</summary>
        public bool CanAddItem(ItemDef itemDef, int amount = 1)
        {
            if (itemDef == null) return false;

            int remaining = amount;

            // Check existing stacks
            if (itemDef.maxStack > 1)
            {
                for (int i = 0; i < SlotCount && remaining > 0; i++)
                {
                    var slot = _slots[i];
                    if (slot != null && !slot.IsEmpty && slot.ItemDef == itemDef && !slot.IsFull)
                    {
                        remaining -= slot.FreeSpace;
                    }
                }
            }

            // Check empty slots
            if (remaining > 0)
            {
                int emptySlots = SlotCount - UsedSlots;
                remaining -= emptySlots * itemDef.maxStack;
            }

            return remaining <= 0;
        }

        // ── Helpers ──

        private int FindEmptySlot()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (_slots[i] == null || _slots[i].IsEmpty)
                    return i;
            }
            return -1;
        }

        /// <summary>Clean up empty stacks (set null if amount = 0).</summary>
        public void Cleanup()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (_slots[i] != null && _slots[i].IsEmpty)
                    _slots[i] = null;
            }
        }

        /// <summary>Clear the entire inventory.</summary>
        public void Clear()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                _slots[i] = null;
            }
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            OnChanged?.Invoke();
            GameEvents.RaiseInventoryChanged();
        }

        private void NotifySlotChanged(int index)
        {
            OnSlotChanged?.Invoke(index);
        }
    }
}
