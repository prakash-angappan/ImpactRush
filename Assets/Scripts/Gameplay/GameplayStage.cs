using ImpactRush.Gameplay.Data;
using ImpactRush.Gameplay.Impacts;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Positions the aim plane and playfield ground layout from gameplay config.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-300)]
    public sealed class GameplayStage : MonoBehaviour
    {
        private const float AimPlaneDepth = 0.08f;
        private const float AimPlaneZOffset = 0.12f;
        private const float DefaultPlaneSize = 10f;

        public static GameplayStage Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameplayConfig _gameplayConfig;
        [SerializeField] private GameplayRectangle _gameplayRectangle;
        [SerializeField] private AimPlane _aimPlane;
        [SerializeField] private Transform _platform;
        [SerializeField] private Transform _ground;

        [Header("Projectile")]
        [SerializeField] private float _projectileLifetime = 12f;

        [Header("Muzzle Effects")]
        [SerializeField] private float _fireSize = 0.2f;
        [SerializeField] private float _fireLifetime = 0.45f;
        [SerializeField] private float _smokeSize = 0.28f;
        [SerializeField] private float _smokeLifetime = 0.7f;

        public GameplayRectangle GameplayRectangle => _gameplayRectangle;
        public Transform Platform => _platform;
        public float ProjectileLifetime => _projectileLifetime;
        public float FireSize => _fireSize;
        public float FireLifetime => _fireLifetime;
        public float SmokeSize => _smokeSize;
        public float SmokeLifetime => _smokeLifetime;

        private void Awake()
        {
            Instance = this;
            EnsureGameplayConfigRegistered();

            EnsureBootstrapSystems();
            ApplyLayout();
            ApplyGroundLayout();
            RebuildCombatZone();
        }

        private void EnsureGameplayConfigRegistered()
        {
            if (_gameplayConfig == null)
            {
                _gameplayConfig = TryLoadDefaultGameplayConfig();
            }

            if (_gameplayConfig != null)
            {
                GameplayConfigProvider.Register(_gameplayConfig);
                return;
            }

            Debug.LogError(
                "GameplayStage: GameplayConfig is not assigned. Assign it on LevelRoot or run "
                + "Impact Rush → Ensure Gameplay Config Assets.");
        }

        private static GameplayConfig TryLoadDefaultGameplayConfig()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<GameplayConfig>(
                "Assets/ScriptableObjects/Gameplay/GameplayConfig.asset");
#else
            return null;
#endif
        }

        private void RebuildCombatZone()
        {
            var combatZone = EnsureComponent<CombatZone>();
            combatZone.Rebuild();
            EnsureComponent<CombatZoneDebugDrawer>();
        }

        private void EnsureBootstrapSystems()
        {
            EnsureComponent<GameplayCollisionSetup>();

            var camera = Camera.main;
            if (camera != null && camera.GetComponent<GameplayCameraShake>() == null)
            {
                camera.gameObject.AddComponent<GameplayCameraShake>();
            }
        }

        private T EnsureComponent<T>() where T : Component
        {
            var component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (GameplayConfigProvider.Active == _gameplayConfig)
            {
                GameplayConfigProvider.Clear();
            }
        }

        private void ApplyLayout()
        {
            if (_gameplayRectangle == null || _aimPlane == null)
            {
                return;
            }

            ConfigureGameplayArea();

            var center = _gameplayRectangle.Center;
            var aimPosition = center + Vector3.forward * AimPlaneZOffset;
            _aimPlane.transform.SetPositionAndRotation(aimPosition, _gameplayRectangle.transform.rotation);
            _aimPlane.ConfigureSize(
                Vector3.zero,
                new Vector3(_gameplayRectangle.Width, _gameplayRectangle.Height, AimPlaneDepth));
        }

        private void ConfigureGameplayArea()
        {
            if (_gameplayRectangle == null || _platform == null)
            {
                return;
            }

            var platformBounds = ResolvePlatformBounds();
            var platformTop = ResolvePlatformTopWorldY();
            var areaHeight = _gameplayConfig != null
                ? _gameplayConfig.GameplayHeight
                : _gameplayRectangle.Height;
            var configuredWidth = _gameplayConfig != null
                ? _gameplayConfig.GameplayWidth
                : _gameplayRectangle.Width;
            var areaWidth = Mathf.Max(configuredWidth, platformBounds.size.x);

            _gameplayRectangle.ConfigureHorizontalWorldExtent(areaWidth);
            _gameplayRectangle.ConfigureVerticalWorldExtent(platformTop, platformTop + areaHeight);
        }

        public Bounds ResolvePlatformBounds()
        {
            if (_platform == null)
            {
                return default;
            }

            var collider = _platform.GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds;
            }

            var renderer = _platform.GetComponent<Renderer>();
            return renderer != null ? renderer.bounds : new Bounds(_platform.position, Vector3.one);
        }

        public float ResolvePlatformTopWorldY()
        {
            if (_platform == null)
            {
                return 0f;
            }

            var collider = _platform.GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds.max.y;
            }

            var renderer = _platform.GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.max.y;
            }

            return _platform.position.y;
        }

        public bool TryGetPlatformScreenBounds(Camera camera, out Rect screenBounds)
        {
            screenBounds = default;
            if (_platform == null || camera == null)
            {
                return false;
            }

            var bounds = default(Bounds);
            var hasBounds = false;
            var collider = _platform.GetComponent<Collider>();
            if (collider != null)
            {
                bounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                var renderer = _platform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
            }

            if (!hasBounds)
            {
                return false;
            }

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            var center = bounds.center;
            var extents = bounds.extents;

            for (var x = -1; x <= 1; x += 2)
            {
                for (var y = -1; y <= 1; y += 2)
                {
                    for (var z = -1; z <= 1; z += 2)
                    {
                        var worldCorner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                        var screenPoint = camera.WorldToScreenPoint(worldCorner);
                        if (screenPoint.z < 0f)
                        {
                            continue;
                        }

                        min.x = Mathf.Min(min.x, screenPoint.x);
                        min.y = Mathf.Min(min.y, screenPoint.y);
                        max.x = Mathf.Max(max.x, screenPoint.x);
                        max.y = Mathf.Max(max.y, screenPoint.y);
                    }
                }
            }

            if (min.x == float.MaxValue)
            {
                return false;
            }

            screenBounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return true;
        }

        private void ApplyGroundLayout()
        {
            if (_ground == null)
            {
                _ground = transform.Find("Ground");
            }

            if (_ground == null)
            {
                return;
            }

            var width = ResolveGroundWidth();
            var length = ResolveGroundLength();
            var centerZ = ResolveGroundCenterZ();
            var height = ResolveGroundHeight();

            _ground.localPosition = new Vector3(0f, height, centerZ);
            _ground.localScale = new Vector3(width / DefaultPlaneSize, 1f, length / DefaultPlaneSize);

            if (_ground.GetComponent<GroundCollider>() == null)
            {
                _ground.gameObject.AddComponent<GroundCollider>();
            }

            var extended = transform.Find("ExtendedGround");
            if (extended != null)
            {
                extended.gameObject.SetActive(false);
            }
        }

        private float ResolveGroundWidth()
        {
            if (_gameplayConfig != null && _gameplayConfig.GroundWidth > 0.01f)
            {
                return _gameplayConfig.GroundWidth;
            }

            return ResolvePlayfieldGroundWidth();
        }

        private float ResolveGroundLength()
        {
            if (_gameplayConfig != null && _gameplayConfig.GroundLength > 0.01f)
            {
                return _gameplayConfig.GroundLength;
            }

            return ResolveExtendedGroundLength();
        }

        private float ResolveGroundCenterZ()
        {
            if (_gameplayConfig != null)
            {
                return _gameplayConfig.GroundCenterZ;
            }

            return ResolveExtendedGroundStartZ() + (ResolveExtendedGroundLength() * 0.5f);
        }

        private float ResolveGroundHeight()
        {
            return _gameplayConfig != null ? _gameplayConfig.GroundHeight : 0f;
        }

        private float ResolvePlayfieldGroundWidth()
        {
            if (_gameplayConfig != null)
            {
                return _gameplayConfig.PlayfieldGroundWidth;
            }

            return 18f;
        }

        private float ResolveExtendedGroundLength()
        {
            if (_gameplayConfig != null)
            {
                return _gameplayConfig.ExtendedGroundLength;
            }

            return 48f;
        }

        private float ResolveExtendedGroundStartZ()
        {
            if (_gameplayConfig != null)
            {
                return _gameplayConfig.ExtendedGroundStartZ;
            }

            return 10f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                ApplyLayout();
                ApplyGroundLayout();
            }
        }

        private void Reset()
        {
            _gameplayRectangle = GetComponentInChildren<GameplayRectangle>();
            _aimPlane = GetComponentInChildren<AimPlane>();
            _platform = transform.Find("Platform");
            _ground = transform.Find("Ground");
        }
#endif
    }
}
