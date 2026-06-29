using ImpactRush.Core.Events;
using ImpactRush.Core.Managers;
using UnityEngine;
using GameScene = ImpactRush.Core.GameScene;

namespace ImpactRush.Core.Bootstrap
{
    /// <summary>
    /// Bootstrap scene entry point. Loads core managers then transitions to MainMenu.
    /// Attach to the GameBootstrap root object in the Bootstrap scene.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameScene _initialScene = GameScene.MainMenu;

        public GameScene InitialScene => _initialScene;

        private void OnEnable()
        {
            EventBus.Subscribe<SceneTransitionFailedEvent>(OnSceneTransitionFailed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SceneTransitionFailedEvent>(OnSceneTransitionFailed);
        }

        private static void OnSceneTransitionFailed(SceneTransitionFailedEvent failedEvent)
        {
            Debug.LogError(
                $"Bootstrap failed to load initial scene '{failedEvent.Scene}': {failedEvent.ErrorMessage}");
        }
    }
}
