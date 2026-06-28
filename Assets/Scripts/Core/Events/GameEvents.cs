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

    public readonly struct GamePausedEvent : IGameEvent
    {
    }

    public readonly struct GameResumedEvent : IGameEvent
    {
    }

    public readonly struct LevelCompleteDetectedEvent : IGameEvent
    {
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
}
