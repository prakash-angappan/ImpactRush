using System.Collections.Generic;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Spawns projectiles and drives them along straight-line ShotData paths.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProjectileLauncher : MonoBehaviour
    {
        private const float UnitSphereRadius = 0.5f;

        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _spawnPoint;
        [Header("Trajectory")]
        [SerializeField] private float _projectileTravelTime = 0.65f;
        [SerializeField] private float _maxGameplayDistance = 20f;
        [SerializeField] private float _minimumArcHeight;
        [SerializeField] private float _maximumArcHeight = 5f;
        [Header("Projectile")]
        [SerializeField] private float _projectileLifetime = 8f;
        [SerializeField] private float _projectileRadius = 0.25f;
        [Header("Impact")]
        [SerializeField] private float _impactForce = 1.5f;
        [SerializeField] private float _explosionRadius;
        [SerializeField] private float _explosionForce;
        [Header("Capacity")]
        [SerializeField] private int _maxActiveProjectiles = 3;

        private readonly List<GameObject> _activeProjectiles = new();

        public Vector3 SpawnPosition => _spawnPoint != null ? _spawnPoint.position : transform.position;

        public ShotData CreateShot(Vector3 target)
        {
            return CreateShotFromDirection((target - SpawnPosition).normalized, target);
        }

        public ShotData CreateShotFromDirection(Vector3 direction, Vector3? targetOverride = null)
        {
            var normalizedDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : transform.forward;
            var maxRange = Mathf.Max(0.01f, _maxGameplayDistance);
            var safeTravelTime = Mathf.Max(0.01f, _projectileTravelTime);
            var projectileSpeed = maxRange / safeTravelTime;
            var travelEnd = SpawnPosition + (normalizedDirection * maxRange);
            var targetPosition = targetOverride ?? travelEnd;
            var distance = Vector3.Distance(SpawnPosition, targetPosition);

            return new ShotData(
                SpawnPosition,
                targetPosition,
                travelEnd,
                normalizedDirection,
                0f,
                safeTravelTime,
                distance,
                Mathf.Clamp01(distance / maxRange),
                projectileSpeed);
        }

        public bool HasCapacity()
        {
            PurgeDestroyedProjectiles();
            return _activeProjectiles.Count < _maxActiveProjectiles;
        }

        public bool TryLaunch(ShotData shot)
        {
            if (!HasCapacity() || _projectilePrefab == null || _spawnPoint == null)
            {
                return false;
            }

            var projectile = Instantiate(
                _projectilePrefab,
                shot.SpawnPosition,
                Quaternion.LookRotation(shot.InitialDirection));

            DisablePhysicsSimulation(projectile);
            ApplyProjectileSize(projectile.transform, projectile.GetComponent<SphereCollider>());

            var follower = projectile.GetComponent<ProjectileFollower>();
            if (follower == null)
            {
                follower = projectile.AddComponent<ProjectileFollower>();
            }

            follower.Initialize(
                shot,
                _projectileRadius,
                _maxGameplayDistance,
                new ImpactSettings
                {
                    ImpactForce = _impactForce,
                    ExplosionRadius = _explosionRadius,
                    ExplosionForce = _explosionForce,
                },
                () => _activeProjectiles.Remove(projectile));

            _activeProjectiles.Add(projectile);
            Destroy(projectile, _projectileLifetime);
            return true;
        }

        private static void DisablePhysicsSimulation(GameObject projectile)
        {
            var rigidbodies = projectile.GetComponentsInChildren<Rigidbody>(true);
            for (var i = 0; i < rigidbodies.Length; i++)
            {
                Destroy(rigidbodies[i]);
            }

            var collider = projectile.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        private void ApplyProjectileSize(Transform projectileTransform, SphereCollider collider)
        {
            var scale = _projectileRadius / UnitSphereRadius;
            projectileTransform.localScale = Vector3.one * scale;

            if (collider != null)
            {
                collider.radius = UnitSphereRadius;
            }
        }

        private void Reset()
        {
            _spawnPoint = FindSpawnPoint();
        }

        private void PurgeDestroyedProjectiles()
        {
            for (var i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                if (_activeProjectiles[i] == null)
                {
                    _activeProjectiles.RemoveAt(i);
                }
            }
        }

        private Transform FindSpawnPoint()
        {
            var transforms = GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var name = transforms[i].name;
                if (name == "SpawnPoint" || name == "ProjectileSpawnPoint")
                {
                    return transforms[i];
                }
            }

            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_projectileTravelTime < 0.01f)
            {
                _projectileTravelTime = 0.01f;
            }

            if (_maxGameplayDistance < 0.01f)
            {
                _maxGameplayDistance = 0.01f;
            }

            if (_minimumArcHeight < 0f)
            {
                _minimumArcHeight = 0f;
            }

            if (_maximumArcHeight < _minimumArcHeight)
            {
                _maximumArcHeight = _minimumArcHeight;
            }
        }
#endif
    }
}
