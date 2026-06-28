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
        [SerializeField] private float _platformTopOffset = 0.55f;
        [SerializeField] private float _checkInterval = 0.25f;

        private float _nextCheckTime;
        private bool _hasReportedComplete;
        private int _initialTargetCount;

        private void Awake()
        {
            _initialTargetCount = CountActiveTargets();
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
            if (AreAllTargetsCleared())
            {
                _hasReportedComplete = true;
                ServiceLocator.Get<GameSessionManager>().NotifyLevelComplete();
            }
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
            var transforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (var i = 0; i < transforms.Length; i++)
            {
                var target = transforms[i];
                if (!target.name.StartsWith("Cube_") || !target.gameObject.activeInHierarchy)
                {
                    continue;
                }

                activeTargets++;
                if (target.position.y < platformTopY)
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
            var transforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (var i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name.StartsWith("Cube_") && transforms[i].gameObject.activeInHierarchy)
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
        }
#endif
    }
}
