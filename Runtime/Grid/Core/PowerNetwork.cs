using System.Collections.Generic;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Plain C# connected component of power devices. Holds node classifications and pushes
    /// topology changes to the devices through their contracts.
    /// </summary>
    public sealed class PowerNetwork
    {
        /// <summary>Nodes currently belonging to this network.</summary>
        public readonly HashSet<IPlaceable> Nodes = new HashSet<IPlaceable>();

        private readonly List<IPlaceable> _generators = new List<IPlaceable>();
        private readonly List<IPlaceable> _consumers = new List<IPlaceable>();
        private readonly List<IPowerStorage> _hubs = new List<IPowerStorage>();
        private readonly List<IPowerStorage> _singleHub = new List<IPowerStorage>(1);

        /// <summary>Adds a node and classifies its attached devices.</summary>
        public void AddNode(IPlaceable placeable)
        {
            if (placeable == null || !Nodes.Add(placeable)) return;

            IPowerHost host = placeable as IPowerHost;
            if (host == null) return;

            if (host.Storage != null) _hubs.Add(host.Storage);
            if (host.Generator != null) _generators.Add(placeable);
            if (host.Consumer != null) _consumers.Add(placeable);
        }

        /// <summary>Removes a node and its device classification.</summary>
        public void RemoveNode(IPlaceable placeable)
        {
            if (placeable == null || !Nodes.Remove(placeable)) return;

            IPowerHost host = placeable as IPowerHost;
            if (host == null) return;

            if (host.Storage != null) _hubs.Remove(host.Storage);
            if (host.Generator != null) _generators.Remove(placeable);
            if (host.Consumer != null) _consumers.Remove(placeable);
        }

        /// <summary>Notifies every device of the current hub set.</summary>
        public void NotifyComponents()
        {
            bool connected = _hubs.Count > 0;
            _singleHub.Clear();
            if (connected) _singleHub.Add(_hubs[0]);

            for (int i = 0; i < _generators.Count; i++)
            {
                IPowerHost host = _generators[i] as IPowerHost;
                if (host != null && host.Generator != null) host.Generator.SetNetwork(connected, _singleHub);
            }

            for (int i = 0; i < _consumers.Count; i++)
            {
                IPowerHost host = _consumers[i] as IPowerHost;
                if (host != null && host.Consumer != null) host.Consumer.SetNetwork(connected, _hubs);
            }
        }
    }
}
