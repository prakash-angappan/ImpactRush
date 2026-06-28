using ImpactRush.Audio;
using ImpactRush.Core.Events;
using ImpactRush.Core.Managers;
using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Impact tuning applied when a projectile hits valid scene geometry.
    /// </summary>
    [System.Serializable]
    public struct ImpactSettings
    {
        public float ImpactForce;
        public float ExplosionRadius;
        public float ExplosionForce;
    }

    /// <summary>
    /// Applies impulses to impacted rigidbodies and optional radial falloff.
    /// </summary>
    public static class ImpactHandler
    {
        public static void ProcessImpact(RaycastHit hit, Vector3 travelVelocity, ImpactSettings settings)
        {
            if (hit.collider == null || !IsValidImpactCollider(hit.collider))
            {
                return;
            }

            ApplyImpact(hit.point, travelVelocity, settings, hit.collider);
            EventBus.Publish(new PlaySfxEvent(AudioIds.Impact));
        }

        private static void ApplyImpact(Vector3 impactPoint, Vector3 travelVelocity, ImpactSettings settings, Collider primaryCollider)
        {
            var impulse = travelVelocity * settings.ImpactForce;
            if (primaryCollider.attachedRigidbody != null)
            {
                primaryCollider.attachedRigidbody.AddForceAtPosition(impulse, impactPoint, ForceMode.Impulse);
            }

            if (settings.ExplosionRadius <= 0f || settings.ExplosionForce <= 0f)
            {
                return;
            }

            var overlaps = UnityEngine.Physics.OverlapSphere(impactPoint, settings.ExplosionRadius, UnityEngine.Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < overlaps.Length; i++)
            {
                var collider = overlaps[i];
                if (!IsValidImpactCollider(collider))
                {
                    continue;
                }

                var rigidbody = collider.attachedRigidbody;
                if (rigidbody == null)
                {
                    continue;
                }

                var offset = collider.bounds.center - impactPoint;
                var distance = offset.magnitude;
                if (distance <= 0.0001f)
                {
                    rigidbody.AddForce(impulse, ForceMode.Impulse);
                    continue;
                }

                var falloff = 1f - Mathf.Clamp01(distance / settings.ExplosionRadius);
                rigidbody.AddForceAtPosition(offset.normalized * (settings.ExplosionForce * falloff), impactPoint, ForceMode.Impulse);
            }
        }

        private static bool IsValidImpactCollider(Collider collider)
        {
            if (collider == null || !collider.enabled || collider.isTrigger)
            {
                return false;
            }

            return collider.gameObject.layer != Layers.Get(Layers.Aim);
        }
    }
}
