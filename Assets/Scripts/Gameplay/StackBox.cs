using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Configures a stack box for stable, Angry Birds-style physics collisions.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class StackBox : MonoBehaviour
    {
        [SerializeField] private float _mass = 1f;
        [SerializeField] private float _drag = 0.15f;
        [SerializeField] private float _angularDrag = 0.05f;

        private PhysicsMaterial _contactMaterial;

        public void Configure(float mass, float drag, float angularDrag, PhysicsMaterial contactMaterial)
        {
            _mass = mass;
            _drag = drag;
            _angularDrag = angularDrag;
            _contactMaterial = contactMaterial;
            Apply();
        }

        private void Awake() => Apply();

        private void Apply()
        {
            var collider = GetComponent<BoxCollider>();
            var rigidbody = GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = gameObject.AddComponent<Rigidbody>();
            }

            rigidbody.mass = _mass;
            rigidbody.linearDamping = _drag;
            rigidbody.angularDamping = _angularDrag;
            rigidbody.useGravity = true;
            rigidbody.isKinematic = false;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (_contactMaterial != null)
            {
                collider.material = _contactMaterial;
            }
        }
    }
}
