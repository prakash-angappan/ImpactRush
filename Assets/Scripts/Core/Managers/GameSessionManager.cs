using ImpactRush.Core.Events;
using ImpactRush.Core.Interfaces;
using UnityEngine;
using GameScene = ImpactRush.Core.GameScene;

namespace ImpactRush.Core.Managers
{
    /// <summary>
    /// Gameplay session state: pause, resume, restart, level completion, ball budget, and scene requests.
    /// </summary>
    public sealed class GameSessionManager : MonoBehaviour, IGameService, IInitializable
    {
        private const float OutOfBallsFailGraceSeconds = 2.5f;

        [SerializeField] private int _startingLevel = 1;

        public bool IsPaused { get; private set; }
        public bool IsLevelComplete { get; private set; }
        public bool IsLevelFailed { get; private set; }
        public int CurrentLevel { get; private set; }
        public int LevelGenerationSeed { get; private set; }
        public int BallsRemaining { get; private set; }

        private float _ballsDepletedTime = -1f;

        public void Initialize()
        {
            CurrentLevel = _startingLevel;
            LevelGenerationSeed = CreateSeedForLevel(CurrentLevel);
            BallsRemaining = 0;
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
            ResetRunState();
            LevelGenerationSeed = Random.Range(1, int.MaxValue);
            EventBus.Publish(new SceneTransitionRequestedEvent(GameScene.Gameplay, showLoadingPopup: true));
        }

        public void AdvanceToNextLevel()
        {
            ResetRunState();
            CurrentLevel++;
            LevelGenerationSeed = CreateSeedForLevel(CurrentLevel) + Random.Range(1, 9999);
            EventBus.Publish(new SceneTransitionRequestedEvent(GameScene.Gameplay, showLoadingPopup: true));
        }

        public void LoadMainMenu()
        {
            ResetRunState();
            CurrentLevel = _startingLevel;
            LevelGenerationSeed = CreateSeedForLevel(CurrentLevel);
            EventBus.Publish(new SceneTransitionRequestedEvent(GameScene.MainMenu));
        }

        public void LoadGameplay()
        {
            ResetRunState();
            CurrentLevel = _startingLevel;
            LevelGenerationSeed = CreateSeedForLevel(CurrentLevel);
            EventBus.Publish(new SceneTransitionRequestedEvent(GameScene.Gameplay));
        }

        public void NotifyLevelComplete()
        {
            if (IsLevelComplete || IsLevelFailed)
            {
                return;
            }

            IsLevelComplete = true;
            Pause();
            EventBus.Publish(new LevelCompleteDetectedEvent());
        }

        public void NotifyLevelFailed()
        {
            if (IsLevelComplete || IsLevelFailed)
            {
                return;
            }

            IsLevelFailed = true;
            Pause();
            EventBus.Publish(new LevelFailedDetectedEvent());
        }

        public bool TryConsumeBall()
        {
            if (IsPaused || IsLevelComplete || IsLevelFailed || BallsRemaining <= 0)
            {
                return false;
            }

            BallsRemaining--;
            if (BallsRemaining <= 0)
            {
                _ballsDepletedTime = Time.time;
            }

            EventBus.Publish(new BallsRemainingChangedEvent(BallsRemaining));
            return true;
        }

        public bool HasOutOfBallsFailGraceElapsed()
        {
            return BallsRemaining <= 0
                && _ballsDepletedTime >= 0f
                && Time.time >= _ballsDepletedTime + OutOfBallsFailGraceSeconds;
        }

        public void SetBallBudget(int ballsRemaining)
        {
            BallsRemaining = Mathf.Max(0, ballsRemaining);
            _ballsDepletedTime = -1f;
            EventBus.Publish(new BallsRemainingChangedEvent(BallsRemaining));
        }

        private void OnSceneTransitionCompleted(SceneTransitionCompletedEvent transitionEvent)
        {
            ResetRunState();
        }

        private void ResetRunState()
        {
            IsLevelComplete = false;
            IsLevelFailed = false;
            IsPaused = false;
            _ballsDepletedTime = -1f;
            Time.timeScale = 1f;
        }

        private static int CreateSeedForLevel(int levelNumber)
        {
            return levelNumber * 10007;
        }
    }
}
