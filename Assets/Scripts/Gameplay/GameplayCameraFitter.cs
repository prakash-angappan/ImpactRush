using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Keeps horizontal framing consistent on tall portrait screens by widening the vertical
    /// field of view as the aspect ratio narrows.
    /// <para>
    /// RECONSTRUCTED FILE: the original was attached to the gameplay camera by the editor
    /// scene builder (<c>UIFrameworkBuilder</c>) but never committed to git. No other code calls
    /// its API, so this is a self-contained, best-effort reconstruction of a portrait camera fit.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-50)]
    public sealed class GameplayCameraFitter : MonoBehaviour
    {
        [SerializeField] private float _referenceAspect = 9f / 16f;
        [SerializeField] private float _referenceFieldOfView = 60f;
        [SerializeField] private float _minFieldOfView = 30f;
        [SerializeField] private float _maxFieldOfView = 100f;

        private Camera _camera;
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            Fit();
        }

        private void Update()
        {
            if (Screen.width != _lastWidth || Screen.height != _lastHeight)
            {
                Fit();
            }
        }

        private void Fit()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            if (_camera == null || _camera.orthographic)
            {
                return;
            }

            var currentAspect = _lastWidth / Mathf.Max(1f, _lastHeight);
            var scale = Mathf.Clamp(_referenceAspect / Mathf.Max(0.0001f, currentAspect), 0.5f, 2f);
            _camera.fieldOfView = Mathf.Clamp(_referenceFieldOfView * scale, _minFieldOfView, _maxFieldOfView);
        }
    }
}
