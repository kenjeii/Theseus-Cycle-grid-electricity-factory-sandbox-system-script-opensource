using System.Collections.Generic;

namespace TheseusCore.Grid
{
    /// <summary>Minimal inventory contract required by crafting and drops.</summary>
    public interface IInventory
    {
        /// <summary>Current count of an item id.</summary>
        int Count(string id);

        /// <summary>Removes items if enough are present.</summary>
        bool Remove(string id, int amount);

        /// <summary>Adds items.</summary>
        void Add(string id, int amount);
    }

    /// <summary>One stack in a code-only inventory.</summary>
    [System.Serializable]
    public struct InventorySlotData
    {
        /// <summary>Item identifier.</summary>
        public string id;

        /// <summary>Stack size.</summary>
        public int amount;

        /// <summary>Creates a slot.</summary>
        public InventorySlotData(string id, int amount)
        {
            this.id = id;
            this.amount = amount;
        }
    }

    /// <summary>
    /// Allocation-conscious stack inventory with a stable slot list. Suitable for a hotbar
    /// or backpack; storage is code-only and can be serialized by any save layer.
    /// </summary>
    public sealed class InventoryService : IInventory
    {
        private readonly List<InventorySlotData> _slots = new List<InventorySlotData>();

        /// <summary>All slots in insertion order.</summary>
        public IReadOnlyList<InventorySlotData> Slots => _slots;

        /// <inheritdoc />
        public int Count(string id)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                InventorySlotData slot = _slots[i];
                if (slot.id == id) return slot.amount;
            }
            return 0;
        }

        /// <inheritdoc />
        public bool Remove(string id, int amount)
        {
            if (amount <= 0) return true;
            if (Count(id) < amount) return false;

            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].id != id) continue;
                InventorySlotData slot = _slots[i];
                slot.amount -= amount;
                if (slot.amount == 0) _slots.RemoveAt(i);
                else _slots[i] = slot;
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void Add(string id, int amount)
        {
            if (string.IsNullOrEmpty(id) || amount <= 0) return;

            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].id != id) continue;
                InventorySlotData slot = _slots[i];
                slot.amount += amount;
                _slots[i] = slot;
                return;
            }
            _slots.Add(new InventorySlotData(id, amount));
        }

        /// <summary>Removes every slot.</summary>
        public void Clear()
        {
            _slots.Clear();
        }
    }
}
