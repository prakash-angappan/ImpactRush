using ImpactRush.Core.Managers;
using GameScene = ImpactRush.Core.GameScene;

namespace ImpactRush.Core.Events
{
    public readonly struct SceneTransitionRequestedEvent : IGameEvent
    {
        public SceneTransitionRequestedEvent(GameScene scene, bool showLoadingPopup = false)
        {
            Scene = scene;
            ShowLoadingPopup = showLoadingPopup;
        }

        public GameScene Scene { get; }
        public bool ShowLoadingPopup { get; }
    }

    public readonly struct SceneTransitionCompletedEvent : IGameEvent
    {
        public SceneTransitionCompletedEvent(GameScene scene)
        {
            Scene = scene;
        }

        public GameScene Scene { get; }
    }

    public readonly struct SceneTransitionFailedEvent : IGameEvent
    {
        public SceneTransitionFailedEvent(GameScene scene, string errorMessage)
        {
            Scene = scene;
            ErrorMessage = errorMessage;
        }

        public GameScene Scene { get; }
        public string ErrorMessage { get; }
    }

    public readonly struct GamePausedEvent : IGameEvent
    {
    }

    public readonly struct GameResumedEvent : IGameEvent
    {
    }

    public readonly struct LevelCompleteDetectedEvent : IGameEvent
    {
    }

    public readonly struct LevelFailedDetectedEvent : IGameEvent
    {
    }

    public readonly struct ProjectileHitEvent : IGameEvent
    {
    }

    public readonly struct BallsRemainingChangedEvent : IGameEvent
    {
        public BallsRemainingChangedEvent(int ballsRemaining)
        {
            BallsRemaining = ballsRemaining;
        }

        public int BallsRemaining { get; }
    }


    public readonly struct PlaySfxEvent : IGameEvent
    {
        public PlaySfxEvent(string clipId)
        {
            ClipId = clipId;
        }

        public string ClipId { get; }
    }

    public readonly struct PlayMusicEvent : IGameEvent
    {
        public PlayMusicEvent(string clipId, bool loop = true)
        {
            ClipId = clipId;
            Loop = loop;
        }

        public string ClipId { get; }
        public bool Loop { get; }
    }

    public readonly struct GameplayHintRequestedEvent : IGameEvent
    {
        public GameplayHintRequestedEvent(
            string message,
            float duration = 1f,
            float fadeTime = 0.2f,
            float fontSize = -1f)
        {
            Message = message;
            Duration = duration;
            FadeTime = fadeTime;
            FontSize = fontSize;
        }

        public string Message { get; }
        public float Duration { get; }
        public float FadeTime { get; }
        public float FontSize { get; }
    }
}
