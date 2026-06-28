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

        private void Start()
        {
            EventBus.Publish(new SceneTransitionRequestedEvent(_initialScene));
        }
    }
}
