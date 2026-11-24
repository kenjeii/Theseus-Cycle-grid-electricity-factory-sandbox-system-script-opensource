using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Base class for devices that observe the hub set of their network. Holds no singleton
    /// reference; topology is pushed in by <see cref="PowerNetwork.NotifyComponents"/>.
    /// </summary>
    public abstract class PowerDevice : MonoBehaviour
    {
        /// <summary>Whether the current network contains at least one storage hub.</summary>
        protected bool Connected { get; private set; }

        /// <summary>Hubs available to this device. The list is owned by the network.</summary>
        protected IReadOnlyList<IPowerStorage> Hubs { get; private set; }

        /// <summary>Applies a topology snapshot.</summary>
        public virtual void SetNetwork(bool connected, IReadOnlyList<IPowerStorage> hubs)
        {
            Connected = connected;
            Hubs = hubs;
        }
    }

    /// <summary>Generates power into the first hub of its network.</summary>
    public sealed class GeneratorComponent : PowerDevice, IPowerGenerator
    {
        [SerializeField] private float _unitsPerSecond = 2f;

        /// <inheritdoc />
        public ObjectType NodeType => ObjectType.Generator;

        /// <summary>Generation rate in units per second.</summary>
        public float UnitsPerSecond => _unitsPerSecond;

        /// <summary>Deposits one tick of generation into the connected hub.</summary>
        public void Generate(float deltaTime)
        {
            if (!Connected || Hubs == null || Hubs.Count == 0) return;
            Hubs[0].Deposit(_unitsPerSecond * Mathf.Max(0f, deltaTime));
        }

        /// <summary>Deposits a fixed amount into the connected hub.</summary>
        public void GenerateFixed(float amount)
        {
            if (!Connected || Hubs == null || Hubs.Count == 0) return;
            Hubs[0].Deposit(Mathf.Max(0f, amount));
        }
    }

    /// <summary>Variant generator with heat or charge-driven output.</summary>
    public sealed class NuclearGeneratorComponent : PowerDevice, IPowerGenerator
    {
        [SerializeField] private float _outputPerTick = 50f;

        /// <inheritdoc />
        public ObjectType NodeType => ObjectType.Generator;

        /// <summary>Deposits one fixed batch into the connected hub.</summary>
        public void GenerateBatch()
        {
            if (!Connected || Hubs == null || Hubs.Count == 0) return;
            Hubs[0].Deposit(Mathf.Max(0f, _outputPerTick));
        }
    }

    /// <summary>Consumes power from any hub with enough charge.</summary>
    public sealed class ConsumerComponent : PowerDevice, IPowerConsumer
    {
        /// <inheritdoc />
        public ObjectType NodeType => ObjectType.Consumer;

        /// <inheritdoc />
        public bool TryConsume(float amount, IReadOnlyList<IPowerStorage> hubs)
        {
            IReadOnlyList<IPowerStorage> sources = hubs ?? Hubs;
            if (!Connected || sources == null || amount <= 0f) return false;

            float remaining = amount;
            for (int i = 0; i < sources.Count; i++)
            {
                IPowerStorage storage = sources[i];
                if (storage == null || !storage.TryWithdraw(remaining)) continue;
                return true;
            }
            return false;
        }
    }

    /// <summary>Battery that stores power and caps at a configurable capacity.</summary>
    public sealed class PowerStorageComponent : PowerDevice, IPowerStorage
    {
        [SerializeField] private float _capacity = 500f;
        [SerializeField] private float _storedPower;

        /// <inheritdoc />
        public ObjectType NodeType => ObjectType.Storage;

        /// <inheritdoc />
        public float StoredPower => _storedPower;

        /// <summary>Maximum charge.</summary>
        public float Capacity => Mathf.Max(0f, _capacity);

        /// <inheritdoc />
        public bool TryWithdraw(float amount)
        {
            if (amount < 0f || amount > _storedPower) return false;
            _storedPower -= amount;
            return true;
        }

        /// <inheritdoc />
        public void Deposit(float amount)
        {
            if (amount <= 0f) return;
            _storedPower = Mathf.Min(Capacity, _storedPower + amount);
        }
    }

    /// <summary>Generator that produces power in response to external triggers.</summary>
    public sealed class DummyGeneratorComponent : PowerDevice, IPowerGenerator
    {
        [SerializeField] private float _powerPerTrigger = 50f;

        /// <inheritdoc />
        public ObjectType NodeType => ObjectType.Generator;

        /// <summary>Adds one trigger worth of power to the connected hub.</summary>
        public void Trigger()
        {
            if (!Connected || Hubs == null || Hubs.Count == 0) return;
            Hubs[0].Deposit(Mathf.Max(0f, _powerPerTrigger));
        }
    }
}
