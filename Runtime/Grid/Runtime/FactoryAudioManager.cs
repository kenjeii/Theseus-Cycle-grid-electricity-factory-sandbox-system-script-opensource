using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Throttled, pooled factory audio. Limits simultaneous emitters per sound id and applies
    /// pitch accumulation for looping generators. No per-play Instantiate in the steady state.
    /// </summary>
    public sealed class FactoryAudioManager : MonoBehaviour
    {
        [System.Serializable]
        public sealed class SoundEntry
        {
            /// <summary>Identifier used by callers.</summary>
            public string id;

            /// <summary>Clip to play.</summary>
            public AudioClip clip;

            /// <summary>Maximum simultaneous emitters.</summary>
            public int maxEmitters = 2;

            /// <summary>Minimum seconds between two plays of this id.</summary>
            public float cooldown = 0.03f;

            /// <summary>Base playback pitch.</summary>
            public float basePitch = 1f;

            /// <summary>Pitch added per concurrent emitter.</summary>
            public float pitchPerEmitter = 0.04f;
        }

        [SerializeField] private List<SoundEntry> _entries = new List<SoundEntry>();
        [SerializeField] private int _sourcesPerEntry = 2;

        private readonly Dictionary<string, SoundEntry> _lookup = new Dictionary<string, SoundEntry>();
        private readonly Dictionary<string, List<AudioSource>> _pools = new Dictionary<string, List<AudioSource>>();
        private readonly Dictionary<string, float> _lastPlay = new Dictionary<string, float>();
        private readonly Dictionary<string, int> _active = new Dictionary<string, int>();

        private void Awake()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                SoundEntry entry = _entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.id)) continue;

                _lookup[entry.id] = entry;
                _lastPlay[entry.id] = -999f;
                _active[entry.id] = 0;

                List<AudioSource> pool = new List<AudioSource>();
                int count = Mathf.Max(1, _sourcesPerEntry);
                for (int s = 0; s < count; s++)
                {
                    AudioSource source = gameObject.AddComponent<AudioSource>();
                    source.playOnAwake = false;
                    source.loop = false;
                    pool.Add(source);
                }
                _pools[entry.id] = pool;
            }
        }

        /// <summary>Plays a one-shot if the cooldown and emitter budget allow it.</summary>
        public bool Play(string id)
        {
            SoundEntry entry;
            if (!_lookup.TryGetValue(id, out entry) || entry.clip == null) return false;

            float now = Time.unscaledTime;
            float last;
            if (_lastPlay.TryGetValue(id, out last) && now - last < entry.cooldown) return false;

            List<AudioSource> pool;
            if (!_pools.TryGetValue(id, out pool)) return false;

            int active;
            _active.TryGetValue(id, out active);
            if (active >= Mathf.Max(1, entry.maxEmitters)) return false;

            AudioSource source = FindFree(pool);
            if (source == null) return false;

            source.pitch = Mathf.Max(0.01f, entry.basePitch + active * Mathf.Max(0f, entry.pitchPerEmitter));
            source.PlayOneShot(entry.clip);
            _lastPlay[id] = now;
            _active[id] = active + 1;
            return true;
        }

        private static AudioSource FindFree(List<AudioSource> pool)
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (!pool[i].isPlaying) return pool[i];
            }
            return null;
        }
    }
}
