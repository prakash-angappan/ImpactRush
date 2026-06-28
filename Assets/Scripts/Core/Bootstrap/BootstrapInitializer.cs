using ImpactRush.Core.Loading;
using UnityEngine;

namespace ImpactRush.Core.Bootstrap
{
    /// <summary>
    /// Entry point for the Bootstrap scene. Transitions to the main menu on startup.
    /// Attach to a root GameObject in the Bootstrap scene.
    /// </summary>
    public sealed class BootstrapInitializer : MonoBehaviour
    {
        [SerializeField] private string _targetScene = SceneNames.MainMenu;

        private ISceneLoadService _sceneLoadService;

        private void Awake()
        {
            _sceneLoadService = new SceneLoadService();
        }

        private async void Start()
        {
            await _sceneLoadService.LoadSceneAsync(_targetScene);
        }
    }
}
