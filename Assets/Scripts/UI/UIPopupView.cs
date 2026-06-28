using System;
using System.Collections;
using UnityEngine;

namespace ImpactRush.UI
{
    /// <summary>
    /// Shared popup show/hide animation for panels managed by <see cref="PopupManager"/>.
    /// </summary>
    public class UIPopupView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private float _fadeDuration = 0.18f;
        [SerializeField] private float _scaleDuration = 0.18f;
        [SerializeField] private float _hiddenScale = 0.92f;

        private Coroutine _animationRoutine;

        public string PopupId => gameObject.name;
        public bool IsVisible => gameObject.activeSelf;

        public event Action Closed;

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            if (_contentRoot == null)
            {
                _contentRoot = transform as RectTransform;
            }
        }

        public virtual void Show(Action onComplete = null)
        {
            gameObject.SetActive(true);
            PlayShowAnimation(onComplete);
        }

        public virtual void Hide(Action onComplete = null)
        {
            PlayHideAnimation(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
                Closed?.Invoke();
            });
        }

        protected void PlayShowAnimation(Action onComplete)
        {
            RestartAnimation(AnimateShow(onComplete));
        }

        protected void PlayHideAnimation(Action onComplete)
        {
            RestartAnimation(AnimateHide(onComplete));
        }

        private void RestartAnimation(IEnumerator routine)
        {
            if (_animationRoutine != null)
            {
                StopCoroutine(_animationRoutine);
            }

            _animationRoutine = StartCoroutine(routine);
        }

        private IEnumerator AnimateShow(Action onComplete)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            if (_contentRoot != null)
            {
                _contentRoot.localScale = Vector3.one * _hiddenScale;
            }

            var elapsed = 0f;
            var duration = Mathf.Max(_fadeDuration, _scaleDuration);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);

                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = t;
                }

                if (_contentRoot != null)
                {
                    _contentRoot.localScale = Vector3.Lerp(Vector3.one * _hiddenScale, Vector3.one, t);
                }

                yield return null;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            if (_contentRoot != null)
            {
                _contentRoot.localScale = Vector3.one;
            }

            onComplete?.Invoke();
        }

        private IEnumerator AnimateHide(Action onComplete)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            var elapsed = 0f;
            var duration = Mathf.Max(_fadeDuration, _scaleDuration);
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);

                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 1f - t;
                }

                if (_contentRoot != null)
                {
                    _contentRoot.localScale = Vector3.Lerp(Vector3.one, Vector3.one * _hiddenScale, t);
                }

                yield return null;
            }

            onComplete?.Invoke();
        }
    }
}
