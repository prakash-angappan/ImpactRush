using UnityEngine;

namespace ImpactRush.Utilities
{
    /// <summary>
    /// Shared extension methods used across runtime assemblies.
    /// </summary>
    public static class Extensions
    {
        public static float ClampVolume(this float volume)
        {
            return Mathf.Clamp(volume, Constants.MinVolume, Constants.MaxVolume);
        }
    }
}
