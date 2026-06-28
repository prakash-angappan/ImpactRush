using UnityEngine;

namespace ImpactRush.UI
{
    /// <summary>
    /// Persistent UI root that survives scene loads.
    /// </summary>
    [DefaultExecutionOrder(-90)]
    [DisallowMultipleComponent]
    public sealed class UIRoot : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
