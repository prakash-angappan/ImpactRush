using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ImpactRush.UI
{
    /// <summary>
    /// Simple scale feedback for uGUI buttons.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float _hoverScale = 1.05f;
        [SerializeField] private float _pressedScale = 0.94f;
        [SerializeField] private float _animationSpeed = 12f;

        private RectTransform _rectTransform;
        private Vector3 _normalScale = Vector3.one;
        private Vector3 _targetScale = Vector3.one;
        private bool _isPressed;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _normalScale = _rectTransform.localScale;
            _targetScale = _normalScale;
        }

        private void Update()
        {
            _rectTransform.localScale = Vector3.Lerp(
                _rectTransform.localScale,
                _targetScale,
                Time.unscaledDeltaTime * _animationSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isPressed)
            {
                return;
            }

            _targetScale = _normalScale * _hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isPressed)
            {
                return;
            }

            _targetScale = _normalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            _targetScale = _normalScale * _pressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            _targetScale = _normalScale * _hoverScale;
        }
    }
}
