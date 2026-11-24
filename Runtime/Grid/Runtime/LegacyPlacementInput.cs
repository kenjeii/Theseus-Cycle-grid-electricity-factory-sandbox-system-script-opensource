using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>
    /// Adapter over Unity's legacy Input manager. Replace with an Input System adapter
    /// by implementing <see cref="IPlacementInput"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LegacyPlacementInput : MonoBehaviour, IPlacementInput
    {
        [SerializeField] private KeyCode _cycleNext = KeyCode.F;
        [SerializeField] private KeyCode _place = KeyCode.Mouse0;
        [SerializeField] private KeyCode _cancel = KeyCode.Escape;

        /// <inheritdoc />
        public bool CycleNextPressed => Input.GetKeyDown(_cycleNext);

        /// <inheritdoc />
        public bool PlacePressed => Input.GetKeyDown(_place);

        /// <inheritdoc />
        public bool CancelPressed => Input.GetKeyDown(_cancel);
    }
}
