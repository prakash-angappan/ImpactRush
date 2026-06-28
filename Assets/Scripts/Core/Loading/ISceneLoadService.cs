using System.Threading.Tasks;

namespace ImpactRush.Core.Loading
{
    /// <summary>
    /// Abstraction for asynchronous scene loading (Dependency Inversion).
    /// </summary>
    public interface ISceneLoadService
    {
        Task LoadSceneAsync(string sceneName);
    }
}
