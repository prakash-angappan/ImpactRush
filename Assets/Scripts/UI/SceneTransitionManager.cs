using System;
using System.Threading.Tasks;
using ImpactRush.Core;
using ImpactRush.Core.Events;
using ImpactRush.Core.Interfaces;
using ImpactRush.Core.Managers;
using ImpactRush.Utilities;
using UnityEngine;
using GameScene = ImpactRush.Core.GameScene;

namespace ImpactRush.UI
{
    /// <summary>
    /// Handles fade transitions and scene loading requests.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SceneTransitionManager : MonoBehaviour, IGameService, IInitializable
    {
        [SerializeField] private FadeOverlayView _fadeOverlay;
        [SerializeField] private PopupManager _popupManager;

        private SceneLoader _sceneLoader;
        private bool _isTransitioning;

        public void Initialize()
        {
            _sceneLoader = ServiceLocator.Get<SceneLoader>();
            Guard.AgainstNull(_fadeOverlay, nameof(_fadeOverlay));
            EventBus.Subscribe<SceneTransitionRequestedEvent>(OnTransitionRequested);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SceneTransitionRequestedEvent>(OnTransitionRequested);
        }

        public async Task TransitionToAsync(GameScene scene, bool showLoadingPopup = false)
        {
            if (_isTransitioning)
            {
                return;
            }

            _isTransitioning = true;
            var loadingPopupOpened = false;
            try
            {
                if (showLoadingPopup && _popupManager != null)
                {
                    _popupManager.OpenPopup(UIPopupIds.Loading);
                    loadingPopupOpened = true;
                }

                await RunFade(_fadeOverlay.FadeOut());
                await _sceneLoader.LoadSceneAsync(scene);
                EventBus.Publish(new SceneTransitionCompletedEvent(scene));
                await RunFade(_fadeOverlay.FadeIn());
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EventBus.Publish(new SceneTransitionFailedEvent(scene, exception.Message));
            }
            finally
            {
                if (loadingPopupOpened && _popupManager != null)
                {
                    _popupManager.ClosePopup(UIPopupIds.Loading);
                }

                _isTransitioning = false;
            }
        }

        private void OnTransitionRequested(SceneTransitionRequestedEvent transitionEvent)
        {
            _ = HandleTransitionRequestedAsync(transitionEvent);
        }

        private async Task HandleTransitionRequestedAsync(SceneTransitionRequestedEvent transitionEvent)
        {
            await TransitionToAsync(transitionEvent.Scene, transitionEvent.ShowLoadingPopup);
        }

        private static async Task RunFade(System.Collections.IEnumerator routine)
        {
            while (routine.MoveNext())
            {
                await Task.Yield();
            }
        }
    }
}
