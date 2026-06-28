using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Invisible surface aligned with GameplayRectangle. Never renders or affects gameplay physics.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class AimPlane : MonoBehaviour
    {
        [SerializeField] private BoxCollider _collider;

        public BoxCollider Collider => _collider;

        private void Reset()
        {
            _collider = GetComponent<BoxCollider>();
            ConfigureCollider();
        }

        private void Awake()
        {
            if (_collider == null)
            {
                _collider = GetComponent<BoxCollider>();
            }

            Guard.AgainstNull(_collider, nameof(_collider));
            ConfigureCollider();
        }

        public void ConfigureSize(Vector3 center, Vector3 size)
        {
            if (_collider == null)
            {
                _collider = GetComponent<BoxCollider>();
            }

            if (_collider == null)
            {
                return;
            }

            _collider.center = center;
            _collider.size = size;
            ConfigureCollider();
        }

        private void ConfigureCollider()
        {
            _collider.isTrigger = true;
            gameObject.layer = Layers.Get(Layers.Aim);
        }
    }
}
