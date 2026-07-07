using ImpactRush.Core.Balls;
using ImpactRush.Core.Events;
using ImpactRush.Core.Managers;
using ImpactRush.Core.Pooling;
using ImpactRush.Gameplay.Data;
using ImpactRush.Gameplay.Impacts;
using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Spawns pooled projectiles using <see cref="BallConfig"/> tuning only.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProjectileLauncher : MonoBehaviour
    {
        private const float UnitSphereRadius = 0.5f;

        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private bool _preferPool = true;
        [SerializeField] private BallConfig _ballOverride;

        private int _activeProjectileCount;

        public int ActiveProjectileCount => _activeProjectileCount;
        public Vector3 SpawnPosition => _spawnPoint != null ? _spawnPoint.position : transform.position;

        public Transform SpawnTransform => _spawnPoint;

        public bool HasCapacity()
        {
            return _activeProjectileCount < ResolveMaxActiveProjectiles();
        }

        public bool TryLaunch(
            LaunchRequest request,
            BallConfig ball,
            float launchSpeedOverride = -1f,
            BallTypeData ballType = null)
        {
            ball ??= ResolveBallConfig();
            if (ball == null || _projectilePrefab == null || _spawnPoint == null)
            {
                LogLaunchFailure(ball);
                return false;
            }

            // NOTE: firing is intentionally NOT gated by the active-projectile count. Cadence is
            // controlled by the cannon fire-rate cooldown; the pool auto-expands to cover concurrency.
            var liveRequest = CannonAiming.BuildLaunchRequest(request.TargetPosition, _spawnPoint);
            var direction = liveRequest.Direction;
            var rotation = direction.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(direction, Vector3.up)
                : _spawnPoint.rotation;
            var launchSpeed = launchSpeedOverride > 0f
                ? launchSpeedOverride
                : ball.InitialSpeed;

            var projectile = RentProjectile(liveRequest.SpawnPosition, rotation, ballType);
            if (projectile == null)
            {
                return false;
            }

            var controller = projectile.GetComponent<ProjectileBase>();
            if (controller == null)
            {
                controller = projectile.AddComponent<StandardBall>();
            }

            var impactSettings = ImpactSettingsBuilder.Build(ball, ballType);
            controller.Launch(ball, liveRequest, impactSettings, OnProjectileFinished, launchSpeed, ballType);
            GameplayDebugSettings.RecordProjectileSpawn(liveRequest.SpawnPosition, liveRequest.TargetPosition);
            _activeProjectileCount++;
            EventBus.Publish(new ProjectileSpawnedEvent(_activeProjectileCount));
            return true;
        }

        private BallConfig ResolveBallConfig()
        {
            if (_ballOverride != null)
            {
                return _ballOverride;
            }

            return GameplayConfigProvider.DefaultBall;
        }

        private int ResolveMaxActiveProjectiles()
        {
            var physics = GameplayConfigProvider.ProjectilePhysics;
            if (physics != null)
            {
                return physics.MaxActiveProjectiles;
            }

            var projectile = GameplayConfigProvider.Projectile;
            return projectile != null ? projectile.MaxActive : 3;
        }

        private GameObject RentProjectile(Vector3 position, Quaternion rotation, BallTypeData ballType)
        {
            if (_preferPool && PoolManager.Instance != null)
            {
                var poolId = ProjectileManager.Instance != null
                    ? ProjectileManager.Instance.ResolvePoolId(ballType)
                    : PoolIds.Projectile;

                var pooled = PoolManager.Instance.Rent(poolId, position, rotation);
                if (pooled != null)
                {
                    return pooled;
                }
            }

            var prefab = ballType != null && ballType.ProjectilePrefab != null
                ? ballType.ProjectilePrefab
                : _projectilePrefab;
            return Instantiate(prefab, position, rotation);
        }

        private void OnProjectileFinished()
        {
            _activeProjectileCount = Mathf.Max(0, _activeProjectileCount - 1);
            EventBus.Publish(new ProjectileReturnedToPoolEvent(_activeProjectileCount));
        }

        private void Reset()
        {
            _spawnPoint = FindSpawnPoint();
        }

        private static void LogLaunchFailure(BallConfig ball)
        {
            if (ball != null)
            {
                return;
            }

            Debug.LogWarning(
                "ProjectileLauncher: Cannot launch because BallConfig is missing. "
                + "Assign GameplayConfig on LevelRoot or set a Ball Override on ProjectileLauncher.");
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
    }
}
