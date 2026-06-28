using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Defines the 2D gameplay area in world space. Screen viewport maps 1:1 to this rectangle.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameplayRectangle : MonoBehaviour
    {
        [SerializeField] private float _width = 5.5f;
        [SerializeField] private float _height = 4.5f;
        [SerializeField] private Vector3 _center = Vector3.zero;

        public float Width => _width;
        public float Height => _height;
        public Vector3 Center => transform.TransformPoint(_center);

        public Vector3 BottomLeft => MapViewportToWorld(Vector2.zero);
        public Vector3 BottomRight => MapViewportToWorld(new Vector2(1f, 0f));
        public Vector3 TopLeft => MapViewportToWorld(new Vector2(0f, 1f));
        public Vector3 TopRight => MapViewportToWorld(Vector2.one);

        public Vector3 MapViewportToWorld(Vector2 viewport)
        {
            var halfWidth = _width * 0.5f;
            var halfHeight = _height * 0.5f;
            var localPoint = _center + new Vector3(
                Mathf.Lerp(-halfWidth, halfWidth, viewport.x),
                Mathf.Lerp(-halfHeight, halfHeight, viewport.y),
                0f);
            return transform.TransformPoint(localPoint);
        }

        public bool IsViewportInside(Vector2 viewport)
        {
            return viewport.x >= 0f && viewport.x <= 1f
                && viewport.y >= 0f && viewport.y <= 1f;
        }

        public Vector3 ResolveViewportTarget(Vector2 viewport)
        {
            if (IsViewportInside(viewport))
            {
                return MapViewportToWorld(viewport);
            }

            return MapViewportToWorld(new Vector2(Mathf.Clamp01(viewport.x), Mathf.Clamp01(viewport.y)));
        }

        public bool ContainsWorldPoint(Vector3 worldPoint)
        {
            var localPoint = transform.InverseTransformPoint(worldPoint) - _center;
            var halfWidth = _width * 0.5f;
            var halfHeight = _height * 0.5f;
            return localPoint.x >= -halfWidth && localPoint.x <= halfWidth
                && localPoint.y >= -halfHeight && localPoint.y <= halfHeight;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_width < 0.1f)
            {
                _width = 0.1f;
            }

            if (_height < 0.1f)
            {
                _height = 0.1f;
            }
        }
#endif
    }
}
