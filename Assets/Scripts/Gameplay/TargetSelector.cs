using System;
using UnityEngine;

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
            if (_camera == null || !TryGetInputScreenPosition(out var screenPosition))
            {
                return;
            }

            if (TryGetScreenTarget(screenPosition, out var target))
            {
                TargetSelected?.Invoke(target);
            }
        }

        private static bool TryGetInputScreenPosition(out Vector2 screenPosition)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    screenPosition = touch.position;
                    return true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                screenPosition = Input.mousePosition;
                return true;
            }

            screenPosition = default;
            return false;
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
            return new Vector2(viewport.x * Screen.width, viewport.y * Screen.height);
        }
    }
}
