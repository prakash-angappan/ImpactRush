using System.Collections.Generic;
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
    /// Switches active screens and coordinates HUD, pause, and win UI.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIManager : MonoBehaviour, IGameService, IInitializable
    {
        [SerializeField] private UIScreenView _mainMenuScreen;
        [SerializeField] private UIScreenView _gameHudScreen;
        [SerializeField] private PopupManager _popupManager;
        private readonly Dictionary<string, UIScreenView> _screens = new();
        private GameSessionManager _sessionManager;
        private GameHudScreenView _gameHudView;

        public void Initialize()
        {
            _sessionManager = ServiceLocator.Get<GameSessionManager>();
            Guard.AgainstNull(_popupManager, nameof(_popupManager));

            RegisterScreen(_mainMenuScreen);
            RegisterScreen(_gameHudScreen);
            _gameHudView = _gameHudScreen as GameHudScreenView;

            EventBus.Subscribe<SceneTransitionCompletedEvent>(OnSceneTransitionCompleted);
            EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Subscribe<GameResumedEvent>(OnGameResumed);
            EventBus.Subscribe<LevelCompleteDetectedEvent>(OnLevelComplete);
            EventBus.Subscribe<LevelFailedDetectedEvent>(OnLevelFailed);
            EventBus.Subscribe<BallsRemainingChangedEvent>(OnBallsRemainingChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SceneTransitionCompletedEvent>(OnSceneTransitionCompleted);
            EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Unsubscribe<GameResumedEvent>(OnGameResumed);
            EventBus.Unsubscribe<LevelCompleteDetectedEvent>(OnLevelComplete);
            EventBus.Unsubscribe<LevelFailedDetectedEvent>(OnLevelFailed);
            EventBus.Unsubscribe<BallsRemainingChangedEvent>(OnBallsRemainingChanged);
        }

        public void ShowScreen(string screenId)
        {
            foreach (var pair in _screens)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                if (pair.Key == screenId)
                {
                    pair.Value.Show();
                }
                else
                {
                    pair.Value.Hide();
                }
            }
        }

        public void ShowPauseMenu()
        {
            _sessionManager.Pause();
            _popupManager.OpenPopup(UIPopupIds.Pause);
        }

        public void HidePauseMenu()
        {
            _popupManager.ClosePopup(UIPopupIds.Pause);
            _sessionManager.Resume();
        }

        public void ShowLevelComplete()
        {
            _popupManager.OpenPopup(UIPopupIds.LevelComplete);
        }

        public void ShowLevelFailed()
        {
            _popupManager.OpenPopup(UIPopupIds.LevelFailed);
        }

        public void ShowSettings()
        {
            _popupManager.OpenPopup(UIPopupIds.Settings);
        }

        public void ShowExitConfirmation()
        {
            _popupManager.OpenPopup(UIPopupIds.ExitConfirmation);
        }

        private void OnSceneTransitionCompleted(SceneTransitionCompletedEvent transitionEvent)
        {
            _popupManager.CloseAllPopups();

            switch (transitionEvent.Scene)
            {
                case GameScene.MainMenu:
                    ShowScreen(UIScreenIds.MainMenu);
                    break;
                case GameScene.Gameplay:
                    ShowScreen(UIScreenIds.GameHud);
                    RefreshLevelLabel();
                    RefreshBallsLabel();
                    break;
            }
        }

        private void OnGamePaused(GamePausedEvent _)
        {
        }

        private void OnGameResumed(GameResumedEvent _)
        {
            _popupManager.ClosePopup(UIPopupIds.Pause);
        }

        private void OnLevelComplete(LevelCompleteDetectedEvent _)
        {
            ShowLevelComplete();
        }

        private void OnLevelFailed(LevelFailedDetectedEvent _)
        {
            ShowLevelFailed();
        }

        private void OnBallsRemainingChanged(BallsRemainingChangedEvent ballsEvent)
        {
            _gameHudView?.SetBallsRemainingLabel(ballsEvent.BallsRemaining);
        }

        private void RefreshLevelLabel()
        {
            var level = _sessionManager != null ? _sessionManager.CurrentLevel : 1;
            _gameHudView?.SetLevelLabel($"LEVEL {level}");
        }

        private void RefreshBallsLabel()
        {
            var balls = _sessionManager != null ? _sessionManager.BallsRemaining : 0;
            _gameHudView?.SetBallsRemainingLabel(balls);
        }

        private void RegisterScreen(UIScreenView screen)
        {
            if (screen == null)
            {
                return;
            }

            screen.Hide();
            _screens[screen.ScreenId] = screen;
        }
    }
}
