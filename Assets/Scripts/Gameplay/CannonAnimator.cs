using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Rotates base yaw and barrel pitch so the bore (pivot +Z at rest) points at the click target.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CannonAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _basePivot;
        [SerializeField] private Transform _barrelPivot;
        [SerializeField] private Transform _spawnPoint;

        private Quaternion _basePivotLocalRotation;
        private Quaternion _barrelPivotLocalRotation;

        private void Reset()
        {
            _basePivot = transform.Find("BasePivot");
            _barrelPivot = transform.Find("BasePivot/BarrelPivot");
            _spawnPoint = transform.Find("BasePivot/BarrelPivot/Barrel/SpawnPoint");
        }

        private void Awake()
        {
            Guard.AgainstNull(_basePivot, nameof(_basePivot));
            Guard.AgainstNull(_barrelPivot, nameof(_barrelPivot));
            Guard.AgainstNull(_spawnPoint, nameof(_spawnPoint));

            _basePivotLocalRotation = _basePivot.localRotation;
            _barrelPivotLocalRotation = _barrelPivot.localRotation;
        }

        public void AimAt(ShotData shot)
        {
            AimAtTarget(shot.TargetPosition);
        }

        public void AimAtTarget(Vector3 targetWorldPosition)
        {
            ResetPivotRotations();

            var toTarget = targetWorldPosition - _spawnPoint.position;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            ApplyBaseYaw(toTarget.normalized);

            toTarget = targetWorldPosition - _spawnPoint.position;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return;
            }

            AlignBarrelPivot(toTarget.normalized);
        }

        private void ResetPivotRotations()
        {
            _basePivot.localRotation = _basePivotLocalRotation;
            _barrelPivot.localRotation = _barrelPivotLocalRotation;
        }

        private void ApplyBaseYaw(Vector3 worldDirection)
        {
            var planarDirection = Vector3.ProjectOnPlane(worldDirection, Vector3.up);
            if (planarDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            var localPlanar = transform.InverseTransformDirection(planarDirection.normalized);
            localPlanar.y = 0f;
            var yaw = Mathf.Atan2(localPlanar.x, localPlanar.z) * Mathf.Rad2Deg;
            _basePivot.localRotation = _basePivotLocalRotation * Quaternion.Euler(0f, yaw, 0f);
        }

        private void AlignBarrelPivot(Vector3 worldDirection)
        {
            var localDirection = _barrelPivot.InverseTransformDirection(worldDirection);
            if (localDirection.sqrMagnitude < 0.0001f)
            {
                return;
            }

            localDirection.Normalize();

            if (Vector3.Angle(Vector3.forward, localDirection) <= 0.01f)
            {
                return;
            }

            var aimRotation = Quaternion.FromToRotation(Vector3.forward, localDirection);
            _barrelPivot.localRotation = _barrelPivotLocalRotation * aimRotation;
        }
    }
}
