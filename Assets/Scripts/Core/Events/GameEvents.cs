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

    /// <summary>Raised when a projectile is launched. Lets systems (audio, analytics, tutorials)
    /// react to firing without referencing the cannon.</summary>
    public readonly struct ProjectileSpawnedEvent : IGameEvent
    {
        public ProjectileSpawnedEvent(int activeProjectiles)
        {
            ActiveProjectiles = activeProjectiles;
        }

        public int ActiveProjectiles { get; }
    }

    /// <summary>Raised when a projectile completes its lifecycle and returns to the pool.</summary>
    public readonly struct ProjectileReturnedToPoolEvent : IGameEvent
    {
        public ProjectileReturnedToPoolEvent(int activeProjectiles)
        {
            ActiveProjectiles = activeProjectiles;
        }

        public int ActiveProjectiles { get; }
    }

    /// <summary>Raised when a breakable object is destroyed, so managers can update platform/level
    /// state without direct references to the breakable.</summary>
    public readonly struct BreakableDestroyedEvent : IGameEvent
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

    /// <summary>Raised when the player selects a different projectile type in the carousel. Carries the
    /// library index only, keeping the Core event free of gameplay/UI type coupling.</summary>
    public readonly struct BallSelectionChangedEvent : IGameEvent
    {
        public BallSelectionChangedEvent(int selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }

        public int SelectedIndex { get; }
    }


    /// <summary>Raised when a status effect is applied to an object. Lets session/analytics/UI react
    /// without referencing the status runtime or the affected object.</summary>
    public readonly struct StatusEffectAppliedEvent : IGameEvent
    {
        public StatusEffectAppliedEvent(string statusId)
        {
            StatusId = statusId;
        }

        public string StatusId { get; }
    }

    /// <summary>Raised when a status effect is removed or expires from an object.</summary>
    public readonly struct StatusEffectRemovedEvent : IGameEvent
    {
        public StatusEffectRemovedEvent(string statusId, bool expired)
        {
            StatusId = statusId;
            Expired = expired;
        }

        public string StatusId { get; }
        public bool Expired { get; }
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
