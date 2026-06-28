using System.Threading.Tasks;
using ImpactRush.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ImpactRush.Core.Loading
{
    /// <summary>
    /// Default scene loading implementation backed by Unity's SceneManager.
    /// </summary>
    public sealed class SceneLoadService : ISceneLoadService
    {
        public async Task LoadSceneAsync(string sceneName)
        {
            Guard.AgainstNullOrEmpty(sceneName, nameof(sceneName));

            var operation = SceneManager.LoadSceneAsync(sceneName);
            Guard.AgainstNull(operation, nameof(operation));

            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }
    }
}
