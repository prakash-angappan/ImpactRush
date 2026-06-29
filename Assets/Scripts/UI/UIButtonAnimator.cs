using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ImpactRush.UI
{
    /// <summary>
    /// Scale and color feedback for uGUI buttons (normal, hover, pressed).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image _background;
        [SerializeField] private float _hoverScale = 1.05f;
        [SerializeField] private float _pressedScale = 0.95f;
        [SerializeField] private float _animationSpeed = 20f;
        [SerializeField] private Color _hoverColor = new(0.28f, 0.55f, 1f, 1f);
        [SerializeField] private Color _pressedColor = new(0.12f, 0.32f, 0.72f, 1f);

        private RectTransform _rectTransform;
        private Vector3 _normalScale = Vector3.one;
        private Vector3 _targetScale = Vector3.one;
        private Color _normalColor = Color.white;
        private Color _targetColor = Color.white;
        private bool _isPressed;
        private bool _isHovered;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _normalScale = _rectTransform.localScale;
            _targetScale = _normalScale;

            if (_background == null)
            {
                _background = transform.Find("Background")?.GetComponent<Image>();
            }

            if (_background != null)
            {
                _normalColor = _background.color;
                _targetColor = _normalColor;
            }
        }

        private void Update()
        {
            var delta = Time.unscaledDeltaTime * _animationSpeed;
            _rectTransform.localScale = Vector3.Lerp(_rectTransform.localScale, _targetScale, delta);

            if (_background != null)
            {
                _background.color = Color.Lerp(_background.color, _targetColor, delta);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            if (_isPressed)
            {
                return;
            }

            ApplyHoverState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            if (_isPressed)
            {
                return;
            }

            ApplyNormalState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            _targetScale = _normalScale * _pressedScale;
            _targetColor = _pressedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;

            if (_isHovered)
            {
                ApplyHoverState();
            }
            else
            {
                ApplyNormalState();
            }
        }

        private void ApplyNormalState()
        {
            _targetScale = _normalScale;
            _targetColor = _normalColor;
        }

        private void ApplyHoverState()
        {
            _targetScale = _normalScale * _hoverScale;
            _targetColor = _hoverColor;
        }
    }
}
