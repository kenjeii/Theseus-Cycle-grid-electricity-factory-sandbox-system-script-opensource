using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Projects the mouse cursor onto the z=0 plane through a camera.
    /// Replace with a touch or XR resolver by implementing <see cref="ICursorWorldResolver"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MouseCursorWorldResolver : MonoBehaviour, ICursorWorldResolver
    {
        [SerializeField] private UnityEngine.Camera _targetCamera;

        private void Awake()
        {
            if (_targetCamera == null) _targetCamera = UnityEngine.Camera.main;
        }

        /// <summary>Camera used for the projection. Defaults to Camera.main.</summary>
        public UnityEngine.Camera TargetCamera
        {
            get => _targetCamera;
            set => _targetCamera = value;
        }

        /// <inheritdoc />
        public bool TryGetWorldPosition(out Vector3 worldPosition)
        {
            if (_targetCamera == null)
            {
                worldPosition = Vector3.zero;
                return false;
            }

            worldPosition = _targetCamera.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = 0f;
            return true;
        }
    }
}
