using ImpactRush.Core.Managers;
using ImpactRush.Gameplay.Data;
using ImpactRush.Gameplay.Impacts;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Completes the level once no targets remain on the platform, after a short polish delay.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelCompleteMonitor : MonoBehaviour
    {
        [SerializeField] private Transform _platform;
        [SerializeField] private Transform _targetRoot;
        [SerializeField] private PlatformTrackingVolume _platformTrackingVolume;
        [SerializeField] private float _checkInterval = 0.25f;

        private float _nextCheckTime;
        private float _platformClearedTime = -1f;
        private bool _hasReportedComplete;
        private int _initialTargetCount;
        private LevelTarget[] _targets = System.Array.Empty<LevelTarget>();

        public int PlatformObjectCount => ResolvePlatformObjectCount();
        public float LevelCompleteCountdownRemaining => ResolveCountdownRemaining();

        private void Start()
        {
            EnsurePlatformTrackingVolume();
            RefreshTargets();
        }

        private void Update()
        {
            if (_hasReportedComplete || _initialTargetCount <= 0)
            {
                UpdateDebugState();
                return;
            }

            if (Time.time < _nextCheckTime)
            {
                UpdateDebugState();
                return;
            }

            _nextCheckTime = Time.time + _checkInterval;

            if (!ServiceLocator.TryGet<GameSessionManager>(out var session))
            {
                UpdateDebugState();
                return;
            }

            if (session.IsLevelComplete || session.IsLevelFailed)
            {
                UpdateDebugState();
                return;
            }

            TryNotifyLevelFailed(session);

            var onPlatform = ResolvePlatformObjectCount();
            if (onPlatform > 0)
            {
                _platformClearedTime = -1f;
                UpdateDebugState();
                return;
            }

            if (_platformClearedTime < 0f)
            {
                _platformClearedTime = Time.time;
                UpdateDebugState();
                return;
            }

            if (Time.time - _platformClearedTime >= ResolveLevelCompleteDelay())
            {
                _hasReportedComplete = true;
                session.NotifyLevelComplete();
            }

            UpdateDebugState();
        }

        private void TryNotifyLevelFailed(GameSessionManager session)
        {
            if (session.IsLevelComplete || session.IsLevelFailed)
            {
                return;
            }

            if (session.BallsRemaining <= 0 && session.HasOutOfBallsFailGraceElapsed())
            {
                session.NotifyLevelFailed();
            }
        }

        public void RefreshTargets()
        {
            EnsurePlatformTrackingVolume();
            _targets = _targetRoot != null
                ? _targetRoot.GetComponentsInChildren<LevelTarget>(true)
                : FindObjectsByType<LevelTarget>(FindObjectsSortMode.None);
            _initialTargetCount = CountActiveTargets();
            _hasReportedComplete = false;
            _platformClearedTime = -1f;
            _platformTrackingVolume?.ResyncOccupants();
            UpdateDebugState();
        }

        private int ResolvePlatformObjectCount()
        {
            EnsurePlatformTrackingVolume();
            return _platformTrackingVolume != null
                ? _platformTrackingVolume.PlatformObjectCount
                : 0;
        }

        private void EnsurePlatformTrackingVolume()
        {
            if (_platformTrackingVolume != null)
            {
                return;
            }

            _platformTrackingVolume = PlatformTrackingVolume.Active;
            if (_platformTrackingVolume != null)
            {
                return;
            }

            CachePlatformReference();
            if (_platform == null)
            {
                return;
            }

            var existing = _platform.Find("PlatformTrackingVolume");
            if (existing != null)
            {
                _platformTrackingVolume = existing.GetComponent<PlatformTrackingVolume>();
                if (_platformTrackingVolume != null)
                {
                    return;
                }
            }

            var volumeObject = new GameObject("PlatformTrackingVolume");
            volumeObject.transform.SetParent(_platform, false);
            _platformTrackingVolume = volumeObject.AddComponent<PlatformTrackingVolume>();
        }

        private void CachePlatformReference()
        {
            if (_platform != null)
            {
                return;
            }

            _platform = transform.Find("Platform");
        }

        private float ResolveLevelCompleteDelay()
        {
            var config = GameplayConfigProvider.Active;
            return config != null ? config.LevelCompleteDelay : 2f;
        }

        private float ResolveCountdownRemaining()
        {
            if (_platformClearedTime < 0f || _hasReportedComplete)
            {
                return -1f;
            }

            return Mathf.Max(0f, ResolveLevelCompleteDelay() - (Time.time - _platformClearedTime));
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

        private void UpdateDebugState()
        {
            GameplayDebugSettings.UpdateLevelCompleteState(
                PlatformObjectCount,
                LevelCompleteCountdownRemaining,
                GameplayDebugSettings.CountRemainingTargets());
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
