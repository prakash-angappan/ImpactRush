using System.Threading.Tasks;
using ImpactRush.Core.Interfaces;
using ImpactRush.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using ImpactRush.Core;
using GameScene = ImpactRush.Core.GameScene;

namespace ImpactRush.Core.Managers
{
    /// <summary>
    /// Asynchronous scene loading backed by Unity SceneManager.
    /// Uses <see cref="GameScene"/> instead of raw scene name strings.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class SceneLoader : MonoBehaviour, IGameService
    {
        private bool _isLoading;

        public bool IsLoading => _isLoading;
        public float Progress { get; private set; }

        public async Task LoadSceneAsync(GameScene scene, LoadSceneMode mode = LoadSceneMode.Single)
        {
            Guard.AgainstFalse(!_isLoading, "A scene load is already in progress.");

            _isLoading = true;
            Progress = 0f;

            try
            {
                var operation = SceneManager.LoadSceneAsync(scene.ToSceneName(), mode);
                Guard.AgainstNull(operation, nameof(operation));

                operation.allowSceneActivation = true;

                while (!operation.isDone)
                {
                    Progress = operation.progress;
                    await Task.Yield();
                }

                Progress = 1f;
            }
            finally
            {
                _isLoading = false;
            }
        }
    }
}
