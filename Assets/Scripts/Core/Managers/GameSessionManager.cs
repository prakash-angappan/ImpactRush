using ImpactRush.Core.Events;
using ImpactRush.Core.Interfaces;
using GameScene = ImpactRush.Core.GameScene;
using UnityEngine;

namespace ImpactRush.Core.Managers
{
    /// <summary>
    /// Gameplay session state: pause, resume, restart, level completion, and scene requests.
    /// </summary>
    public sealed class GameSessionManager : MonoBehaviour, IGameService, IInitializable
    {
        [SerializeField] private int _startingLevel = 1;

        public bool IsPaused { get; private set; }
        public bool IsLevelComplete { get; private set; }
        public int CurrentLevel { get; private set; }

        public void Initialize()
        {
            CurrentLevel = _startingLevel;
            EventBus.Subscribe<SceneTransitionCompletedEvent>(OnSceneTransitionCompleted);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SceneTransitionCompletedEvent>(OnSceneTransitionCompleted);
        }

        public void Pause()
        {
            if (IsPaused)
            {
                return;
            }

            IsPaused = true;
            Time.timeScale = 0f;
            EventBus.Publish(new GamePausedEvent());
        }

        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = 1f;
            EventBus.Publish(new GameResumedEvent());
        }

        public void RestartLevel()
        {
            IsLevelComplete = false;
            IsPaused = false;
            Time.timeScale = 1f;
            EventBus.Publish(new SceneTransitionRequestedEvent(GameScene.Gameplay, showLoadingPopup: true));
        }

        public void LoadMainMenu()
        {
            IsLevelComplete = false;
            IsPaused = false;
            Time.timeScale = 1f;
            EventBus.Publish(new SceneTransitionRequestedEvent(GameScene.MainMenu));
        }

        public void LoadGameplay()
        {
            IsLevelComplete = false;
            IsPaused = false;
            Time.timeScale = 1f;
            EventBus.Publish(new SceneTransitionRequestedEvent(GameScene.Gameplay));
        }

        public void NotifyLevelComplete()
        {
            if (IsLevelComplete)
            {
                return;
            }

            IsLevelComplete = true;
            Pause();
            EventBus.Publish(new LevelCompleteDetectedEvent());
        }

        private void OnSceneTransitionCompleted(SceneTransitionCompletedEvent transitionEvent)
        {
            IsLevelComplete = false;
            IsPaused = false;
            Time.timeScale = 1f;

            if (transitionEvent.Scene == GameScene.Gameplay)
            {
                CurrentLevel = _startingLevel;
            }
        }
    }
}
