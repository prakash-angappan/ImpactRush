using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Physics driver for a single piece of the target stack. Starts kinematic while the
    /// level is being laid out, then <see cref="SetDynamic"/> releases it so the structure
    /// can react to projectile impacts.
    /// <para>
    /// RECONSTRUCTED FILE: referenced by <see cref="TargetStackPhysics"/> (Configure/SetDynamic)
    /// and <see cref="ImpactHandler"/> (presence check) but never committed to git. The API and
    /// behaviour are inferred from those call sites and mirror the surviving <see cref="StackBox"/>.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StackPiece : MonoBehaviour
    {
        [SerializeField] private float _mass = 1f;
        [SerializeField] private float _drag = 0.2f;
        [SerializeField] private float _angularDrag = 0.2f;

        private PhysicsMaterial _contactMaterial;
        private Rigidbody _rigidbody;

        /// <summary>
        /// Applies rigidbody settings. When <paramref name="startKinematic"/> is true the piece
        /// is frozen (kinematic, no gravity) until <see cref="SetDynamic"/> is called.
        /// </summary>
        public void Configure(float mass, float drag, float angularDrag, PhysicsMaterial contactMaterial, bool startKinematic)
        {
            _mass = mass;
            _drag = drag;
            _angularDrag = angularDrag;
            _contactMaterial = contactMaterial;

            EnsureRigidbody();
            _rigidbody.mass = _mass;
            _rigidbody.linearDamping = _drag;
            _rigidbody.angularDamping = _angularDrag;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rigidbody.isKinematic = startKinematic;
            _rigidbody.useGravity = !startKinematic;

            if (_contactMaterial != null)
            {
                var pieceCollider = GetComponent<Collider>();
                if (pieceCollider != null)
                {
                    pieceCollider.material = _contactMaterial;
                }
            }
        }

        /// <summary>
        /// Releases the piece so it responds to gravity and collisions.
        /// </summary>
        public void SetDynamic()
        {
            EnsureRigidbody();
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;
            if (_rigidbody.IsSleeping())
            {
                _rigidbody.WakeUp();
            }
        }

        private void EnsureRigidbody()
        {
            if (_rigidbody != null)
            {
                return;
            }

            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
            }
        }
    }
}
