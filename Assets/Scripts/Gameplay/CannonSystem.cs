using ImpactRush.Core.Managers;
using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Cannon input bridge: validated target selection, aim rotation, and fire state machine.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CannonSystem : MonoBehaviour
    {
        [SerializeField] private TargetSelector _targetSelector;
        [SerializeField] private CannonAimController _aimController;
        [SerializeField] private CannonStateController _stateController;

        private void Awake()
        {
            if (_targetSelector == null)
            {
                _targetSelector = GetComponent<TargetSelector>();
            }

            if (_aimController == null)
            {
                _aimController = GetComponent<CannonAimController>();
            }

            if (_stateController == null)
            {
                _stateController = GetComponent<CannonStateController>();
            }

            Guard.AgainstNull(_targetSelector, nameof(_targetSelector));
            Guard.AgainstNull(_aimController, nameof(_aimController));
            Guard.AgainstNull(_stateController, nameof(_stateController));
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
            if (GameplayTargetValidator.TryValidateShotTarget(
                    centerTarget,
                    _targetSelector.GameplayRectangle,
                    out var mappedTarget,
                    out _))
            {
                _aimController.SetTargetImmediate(mappedTarget);
            }
        }

        private void HandleTargetSelected(Vector3 target)
        {
            if (IsGameplayInputBlocked() || !_stateController.CanAcceptInput())
            {
                return;
            }

            if (!GameplayTargetValidator.TryValidateShotTarget(
                    target,
                    _targetSelector.GameplayRectangle,
                    out var mappedTarget,
                    out _))
            {
                return;
            }

            _aimController.SetTarget(mappedTarget);
            _stateController.BeginShot(mappedTarget);
        }

        private static bool IsGameplayInputBlocked()
        {
            if (!ServiceLocator.TryGet<GameSessionManager>(out var session))
            {
                return false;
            }

            return session.IsPaused || session.IsLevelComplete || session.IsLevelFailed;
        }
    }
}
