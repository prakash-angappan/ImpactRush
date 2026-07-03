using System;
using ImpactRush.Core.Managers;
using ImpactRush.Gameplay.Impacts;
using ImpactRush.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Maps screen input to world targets by raycasting against the aim plane.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TargetSelector : MonoBehaviour
    {
        private const float BelowPlatformScreenMargin = 12f;

        [SerializeField] private Camera _camera;
        [SerializeField] private GameplayRectangle _gameplayRectangle;
        [SerializeField] private AimPlane _aimPlane;
        [SerializeField] private AimManager _aimManager;

        private Vector2 _lastScreenPosition;
        private Vector3 _lastWorldTarget;
        private bool _hasAimDebug;

        public GameplayRectangle GameplayRectangle => _gameplayRectangle;

        public event Action<Vector3> TargetSelected;
        public event Action<GameplayTargetRejectReason> TargetRejected;

        private void Reset()
        {
            _gameplayRectangle = FindFirstObjectByType<GameplayRectangle>();
            _aimPlane = FindFirstObjectByType<AimPlane>();
        }

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_gameplayRectangle == null)
            {
                _gameplayRectangle = FindFirstObjectByType<GameplayRectangle>();
            }

            if (_aimPlane == null)
            {
                _aimPlane = FindFirstObjectByType<AimPlane>();
            }

            EnsureAimManager();
        }

        // AimManager is the single source of truth for screen->world aiming. Resolve or create one so
        // the cannon never computes aiming itself (FEATURE-080).
        private void EnsureAimManager()
        {
            if (_aimManager != null)
            {
                return;
            }

            _aimManager = AimManager.Instance != null
                ? AimManager.Instance
                : FindFirstObjectByType<AimManager>();

            if (_aimManager == null)
            {
                _aimManager = gameObject.AddComponent<AimManager>();
            }
        }

        private void Update()
        {
            if (IsGameplayInputBlocked())
            {
                return;
            }

            if (_camera == null || !TryGetInputScreenPosition(out var screenPosition))
            {
                return;
            }

            if (TryGetScreenTarget(screenPosition, out var rawTarget))
            {
                if (GameplayTargetValidator.TryValidateShotTarget(
                        rawTarget,
                        _gameplayRectangle,
                        out var validatedTarget,
                        out var rejectReason))
                {
                    _lastScreenPosition = screenPosition;
                    _lastWorldTarget = validatedTarget;
                    _hasAimDebug = true;
                    GameplayDebugSettings.RecordAimSelection(screenPosition, validatedTarget);
                    TargetSelected?.Invoke(validatedTarget);
                    return;
                }

                HandleRejectedTarget(rejectReason);
                return;
            }

            if (IsScreenTapBelowPlatform(screenPosition))
            {
                HandleRejectedTarget(GameplayTargetRejectReason.BelowPlatform);
            }
        }

        private void HandleRejectedTarget(GameplayTargetRejectReason rejectReason)
        {
            TargetRejected?.Invoke(rejectReason);

            if (rejectReason == GameplayTargetRejectReason.BelowPlatform)
            {
                GameplayHint.ShowBelowPlatform();
            }
        }

        private static bool IsGameplayInputBlocked()
        {
            if (!ServiceLocator.TryGet<GameSessionManager>(out var session))
            {
                return false;
            }

            return session.IsPaused || session.IsLevelComplete || session.IsLevelFailed;
        }

        private static bool TryGetInputScreenPosition(out Vector2 screenPosition)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    if (IsPointerOverUi(touch.position, touch.fingerId))
                    {
                        screenPosition = default;
                        return false;
                    }

                    screenPosition = touch.position;
                    return true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOverUi(Input.mousePosition))
                {
                    screenPosition = default;
                    return false;
                }

                screenPosition = Input.mousePosition;
                return true;
            }

            screenPosition = default;
            return false;
        }

        private static bool IsPointerOverUi(Vector2 screenPosition, int pointerId = -1)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }

            var pointerData = new PointerEventData(eventSystem)
            {
                position = screenPosition,
                pointerId = pointerId
            };

            var results = new System.Collections.Generic.List<RaycastResult>(8);
            eventSystem.RaycastAll(pointerData, results);
            return results.Count > 0;
        }

        public Vector3 GetViewportTarget(Vector2 viewport)
        {
            if (_camera != null && TryGetScreenTarget(ViewportToScreen(viewport), out var target))
            {
                if (GameplayTargetValidator.TryValidateShotTarget(
                        target,
                        _gameplayRectangle,
                        out var validatedTarget,
                        out _))
                {
                    return validatedTarget;
                }
            }

            if (_gameplayRectangle != null)
            {
                return _gameplayRectangle.ResolveViewportTarget(viewport);
            }

            return transform.position + transform.forward * 10f;
        }

        public bool TryGetScreenTarget(Vector2 screenPosition, out Vector3 target)
        {
            target = default;

            // Delegate all screen->world resolution to the AimManager (single source of truth). This
            // keeps the parallax fix, aim-plane and gameplay-bounds handling in one place so future
            // camera/platform/bounds changes never break aiming from here (FEATURE-080).
            EnsureAimManager();
            if (_aimManager != null)
            {
                return _aimManager.TryResolveWorldTarget(screenPosition, out target);
            }

            // Defensive fallback (AimManager unavailable): resolve inline so aiming never hard-fails.
            if (_camera == null)
            {
                return false;
            }

            var ray = _camera.ScreenPointToRay(screenPosition);
            if (UnityEngine.Physics.Raycast(ray, out var targetHit, 1000f, Layers.BreakableImpactMask, QueryTriggerInteraction.Ignore))
            {
                target = targetHit.point;
                return true;
            }

            if (_aimPlane != null && _aimPlane.Collider != null
                && _aimPlane.Collider.Raycast(ray, out var hit, 1000f))
            {
                target = hit.point;
                return true;
            }

            if (_gameplayRectangle == null)
            {
                return false;
            }

            var plane = new Plane(_gameplayRectangle.transform.forward, _gameplayRectangle.Center);
            if (!plane.Raycast(ray, out var distance))
            {
                return false;
            }

            target = ray.GetPoint(distance);
            return true;
        }

        private bool IsScreenTapBelowPlatform(Vector2 screenPosition)
        {
            var stage = GameplayStage.Instance;
            if (stage == null || _camera == null)
            {
                return false;
            }

            if (!stage.TryGetPlatformScreenBounds(_camera, out var platformBounds))
            {
                return false;
            }

            return screenPosition.y < platformBounds.yMin - BelowPlatformScreenMargin;
        }

