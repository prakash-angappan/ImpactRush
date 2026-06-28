using System.Threading.Tasks;
using ImpactRush.Core.Loading;
using ImpactRush.Utilities;

namespace ImpactRush.Core.Managers
{
    /// <summary>
    /// Coordinates high-level scene flow. Gameplay systems depend on this facade
    /// rather than calling SceneManager directly (Single Responsibility).
    /// </summary>
    public sealed class SceneFlowManager
    {
        private readonly ISceneLoadService _sceneLoadService;

        public SceneFlowManager(ISceneLoadService sceneLoadService)
        {
            Guard.AgainstNull(sceneLoadService, nameof(sceneLoadService));
            _sceneLoadService = sceneLoadService;
        }

        public Task LoadMainMenuAsync() => _sceneLoadService.LoadSceneAsync(SceneNames.MainMenu);

        public Task LoadGameplayAsync() => _sceneLoadService.LoadSceneAsync(SceneNames.Gameplay);

        public Task LoadBootstrapAsync() => _sceneLoadService.LoadSceneAsync(SceneNames.Bootstrap);
    }
}
