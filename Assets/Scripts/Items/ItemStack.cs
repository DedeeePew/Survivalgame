using System;

namespace SurvivalGame.Items
{
    /// <summary>
    /// Runtime representation of an item stack in an inventory slot.
    /// This is a plain C# class (not MonoBehaviour) for clean data separation.
    /// 
    /// Design decisions:
    /// - Immutable reference to ItemDef (what kind of item)
    /// - Mutable amount and durability (runtime state)
    /// - durability = -1 means "no durability" (resources, non-tools)
    /// - Clone() for split operations
    /// </summary>
    [Serializable]
    public class ItemStack
    {
        public ItemDef ItemDef { get; private set; }
        public int Amount { get; private set; }
        public float Durability { get; private set; } // -1 = no durability

        public ItemStack(ItemDef itemDef, int amount, float durability = -1f)
        {
            ItemDef = itemDef;
            Amount = Math.Max(0, amount);
            Durability = durability;
        }

        // ── Properties ──

        public string ItemId => ItemDef != null ? ItemDef.id : "null";
        public bool IsEmpty => Amount <= 0 || ItemDef == null;
        public bool IsFull => Amount >= MaxStack;
        public int MaxStack => ItemDef != null ? ItemDef.maxStack : 1;
        public float TotalWeight => ItemDef != null ? ItemDef.weight * Amount : 0f;
        public bool HasDurability => Durability >= 0f;
        public int FreeSpace => MaxStack - Amount;

        // ── Mutations ──

        /// <summary>Add amount to this stack. Returns overflow (amount that didn't fit).</summary>
        public int Add(int count)
        {
            if (count <= 0) return 0;

            int canFit = FreeSpace;
            int toAdd = Math.Min(count, canFit);
            Amount += toAdd;
            return count - toAdd; // overflow
        }

        /// <summary>Remove amount from this stack. Returns actually removed count.</summary>
        public int Remove(int count)
        {
            if (count <= 0) return 0;

            int toRemove = Math.Min(count, Amount);
            Amount -= toRemove;
            return toRemove;
        }

        /// <summary>Set amount directly (for loading saves, etc.)</summary>
        public void SetAmount(int amount)
        {
            Amount = Math.Max(0, amount);
        }

        /// <summary>Reduce durability. Returns true if broken (durability <= 0).</summary>
        public bool ReduceDurability(float amount)
        {
            if (!HasDurability) return false;

            Durability = Math.Max(0f, Durability - amount);
            return Durability <= 0f;
        }

        /// <summary>Can this stack merge with another of the same item?</summary>
        public bool CanMergeWith(ItemStack other)
        {
            if (other == null || other.IsEmpty) return false;
            if (ItemDef != other.ItemDef) return false;
            if (IsFull) return false;
            // Don't merge items with different durability
            if (HasDurability || other.HasDurability) return false;
            return true;
        }

        /// <summary>
        /// Split this stack: removes splitAmount and returns a new stack.
        /// Returns null if split is invalid.
        /// </summary>
        public ItemStack Split(int splitAmount)
        {
            if (splitAmount <= 0 || splitAmount >= Amount) return null;

            Amount -= splitAmount;
            return new ItemStack(ItemDef, splitAmount, Durability);
        }

        /// <summary>Create a copy of this stack.</summary>
        public ItemStack Clone()
        {
            return new ItemStack(ItemDef, Amount, Durability);
        }

        public override string ToString()
        {
            string durStr = HasDurability ? $" [{Durability:F0}hp]" : "";
            return $"{ItemDef?.displayName ?? "null"} x{Amount}{durStr}";
        }
    }
}
