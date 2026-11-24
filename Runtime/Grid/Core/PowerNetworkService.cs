using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Pure C# power topology service. Adding a node merges adjacent networks in one pass;
    /// removing a node only marks its network dirty, and a single BFS rebuild per frame
    /// reconnects the remaining nodes into connected components. All buffers are reused.
    /// </summary>
    public sealed class PowerNetworkService
    {
        private readonly GridPlacementService _grid;
        private readonly Dictionary<IPlaceable, PowerNetwork> _objectToNetwork =
            new Dictionary<IPlaceable, PowerNetwork>();
        private readonly List<PowerNetwork> _activeNetworks = new List<PowerNetwork>();
        private readonly HashSet<PowerNetwork> _dirtyNetworks = new HashSet<PowerNetwork>();

        private readonly List<IPlaceable> _neighbours = new List<IPlaceable>();
        private readonly HashSet<IPlaceable> _seen = new HashSet<IPlaceable>();
        private readonly List<PowerNetwork> _adjacent = new List<PowerNetwork>();
        private readonly HashSet<PowerNetwork> _adjacentSet = new HashSet<PowerNetwork>();
        private readonly HashSet<IPlaceable> _remaining = new HashSet<IPlaceable>();
        private readonly Queue<IPlaceable> _queue = new Queue<IPlaceable>();
        private readonly HashSet<IPlaceable> _localSeen = new HashSet<IPlaceable>();

        /// <summary>Creates a service bound to a placement service.</summary>
        public PowerNetworkService(GridPlacementService grid)
        {
            _grid = grid;
        }

        /// <summary>Number of live networks.</summary>
        public int NetworkCount => _activeNetworks.Count;

        /// <summary>Returns the network containing a node, if any.</summary>
        public bool TryGetNetwork(IPlaceable placeable, out PowerNetwork network)
        {
            return _objectToNetwork.TryGetValue(placeable, out network);
        }

        /// <summary>Registers a node and merges all adjacent networks.</summary>
        public void AddNode(IPlaceable placeable)
        {
            if (placeable == null || placeable.ObjectType == ObjectType.None) return;

            _grid.CollectNeighbours(placeable, _neighbours, _seen);
            _adjacent.Clear();
            _adjacentSet.Clear();

            for (int i = 0; i < _neighbours.Count; i++)
            {
                PowerNetwork network;
                if (!_objectToNetwork.TryGetValue(_neighbours[i], out network)) continue;
                if (!_adjacentSet.Add(network)) continue;
                _adjacent.Add(network);
            }

            if (_adjacent.Count == 0)
            {
                PowerNetwork created = new PowerNetwork();
                created.AddNode(placeable);
                _objectToNetwork[placeable] = created;
                _activeNetworks.Add(created);
                created.NotifyComponents();
                return;
            }

            PowerNetwork main = _adjacent[0];
            main.AddNode(placeable);
            _objectToNetwork[placeable] = main;

            for (int i = 1; i < _adjacent.Count; i++)
            {
                PowerNetwork absorbed = _adjacent[i];
                foreach (IPlaceable node in absorbed.Nodes)
                {
                    main.AddNode(node);
                    _objectToNetwork[node] = main;
                }
                _activeNetworks.Remove(absorbed);
                _dirtyNetworks.Remove(absorbed);
            }

            main.NotifyComponents();
        }

        /// <summary>Unregisters a node and defers reconnection to the next rebuild.</summary>
        public void RemoveNode(IPlaceable placeable)
        {
            if (placeable == null) return;

            PowerNetwork network;
            if (!_objectToNetwork.TryGetValue(placeable, out network)) return;

            network.RemoveNode(placeable);
            _objectToNetwork.Remove(placeable);
            _dirtyNetworks.Add(network);
        }

        /// <summary>Rebuilds all dirty networks. Call once per frame from LateUpdate.</summary>
        public void RebuildDirtyNetworks()
        {
            if (_dirtyNetworks.Count == 0) return;

            foreach (PowerNetwork dirty in _dirtyNetworks)
            {
                _activeNetworks.Remove(dirty);

                _remaining.Clear();
                _remaining.UnionWith(dirty.Nodes);
                _seen.Clear();

                foreach (IPlaceable start in _remaining)
                {
                    if (_seen.Contains(start)) continue;

                    PowerNetwork split = new PowerNetwork();
                    _queue.Clear();
                    _queue.Enqueue(start);
                    _seen.Add(start);

                    while (_queue.Count > 0)
                    {
                        IPlaceable current = _queue.Dequeue();
                        split.AddNode(current);
                        _objectToNetwork[current] = split;

                        _grid.CollectNeighbours(current, _neighbours, _localSeen);
                        for (int i = 0; i < _neighbours.Count; i++)
                        {
                            IPlaceable neighbour = _neighbours[i];
                            if (!_remaining.Contains(neighbour)) continue;
                            if (_seen.Contains(neighbour)) continue;
                            _seen.Add(neighbour);
                            _queue.Enqueue(neighbour);
                        }
                    }

                    _activeNetworks.Add(split);
                    split.NotifyComponents();
                }
            }

            _dirtyNetworks.Clear();
        }

        /// <summary>Drops all networks and mappings.</summary>
        public void Clear()
        {
            _objectToNetwork.Clear();
            _activeNetworks.Clear();
            _dirtyNetworks.Clear();
        }
    }
}
