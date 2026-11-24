using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Data-only sprite set for the twelve possible four-direction connection masks.
    /// No textures or assets are shipped with this package.
    /// </summary>
    [CreateAssetMenu(menuName = "Theseus Core/Grid/Wire sprite set", fileName = "WireSpriteSet")]
    public sealed class WireSpriteSet : ScriptableObject
    {
        [SerializeField] private Sprite _empty;
        [SerializeField] private Sprite _vertical;
        [SerializeField] private Sprite _horizontal;
        [SerializeField] private Sprite _cornerLD;
        [SerializeField] private Sprite _cornerRD;
        [SerializeField] private Sprite _cornerLU;
        [SerializeField] private Sprite _cornerRU;
        [SerializeField] private Sprite _teeLeft;
        [SerializeField] private Sprite _teeRight;
        [SerializeField] private Sprite _teeUp;
        [SerializeField] private Sprite _teeDown;
        [SerializeField] private Sprite _cross;

        /// <summary>Maps a 4-bit mask (up=1, right=2, down=4, left=8) to a sprite.</summary>
        public Sprite GetSprite(int mask)
        {
            switch (mask)
            {
                case 5: return _vertical;
                case 10: return _horizontal;
                case 12: return _cornerLD;
                case 6: return _cornerRD;
                case 9: return _cornerLU;
                case 3: return _cornerRU;
                case 13: return _teeLeft;
                case 7: return _teeRight;
                case 11: return _teeUp;
                case 14: return _teeDown;
                case 15: return _cross;
                default: return _empty;
            }
        }
    }
}
