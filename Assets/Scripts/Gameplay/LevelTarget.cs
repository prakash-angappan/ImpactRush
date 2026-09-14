using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Marker component identifying a knock-down target in the stack.
    /// <para>
    /// RECONSTRUCTED FILE: the original was referenced by <see cref="LevelCompleteMonitor"/>
    /// and <see cref="TargetStackPhysics"/> (via GetComponentsInChildren / FindObjectsByType)
    /// but was never committed to git. It is rebuilt here as a pure marker, which is exactly
    /// how the surviving code uses it (only <c>gameObject</c> and <c>transform</c> are read).
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelTarget : MonoBehaviour
    {
    }
}
