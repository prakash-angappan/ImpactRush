using ImpactRush.Core.Data;
using ImpactRush.Core.Managers;
using ImpactRush.Gameplay.Objectives;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Legacy façade that forwards level completion to <see cref="ObjectiveManager"/> so existing scene
    /// wiring stays intact while completion rules are data-driven.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelCompleteMonitor : MonoBehaviour
    {
        [SerializeField] private Transform _platform;
        [SerializeField] private Transform _targetRoot;
        [SerializeField] private PlatformTrackingVolume _platformTrackingVolume;
        [SerializeField] private ObjectiveManager _objectiveManager;
        [SerializeField] private ObjectiveData _defaultPrimaryObjective;

        public int PlatformObjectCount => _objectiveManager != null ? _objectiveManager.PlatformObjectCount : 0;
        public float LevelCompleteCountdownRemaining =>
            _objectiveManager != null ? _objectiveManager.LevelCompleteCountdownRemaining : -1f;

        private void Awake()
        {
            EnsureObjectiveManager();
        }

        private void Start()
        {
            EnsurePlatformTrackingVolume();
            LoadObjectivesForCurrentLevel();
            RefreshTargets();
        }

        private void Update()
        {
            _objectiveManager?.Tick();
        }

        public void RefreshTargets()
        {
            EnsureObjectiveManager();
            EnsurePlatformTrackingVolume();
            LoadObjectivesForCurrentLevel();
            _objectiveManager.Configure(_platform, _targetRoot, _platformTrackingVolume);
            _objectiveManager.RefreshTargets();
        }

        private void LoadObjectivesForCurrentLevel()
        {
            EnsureObjectiveManager();
            LevelData level = null;
            LevelManager levelManager = null;
            if (ServiceLocator.TryGet(out levelManager))
            {
                level = levelManager.CurrentLevel;
            }

            if (level == null && ServiceLocator.TryGet<GameSessionManager>(out var session) && levelManager != null)
            {
                level = levelManager.ResolveCurrentLevel(session.CurrentLevel);
            }

            _objectiveManager.LoadObjectives(level);
        }

        private void EnsureObjectiveManager()
        {
            if (_objectiveManager == null)
            {
                _objectiveManager = GetComponent<ObjectiveManager>();
            }

            if (_objectiveManager == null)
            {
                _objectiveManager = gameObject.AddComponent<ObjectiveManager>();
            }

            if (_defaultPrimaryObjective != null)
            {
                _objectiveManager.SetDefaultPrimaryObjective(_defaultPrimaryObjective);
            }
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
            if (_targetRoot == null)
            {
                _targetRoot = transform.Find("TargetStack");
            }
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
