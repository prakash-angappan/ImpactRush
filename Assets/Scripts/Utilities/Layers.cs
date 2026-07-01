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
        public const string AimPlane = "AimPlane";
        public const string Breakable = "Breakable";
        public const string Ground = "Ground";
        public const string Platform = "Platform";
        public const string Projectile = "Projectile";
        public const string GameplayBounds = "GameplayBounds";
        public const string Environment = "Environment";
        public const string TransparentFx = "TransparentFX";
        public const string IgnoreRaycast = "Ignore Raycast";
        public const string Water = "Water";
        public const string Ui = "UI";

        private static LayerMask? _breakableImpactMask;
        private static LayerMask? _projectileCastMask;

        public static LayerMask BreakableImpactMask
        {
            get
            {
                _breakableImpactMask ??= Mask(Breakable);
                return _breakableImpactMask.Value;
            }
        }

        public static LayerMask ProjectileCastMask
        {
            get
            {
                _projectileCastMask ??= Mask(Breakable, Ground, Platform);
                return _projectileCastMask.Value;
            }
        }

        public static int Get(string layerName) => LayerMask.NameToLayer(layerName);

        public static bool TrySetLayer(GameObject gameObject, string layerName)
        {
            if (gameObject == null)
            {
                return false;
            }

            var layer = Get(layerName);
            if (layer < 0)
            {
                return false;
            }

            gameObject.layer = layer;
            return true;
        }

        public static int Mask(string layerName)
        {
            var layer = Get(layerName);
            return layer >= 0 ? 1 << layer : 0;
        }

        public static LayerMask Mask(params string[] layerNames)
        {
            var mask = 0;
            for (var i = 0; i < layerNames.Length; i++)
            {
                mask |= Mask(layerNames[i]);
            }

            return mask;
        }

        public static void InvalidateCachedMasks()
        {
            _breakableImpactMask = null;
            _projectileCastMask = null;
        }

        public static bool IsInLayerMask(GameObject gameObject, LayerMask layerMask)
        {
            return (layerMask.value & (1 << gameObject.layer)) != 0;
        }
    }
}