#if UNITY_EDITOR
        // Debug-only visualization of the aim pipeline: Screen Ray -> Aim Plane -> World Target.
        private void OnDrawGizmos()
        {
            var settings = GameplayDebugSettings.Instance;
            if (settings == null || !settings.EnableGameplayDebugMode)
            {
                return;
            }

            if (_gameplayRectangle != null)
            {
                var center = _gameplayRectangle.Center + Vector3.forward * 0.12f;
                var size = new Vector3(_gameplayRectangle.Width, _gameplayRectangle.Height, 0.02f);
                Gizmos.color = new Color(0.2f, 0.7f, 1f, 0.9f);
                Gizmos.matrix = Matrix4x4.TRS(center, _gameplayRectangle.transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, size);
                Gizmos.matrix = Matrix4x4.identity;
            }

            if (!_hasAimDebug)
            {
                return;
            }

            if (_camera != null)
            {
                var ray = _camera.ScreenPointToRay(_lastScreenPosition);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(ray.origin, _lastWorldTarget);
            }

            // Target point (where the player clicked / where the ball should hit).
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_lastWorldTarget, 0.12f);

            // Predicted projectile path (straight spawn -> target) and the actual impact point,
            // so any residual vertical drift between predicted and actual is obvious.
            var spawn = GameplayDebugSettings.ProjectileSpawnPosition;
            if (spawn != Vector3.zero)
            {
                Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.9f);
                Gizmos.DrawLine(spawn, GameplayDebugSettings.PredictedImpactPosition);
                Gizmos.DrawWireSphere(spawn, 0.1f);
            }

            var actualImpact = GameplayDebugSettings.ImpactPosition;
            if (actualImpact != Vector3.zero)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(actualImpact, 0.14f);
            }
        }
#endif

        private Vector2 ViewportToScreen(Vector2 viewport)
        {
            if (_camera == null)
            {
                return new Vector2(viewport.x * Screen.width, viewport.y * Screen.height);
            }

            var rect = _camera.rect;
            var cameraViewport = new Vector3(
                rect.x + viewport.x * rect.width,
                rect.y + viewport.y * rect.height,
                0f);
            return _camera.ViewportToScreenPoint(cameraViewport);
        }
    }
}
