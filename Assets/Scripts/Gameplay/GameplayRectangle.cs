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

        [Header("Debug")]
        [SerializeField] private bool _drawAreaDebug = true;
        [SerializeField] private float _invalidAreaDepth = 0.75f;

        public float Width => _width;
        public float Height => _height;
        public Vector3 Center => transform.TransformPoint(_center);
        public float WorldBottomY => GetWorldCornerY(-1f);
        public float WorldTopY => GetWorldCornerY(1f);

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

        public Vector3 ClampWorldPoint(Vector3 worldPoint)
        {
            var localPoint = transform.InverseTransformPoint(worldPoint) - _center;
            var halfWidth = _width * 0.5f;
            var halfHeight = _height * 0.5f;
            localPoint.x = Mathf.Clamp(localPoint.x, -halfWidth, halfWidth);
            localPoint.y = Mathf.Clamp(localPoint.y, -halfHeight, halfHeight);
            return transform.TransformPoint(_center + localPoint);
        }

        public void ConfigureVerticalWorldExtent(float worldBottomY, float worldTopY)
        {
            var safeTop = Mathf.Max(worldBottomY + 0.1f, worldTopY);
            var horizontalAnchor = transform.TransformPoint(new Vector3(_center.x, 0f, _center.z));
            var worldCenter = new Vector3(
                horizontalAnchor.x,
                (worldBottomY + safeTop) * 0.5f,
                horizontalAnchor.z);
            _center = transform.InverseTransformPoint(worldCenter);
            _height = safeTop - worldBottomY;
        }

        public void ConfigureHorizontalWorldExtent(float worldWidth)
        {
            _width = Mathf.Max(0.1f, worldWidth);
        }

        private float GetWorldCornerY(float verticalSign)
        {
            var halfHeight = _height * 0.5f;
            var localPoint = _center + new Vector3(0f, halfHeight * verticalSign, 0f);
            return transform.TransformPoint(localPoint).y;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawAreaDebug)
            {
                return;
            }

            DrawAreaDebug();
        }

        private void DrawAreaDebug()
        {
            var bottomLeft = BottomLeft;
            var bottomRight = BottomRight;
            var topLeft = TopLeft;
            var topRight = TopRight;

            Gizmos.color = new Color(0.2f, 0.95f, 0.25f, 0.95f);
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);

            var invalidBottomY = WorldBottomY - Mathf.Max(0.05f, _invalidAreaDepth);
            var invalidBottomLeft = new Vector3(bottomLeft.x, invalidBottomY, bottomLeft.z);
            var invalidBottomRight = new Vector3(bottomRight.x, invalidBottomY, bottomRight.z);
            var invalidTopLeft = bottomLeft;
            var invalidTopRight = bottomRight;

            Gizmos.color = new Color(0.95f, 0.2f, 0.2f, 0.95f);
            Gizmos.DrawLine(invalidBottomLeft, invalidBottomRight);
            Gizmos.DrawLine(invalidBottomRight, invalidTopRight);
            Gizmos.DrawLine(invalidTopRight, invalidTopLeft);
            Gizmos.DrawLine(invalidTopLeft, invalidBottomLeft);

            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(
                (topLeft + topRight) * 0.5f + Vector3.up * 0.2f,
                "Valid Gameplay Area");
            UnityEditor.Handles.Label(
                (invalidBottomLeft + invalidBottomRight) * 0.5f + Vector3.up * 0.1f,
                "Invalid Below Platform");
        }

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

            _invalidAreaDepth = Mathf.Max(0.05f, _invalidAreaDepth);
        }
#endif
    }
}
