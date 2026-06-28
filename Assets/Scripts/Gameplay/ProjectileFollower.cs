using System;
using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Moves a projectile along ShotData without Rigidbody simulation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProjectileFollower : MonoBehaviour
    {
        private const float CollisionGraceDistance = 0.35f;

        private ShotData _shot;
        private float _radius;
        private float _distanceFromStart;
        private float _maxTravelDistance;
        private ImpactSettings _impactSettings;
        private Collider _collider;
        private Vector3 _lastPosition;
        private Action _onFinished;

        public void Initialize(
            ShotData shot,
            float projectileRadius,
            float maxTravelDistance,
            ImpactSettings impactSettings,
            Action onFinished)
        {
            _shot = shot;
            _radius = projectileRadius;
            _maxTravelDistance = Mathf.Max(0.01f, maxTravelDistance);
            _impactSettings = impactSettings;
            _onFinished = onFinished;
            _collider = GetComponent<Collider>();
            _distanceFromStart = 0f;
            _lastPosition = shot.SpawnPosition;
            transform.position = _lastPosition;
            transform.rotation = Quaternion.LookRotation(shot.InitialDirection);
        }

        private void Update()
        {
            var displacement = _shot.InitialDirection * (_shot.ProjectileSpeed * Time.deltaTime);
            var newPosition = _lastPosition + displacement;

            if (_distanceFromStart >= CollisionGraceDistance && TryImpactAlongSegment(_lastPosition, newPosition))
            {
                return;
            }

            _distanceFromStart += displacement.magnitude;
            transform.position = newPosition;
            transform.rotation = Quaternion.LookRotation(_shot.InitialDirection);
            _lastPosition = newPosition;

            if (_distanceFromStart >= _maxTravelDistance)
            {
                Finish();
            }
        }

        private bool TryImpactAlongSegment(Vector3 from, Vector3 to)
        {
            var displacement = to - from;
            var distance = displacement.magnitude;
            if (distance <= 0.0001f)
            {
                return false;
            }

            if (!UnityEngine.Physics.SphereCast(from, _radius, displacement.normalized, out var hit, distance, UnityEngine.Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            if (hit.collider == _collider || hit.collider.isTrigger || hit.collider.gameObject.layer == Layers.Get(Layers.Aim))
            {
                return false;
            }

            ImpactHandler.ProcessImpact(hit, _shot.EvaluateWorldVelocity(0f), _impactSettings);
            Finish();
            return true;
        }

        private void Finish()
        {
            _onFinished?.Invoke();
            Destroy(gameObject);
        }
    }
}
