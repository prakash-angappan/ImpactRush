using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Authoritative data for a single straight-line shot.
    /// </summary>
    public readonly struct ShotData
    {
        public ShotData(
            Vector3 spawnPosition,
            Vector3 targetPosition,
            Vector3 controlPoint,
            Vector3 initialDirection,
            float arcHeight,
            float travelTime,
            float distance,
            float distanceFactor,
            float projectileSpeed)
        {
            SpawnPosition = spawnPosition;
            TargetPosition = targetPosition;
            ControlPoint = controlPoint;
            InitialDirection = initialDirection;
            ArcHeight = arcHeight;
            TravelTime = travelTime;
            Distance = distance;
            DistanceFactor = distanceFactor;
            ProjectileSpeed = projectileSpeed;
        }

        public Vector3 SpawnPosition { get; }
        public Vector3 TargetPosition { get; }
        public Vector3 ControlPoint { get; }
        public Vector3 InitialDirection { get; }
        public float ArcHeight { get; }
        public float TravelTime { get; }
        public float Distance { get; }
        public float DistanceFactor { get; }
        public float ProjectileSpeed { get; }

        public Vector3 Evaluate(float t)
        {
            var clamped = Mathf.Clamp01(t);
            return Vector3.Lerp(SpawnPosition, ControlPoint, clamped);
        }

        public Vector3 EvaluateTangent(float t)
        {
            return ControlPoint - SpawnPosition;
        }

        public Vector3 EvaluateWorldVelocity(float t)
        {
            return InitialDirection * ProjectileSpeed;
        }
    }
}
