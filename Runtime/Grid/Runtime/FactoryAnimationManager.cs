using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Centralized frame-by-frame sprite animation registry. Replaces one Animator per machine
    /// with a single tick loop; clients register a SpriteRenderer and an animation id.
    /// No LINQ and no per-frame allocations.
    /// </summary>
    public sealed class FactoryAnimationManager : MonoBehaviour
    {
        [System.Serializable]
        private sealed class AnimationGroup
        {
            public Sprite[] Frames;
            public float Fps = 12f;
            public bool Loop = true;
        }

        [SerializeField] private List<AnimationGroup> _groups = new List<AnimationGroup>();

        private readonly List<FactoryAnimClient> _clients = new List<FactoryAnimClient>();
        private readonly List<int> _removeIndices = new List<int>();

        private void Update()
        {
            float delta = Time.deltaTime;

            for (int i = _clients.Count - 1; i >= 0; i--)
            {
                FactoryAnimClient client = _clients[i];
                if (client == null || client.Renderer == null)
                {
                    _clients.RemoveAt(i);
                    continue;
                }

                AnimationGroup group = client.GroupId >= 0 && client.GroupId < _groups.Count
                    ? _groups[client.GroupId]
                    : null;
                if (group == null || group.Frames == null || group.Frames.Length == 0) continue;
                if (group.Fps <= 0f) continue;

                client.Time += delta * group.Fps;
                int frame = (int)client.Time;
                if (frame >= group.Frames.Length)
                {
                    if (!group.Loop)
                    {
                        frame = group.Frames.Length - 1;
                        client.Time = frame;
                        client.Unregister();
                    }
                    else
                    {
                        frame %= group.Frames.Length;
                        client.Time = frame + (client.Time - (int)client.Time);
                    }
                }

                client.Renderer.sprite = group.Frames[frame];
            }
        }

        /// <summary>Registers a renderer for an animation group. Safe to call repeatedly.</summary>
        public void Register(FactoryAnimClient client)
        {
            if (client == null || _clients.Contains(client)) return;
            _clients.Add(client);
        }

        /// <summary>Unregisters a client.</summary>
        public void Unregister(FactoryAnimClient client)
        {
            if (client == null) return;
            _clients.Remove(client);
        }

        /// <summary>Number of frames in a group.</summary>
        public int FrameCount(int groupId)
        {
            if (groupId < 0 || groupId >= _groups.Count) return 0;
            AnimationGroup group = _groups[groupId];
            return group.Frames == null ? 0 : group.Frames.Length;
        }
    }

    /// <summary>Registers/unregisters a SpriteRenderer with a centralized animation manager.</summary>
    public sealed class FactoryAnimClient : MonoBehaviour
    {
        [SerializeField] private int _groupId;
        [SerializeField] private SpriteRenderer _renderer;

        private FactoryAnimationManager _manager;

        /// <summary>Current animation time in frames.</summary>
        public float Time { get; set; }

        /// <summary>Group id assigned by the manager.</summary>
        public int GroupId => _groupId;

        /// <summary>Renderer driven by the manager.</summary>
        public SpriteRenderer Renderer => _renderer != null ? _renderer : (_renderer = GetComponent<SpriteRenderer>());

        private void OnEnable()
        {
            _manager = FindAnyObjectByType<FactoryAnimationManager>();
            if (_manager != null) _manager.Register(this);
        }

        private void OnDisable()
        {
            if (_manager != null) _manager.Unregister(this);
            _manager = null;
        }

        /// <summary>Sets a different animation group and resets playback.</summary>
        public void SetGroup(int groupId)
        {
            _groupId = groupId;
            Time = 0f;
        }

        /// <summary>Detaches from the manager.</summary>
        public void Unregister()
        {
            if (_manager != null) _manager.Unregister(this);
        }
    }
}
