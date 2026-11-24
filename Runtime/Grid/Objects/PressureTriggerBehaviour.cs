using System;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Grid behaviour for pressure plates: fires an event once per press instead of
    /// calling game-specific managers. Supports multiple simultaneous overlapping objects.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PressureTriggerBehaviour : MonoBehaviour, IGridBehaviour
    {
        [SerializeField] private string _triggerTag = "Player";
        [SerializeField] private bool _ignoreTag;
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private Sprite _idleSprite;
        [SerializeField] private Sprite _pressedSprite;

        private int _overlappingCount;
        private bool _firedThisPress;

        /// <summary>Raised once when the plate transitions from empty to pressed.</summary>
        public event Action<PressureTriggerBehaviour> Activated;

        /// <summary>Whether at least one eligible object is overlapping.</summary>
        public bool IsPressed => _overlappingCount > 0;

        private void Awake()
        {
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsEligible(other)) return;

            _overlappingCount++;
            if (_overlappingCount != 1) return;

            ApplyVisual(true);
            if (_firedThisPress) return;
            _firedThisPress = true;
            Activated?.Invoke(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsEligible(other)) return;

            _overlappingCount = Mathf.Max(0, _overlappingCount - 1);
            if (_overlappingCount > 0) return;

            ApplyVisual(false);
            _firedThisPress = false;
        }

        /// <inheritdoc />
        public void OnPlaced(IPlaceable placeable, GridPlacementService service)
        {
            _overlappingCount = 0;
            _firedThisPress = false;
        }

        /// <inheritdoc />
        public void OnRemoved()
        {
            _overlappingCount = 0;
            _firedThisPress = false;
            ApplyVisual(false);
        }

        private bool IsEligible(Collider2D other)
        {
            if (other == null) return false;
            if (_ignoreTag || string.IsNullOrEmpty(_triggerTag)) return true;
            return other.CompareTag(_triggerTag);
        }

        private void ApplyVisual(bool pressed)
        {
            if (_renderer == null || _idleSprite == null || _pressedSprite == null) return;
            _renderer.sprite = pressed ? _pressedSprite : _idleSprite;
        }
    }
}
