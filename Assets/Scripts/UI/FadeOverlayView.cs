using System.Collections;
using ImpactRush.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace ImpactRush.UI
{
    /// <summary>
    /// Full-screen fade overlay used for scene transitions.
    /// </summary>
    public sealed class FadeOverlayView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _fadeImage;
        [SerializeField] private float _fadeDuration = 0.35f;

        private Coroutine _fadeRoutine;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            SetAlpha(0f);
            SetRaycastBlocking(false);
        }

        public IEnumerator FadeOut()
        {
            yield return Animate(0f, 1f, blockRaycasts: true);
        }

        public IEnumerator FadeIn()
        {
            yield return Animate(1f, 0f, blockRaycasts: false);
        }

        private IEnumerator Animate(float from, float to, bool blockRaycasts)
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            SetRaycastBlocking(blockRaycasts);
            var elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / _fadeDuration);
                SetAlpha(Mathf.Lerp(from, to, t));
                yield return null;
            }

            SetAlpha(to);
            SetRaycastBlocking(blockRaycasts && to > 0.01f);
        }

        private void SetAlpha(float alpha)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = alpha;
            }

            if (_fadeImage != null)
            {
                var color = _fadeImage.color;
                color.a = alpha;
                _fadeImage.color = color;
            }
        }

        private void SetRaycastBlocking(bool block)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = block;
                _canvasGroup.interactable = block;
            }
        }
    }
}
