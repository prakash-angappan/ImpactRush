using ImpactRush.Audio;
using ImpactRush.Core.Events;
using ImpactRush.Core.Managers;
using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Coordinates tap targeting, ShotData generation, cannon aim, and projectile launch.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CannonSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TargetSelector _targetSelector;
        [SerializeField] private CannonAnimator _cannonAnimator;
        [SerializeField] private ProjectileLauncher _projectileLauncher;
        [Header("Timing")]
        [SerializeField] private float _shotCooldown = 0.08f;
        [Header("Debug")]
        [SerializeField] private bool _drawInitialDirectionDebug;
        [SerializeField] private float _debugLineLength = 3f;

        private float _nextShotTime;
        private ShotData _lastShotData;
        private bool _hasLastShotData;

        private void Awake()
        {
            if (_targetSelector == null)
            {
                _targetSelector = GetComponent<TargetSelector>();
            }

            if (_cannonAnimator == null)
            {
                _cannonAnimator = GetComponent<CannonAnimator>();
            }

            if (_projectileLauncher == null)
            {
                _projectileLauncher = GetComponent<ProjectileLauncher>();
            }

            Guard.AgainstNull(_targetSelector, nameof(_targetSelector));
            Guard.AgainstNull(_cannonAnimator, nameof(_cannonAnimator));
            Guard.AgainstNull(_projectileLauncher, nameof(_projectileLauncher));
        }

        private void OnEnable()
        {
            _targetSelector.TargetSelected += HandleTargetSelected;
        }

        private void OnDisable()
        {
            _targetSelector.TargetSelected -= HandleTargetSelected;
        }

        private void Start()
        {
            var centerTarget = _targetSelector.GetViewportTarget(new Vector2(0.5f, 0.5f));
            AimCannonAtTarget(centerTarget);
        }

        private void HandleTargetSelected(Vector3 target)
        {
            if (IsGameplayInputBlocked() || Time.time < _nextShotTime || !_projectileLauncher.HasCapacity())
            {
                return;
            }

            if (!TryConsumeBall())
            {
                return;
            }

            _cannonAnimator.AimAtTarget(target);

            var direction = (target - _projectileLauncher.SpawnPosition).normalized;
            var shot = _projectileLauncher.CreateShotFromDirection(direction, target);
            RememberShot(shot);

            if (_projectileLauncher.TryLaunch(shot))
            {
                EventBus.Publish(new PlaySfxEvent(AudioIds.ProjectileFire));
            }

            _nextShotTime = Time.time + _shotCooldown;
        }

        private void AimCannonAtTarget(Vector3 target)
        {
            _cannonAnimator.AimAtTarget(target);

            var direction = (target - _projectileLauncher.SpawnPosition).normalized;
            var shot = _projectileLauncher.CreateShotFromDirection(direction, target);
            RememberShot(shot);
        }

        private void RememberShot(ShotData shot)
        {
            _lastShotData = shot;
            _hasLastShotData = true;
        }

        private static bool IsGameplayInputBlocked()
        {
            if (!ServiceLocator.TryGet<GameSessionManager>(out var session))
            {
                return false;
            }

            return session.IsPaused || session.IsLevelComplete || session.IsLevelFailed;
        }

        private static bool TryConsumeBall()
        {
            if (!ServiceLocator.TryGet<GameSessionManager>(out var session))
            {
                return true;
            }

            return session.TryConsumeBall();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawInitialDirectionDebug || !_hasLastShotData)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                _lastShotData.SpawnPosition,
                _lastShotData.SpawnPosition + (_lastShotData.InitialDirection * _debugLineLength));
        }
#endif
    }
}
