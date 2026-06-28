using UnityEngine;

namespace ImpactRush.Utilities
{
    /// <summary>
    /// Canonical layer names and mask helpers for physics and rendering.
    /// </summary>
    public static class Layers
    {
        public const string Default = "Default";
        public const string Aim = "Aim";
        public const string TransparentFx = "TransparentFX";
        public const string IgnoreRaycast = "Ignore Raycast";
        public const string Water = "Water";
        public const string Ui = "UI";

        public static int Get(string layerName) => LayerMask.NameToLayer(layerName);

        public static int Mask(string layerName) => 1 << Get(layerName);

        public static bool IsInLayerMask(GameObject gameObject, LayerMask layerMask)
        {
            return (layerMask.value & (1 << gameObject.layer)) != 0;
        }
    }
}
