using System.Collections.Generic;

namespace TheseusCore.Grid
{
    /// <summary>A placeable that hosts one or more power devices.</summary>
    public interface IPowerHost
    {
        /// <summary>Generator device, if present.</summary>
        IPowerGenerator Generator { get; }

        /// <summary>Consumer device, if present.</summary>
        IPowerConsumer Consumer { get; }

        /// <summary>Storage device, if present.</summary>
        IPowerStorage Storage { get; }
    }

    /// <summary>Base contract for any device participating in a power network.</summary>
    public interface IPowerNode
    {
        /// <summary>Role of this node in the network.</summary>
        ObjectType NodeType { get; }
    }

    /// <summary>A device that produces power into connected storage.</summary>
    public interface IPowerGenerator : IPowerNode
    {
        /// <summary>Called whenever the network topology changes.</summary>
        void SetNetwork(bool connected, IReadOnlyList<IPowerStorage> hubs);
    }

    /// <summary>A device that spends power from connected storage.</summary>
    public interface IPowerConsumer : IPowerNode
    {
        /// <summary>Called whenever the network topology changes.</summary>
        void SetNetwork(bool connected, IReadOnlyList<IPowerStorage> hubs);

        /// <summary>Attempts to withdraw power. Returns true when the operation may proceed.</summary>
        bool TryConsume(float amount, IReadOnlyList<IPowerStorage> hubs);
    }

    /// <summary>A battery or buffer that stores power for a network.</summary>
    public interface IPowerStorage : IPowerNode
    {
        /// <summary>Currently stored power.</summary>
        float StoredPower { get; }

        /// <summary>Removes power if available.</summary>
        bool TryWithdraw(float amount);

        /// <summary>Adds power up to capacity.</summary>
        void Deposit(float amount);
    }
}
