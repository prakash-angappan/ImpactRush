using ImpactRush.Core.Managers;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Detects when all stack targets have fallen below the platform and notifies the session manager.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelCompleteMonitor : MonoBehaviour
    {
        [SerializeField] private Transform _platform;
        [SerializeField] private Transform _targetRoot;
        [SerializeField] private float _platformTopOffset = 0.55f;
        [SerializeField] private float _checkInterval = 0.25f;

        private float _nextCheckTime;
        private bool _hasReportedComplete;
        private int _initialTargetCount;
        private LevelTarget[] _targets = System.Array.Empty<LevelTarget>();

        private void Start()
        {
            RefreshTargets();
        }

        private void Update()
        {
            if (_hasReportedComplete || _initialTargetCount <= 0)
            {
                return;
            }

            if (Time.time < _nextCheckTime)
            {
                return;
            }

            _nextCheckTime = Time.time + _checkInterval;

            if (!ServiceLocator.TryGet<GameSessionManager>(out var session))
            {
                return;
            }

            if (session.IsLevelComplete || session.IsLevelFailed)
            {
                return;
            }

            if (AreAllTargetsCleared())
            {
                _hasReportedComplete = true;
                session.NotifyLevelComplete();
                return;
            }

            if (session.BallsRemaining <= 0 && session.HasOutOfBallsFailGraceElapsed())
            {
                session.NotifyLevelFailed();
            }
        }

        public void RefreshTargets()
        {
            _targets = _targetRoot != null
                ? _targetRoot.GetComponentsInChildren<LevelTarget>(true)
                : FindObjectsByType<LevelTarget>(FindObjectsSortMode.None);
            _initialTargetCount = CountActiveTargets();
            _hasReportedComplete = false;
        }

        private bool AreAllTargetsCleared()
        {
            if (_platform == null)
            {
                return false;
            }

            var platformTopY = _platform.position.y + _platformTopOffset;
            var activeTargets = 0;
            var clearedTargets = 0;

            for (var i = 0; i < _targets.Length; i++)
            {
                var target = _targets[i];
                if (target == null || !target.gameObject.activeInHierarchy)
                {
                    continue;
                }

                activeTargets++;
                if (target.transform.position.y < platformTopY)
                {
                    clearedTargets++;
                }
            }

            if (activeTargets == 0)
            {
                return false;
            }

            return clearedTargets >= activeTargets;
        }

        private int CountActiveTargets()
        {
            var count = 0;
            for (var i = 0; i < _targets.Length; i++)
            {
                if (_targets[i] != null && _targets[i].gameObject.activeInHierarchy)
                {
                    count++;
                }
            }

            return count;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _platform = transform.Find("Platform");
            _targetRoot = transform.Find("TargetStack");
        }
#endif
    }
}
