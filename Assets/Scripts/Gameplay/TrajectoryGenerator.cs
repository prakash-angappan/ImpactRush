using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Builds straight-line shots from spawn toward the screen target.
    /// </summary>
    public static class TrajectoryGenerator
    {
        public static ShotData Create(
            Vector3 spawnPosition,
            Vector3 targetPosition,
            float maxGameplayDistance,
            float minimumArcHeight,
            float maximumArcHeight,
            float travelTime)
        {
            var toTarget = targetPosition - spawnPosition;
            var distance = toTarget.magnitude;
            var distanceFactor = Mathf.Clamp01(distance / Mathf.Max(0.01f, maxGameplayDistance));
            var initialDirection = NormalizeDirection(toTarget, Vector3.forward);
            var safeTravelTime = Mathf.Max(0.01f, travelTime);
            var maxRange = Mathf.Max(0.01f, maxGameplayDistance);
            var projectileSpeed = maxRange / safeTravelTime;
            var travelEnd = spawnPosition + (initialDirection * maxRange);

            return new ShotData(
                spawnPosition,
                targetPosition,
                travelEnd,
                initialDirection,
                0f,
                safeTravelTime,
                distance,
                distanceFactor,
                projectileSpeed);
        }

        private static Vector3 NormalizeDirection(Vector3 primary, Vector3 fallback)
        {
            if (primary.sqrMagnitude > 0.0001f)
            {
                return primary.normalized;
            }

            return fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector3.forward;
        }
    }
}
