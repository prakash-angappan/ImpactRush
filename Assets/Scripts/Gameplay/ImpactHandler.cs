using ImpactRush.Gameplay.Impacts;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Impact tuning applied when a projectile hits valid scene geometry.
    /// </summary>
    [System.Serializable]
    public struct ProjectileImpactSettings
    {
        public float ImpactRadius;
        public float MaximumForce;
        public float MinimumForce;
        public float ExplosionUpwardModifier;

        public ImpactSettings ToImpactSettings()
        {
            return new ImpactSettings
            {
                DirectImpactForce = MaximumForce,
                NeighbourRadius = ImpactRadius,
                MaximumNeighbours = 4,
                NeighbourForceMultiplier = 0.35f,
                MaximumTorque = MaximumForce * 0.05f,
                ProjectileEnergyLoss = 0.35f,
                UpwardModifier = ExplosionUpwardModifier,
                MinimumForce = MinimumForce,
            };
        }
    }

    /// <summary>
    /// Compatibility entry points that delegate to <see cref="ImpactResolver"/>.
    /// </summary>
    public static class ImpactHandler
    {
        public static void ProcessImpact(RaycastHit hit, Vector3 travelVelocity, ProjectileImpactSettings settings)
        {
            if (hit.collider == null)
            {
                return;
            }

            var direction = travelVelocity.sqrMagnitude > 0.0001f
                ? travelVelocity.normalized
                : (hit.normal.sqrMagnitude > 0.0001f ? -hit.normal.normalized : Vector3.forward);

            ProcessAreaImpact(hit.point, direction, travelVelocity.magnitude, settings, hit.normal);
        }

        public static void ProcessAreaImpact(
            Vector3 position,
            Vector3 travelDirection,
            float travelSpeed,
            ImpactSettings settings,
            Vector3 normal)
        {
            Resolve(position, travelDirection, travelSpeed, settings, normal, null);
        }

        public static void ProcessAreaImpact(
            Vector3 position,
            Vector3 travelDirection,
            float travelSpeed,
            ProjectileImpactSettings settings,
            Vector3 normal)
        {
            ProcessAreaImpact(position, travelDirection, travelSpeed, settings.ToImpactSettings(), normal);
        }

        private static void Resolve(
            Vector3 position,
            Vector3 travelDirection,
            float travelSpeed,
            ImpactSettings settings,
            Vector3 normal,
            Collider hitCollider)
        {
            if (settings.NeighbourRadius <= 0f && settings.DirectImpactForce <= 0f)
            {
                return;
            }

            var resolvedNormal = normal.sqrMagnitude > 0.0001f ? normal : Vector3.up;
            var direction = travelDirection.sqrMagnitude > 0.0001f
                ? travelDirection.normalized
                : resolvedNormal;

            var contact = new ImpactContact(
                position,
                resolvedNormal,
                direction,
                travelSpeed,
                hitCollider);

            if (ImpactResolver.Instance != null)
            {
                ImpactResolver.Instance.Resolve(in contact, in settings);
                return;
            }

            Debug.LogWarning("ImpactResolver is missing. Impact was skipped.");
        }
    }
}
