using UnityEngine;

namespace ImpactRush.UI
{
    /// <summary>
    /// Applies mobile safe-area padding to a root rect transform.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private bool _applyOnAwake = true;

        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            if (_target == null)
            {
                _target = transform as RectTransform;
            }

            if (_applyOnAwake)
            {
                ApplySafeArea();
            }
        }

        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea || _lastScreenSize.x != Screen.width || _lastScreenSize.y != Screen.height)
            {
                ApplySafeArea();
            }
        }

        public void ApplySafeArea()
        {
            if (_target == null)
            {
                return;
            }

            var safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _target.anchorMin = anchorMin;
            _target.anchorMax = anchorMax;
            _target.offsetMin = Vector2.zero;
            _target.offsetMax = Vector2.zero;
        }
    }
}
