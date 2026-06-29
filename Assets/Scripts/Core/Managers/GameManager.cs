using System;
using ImpactRush.Core.Bootstrap;
using ImpactRush.Core.Data;
using ImpactRush.Core.Events;
using ImpactRush.Core.Interfaces;
using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Core.Managers
{
    /// <summary>
    /// Application-level lifecycle owner. Persists across scenes and wires core services.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private bool _enableVSync = true;
        [SerializeField] private GameObject _uiRootPrefab;

        private GameObject _uiRootInstance;

        private void Awake()
        {
            Guard.AgainstNull(_gameSettings, nameof(_gameSettings));

            DontDestroyOnLoad(gameObject);
            EnsureUIRoot();
            ApplyApplicationSettings();
            RegisterServices();
            InitializeServices();
            RequestInitialSceneTransition();
        }

        private void EnsureUIRoot()
        {
            _uiRootInstance = GameObject.Find("UIRoot");
            if (_uiRootInstance != null)
            {
                return;
            }

            if (_uiRootPrefab == null)
            {
                _uiRootPrefab = Resources.Load<GameObject>("UI/UIRoot");
            }

            if (_uiRootPrefab != null)
            {
                _uiRootInstance = Instantiate(_uiRootPrefab);
                _uiRootInstance.name = "UIRoot";
                return;
            }

            _uiRootInstance = TryCreateRuntimeUIRoot();
            if (_uiRootInstance == null)
            {
                Debug.LogError(
                    "UIRoot prefab is not assigned on GameManager and could not be loaded from Resources/UI/UIRoot. " +
                    "Run Impact Rush > Build UI Framework in the editor.");
            }
        }

        private static GameObject TryCreateRuntimeUIRoot()
        {
            const string builderTypeName = "ImpactRush.UI.UIRootRuntimeBuilder, ImpactRush.UI";
            var builderType = Type.GetType(builderTypeName);
            if (builderType == null)
            {
                return null;
            }

            var createMethod = builderType.GetMethod("Create", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            return createMethod?.Invoke(null, null) as GameObject;
        }

        private void ApplyApplicationSettings()
        {
            Application.targetFrameRate = _gameSettings.TargetFps;
            QualitySettings.vSyncCount = _enableVSync ? 1 : 0;
            Time.fixedDeltaTime = _gameSettings.FixedTimestep;
            AudioListener.volume = _gameSettings.MasterVolume;
        }

        private void RegisterServices()
        {
            ServiceLocator.Register(_gameSettings);
            RegisterServicesOn(gameObject);

            if (_uiRootInstance != null)
            {
                RegisterServicesOn(_uiRootInstance);
            }
        }

        private static void RegisterServicesOn(GameObject root)
        {
            var services = root.GetComponentsInChildren<IGameService>(true);
            foreach (var service in services)
            {
                ServiceLocator.Register(service.GetType(), service);
            }
        }

        private void InitializeServices()
        {
            InitializeOn(gameObject);

            if (_uiRootInstance != null)
            {
                InitializeOn(_uiRootInstance);
            }
        }

        private static void InitializeOn(GameObject root)
        {
            var initializables = root.GetComponentsInChildren<IInitializable>(true);
            foreach (var initializable in initializables)
            {
                initializable.Initialize();
            }
        }

        private void RequestInitialSceneTransition()
        {
            if (_uiRootInstance == null)
            {
                return;
            }

            var bootstrap = GetComponent<GameBootstrap>();
            if (bootstrap == null)
            {
                return;
            }

            EventBus.Publish(new SceneTransitionRequestedEvent(bootstrap.InitialScene));
        }
    }
}
