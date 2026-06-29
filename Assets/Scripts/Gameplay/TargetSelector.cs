using System;
using System.Collections.Generic;
using ImpactRush.Core.Managers;
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
        [SerializeField] private Camera _camera;
        [SerializeField] private GameplayRectangle _gameplayRectangle;
        [SerializeField] private AimPlane _aimPlane;

        public GameplayRectangle GameplayRectangle => _gameplayRectangle;

        public event Action<Vector3> TargetSelected;

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

            if (TryGetScreenTarget(screenPosition, out var target))
            {
                TargetSelected?.Invoke(target);
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

            var results = new List<RaycastResult>(8);
            eventSystem.RaycastAll(pointerData, results);
            return results.Count > 0;
        }

        public Vector3 GetViewportTarget(Vector2 viewport)
        {
            if (_camera != null && TryGetScreenTarget(ViewportToScreen(viewport), out var target))
            {
                return target;
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
            if (_camera == null)
            {
                return false;
            }

            var ray = _camera.ScreenPointToRay(screenPosition);

            if (_aimPlane != null && _aimPlane.Collider != null
                && _aimPlane.Collider.Raycast(ray, out var hit, 1000f))
            {
                target = hit.point;
                return true;
            }

            if (_gameplayRectangle != null)
            {
                var plane = new Plane(_gameplayRectangle.transform.forward, _gameplayRectangle.Center);
                if (plane.Raycast(ray, out var distance))
                {
                    target = ray.GetPoint(distance);
                    return true;
                }
            }

            return false;
        }

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
