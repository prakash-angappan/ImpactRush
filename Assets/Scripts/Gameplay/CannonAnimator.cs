using System;
using System.Collections;
using ImpactRush.Gameplay.Data;
using ImpactRush.Utilities;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Legacy cannon rotation, recoil, and fire animation (FEATURE-054–057).
    /// Disabled while <see cref="CannonAimController"/> is active (FEATURE-060).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CannonAnimator : MonoBehaviour
    {
        private static readonly Vector3 DefaultBarrelLocalPosition = new Vector3(0f, 0.5f, 0.65f);

        [Header("References")]
        [SerializeField] private Transform _basePivot;
        [SerializeField] private Transform _barrelPivot;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _recoilTransform;

        [Header("Rotation Limits")]
        [SerializeField] private float _minYawDegrees = -90f;
        [SerializeField] private float _maxYawDegrees = 90f;
        [SerializeField] private float _minPitchDegrees = -45f;
        [SerializeField] private float _maxPitchDegrees = 75f;

        [Header("Debug")]
        [SerializeField] private bool _drawAimDebug = true;

        private Quaternion _basePivotLocalRotation;
        private Quaternion _barrelPivotLocalRotation;
        private Vector3 _recoilLocalRestPosition;
        private Quaternion _targetBaseLocalRotation;
        private Quaternion _targetBarrelLocalRotation;
        private Vector3 _debugTargetPosition;
        private Vector3 _aimTargetWorldPosition;
        private bool _hasAimTarget;
        private bool _hasDebugTarget;
        private float _debugTargetYaw;
        private float _debugTargetPitch;
        private float _debugCurrentYaw;
        private float _debugCurrentPitch;
        private Coroutine _returnRoutine;

        public float DebugTargetYaw => _debugTargetYaw;
        public float DebugTargetPitch => _debugTargetPitch;
        public float DebugCurrentYaw => _debugCurrentYaw;
        public float DebugCurrentPitch => _debugCurrentPitch;
        public bool HasDebugTarget => _hasDebugTarget;
        public Vector3 DebugTargetPosition => _debugTargetPosition;

        private void Reset()
        {
            _basePivot = transform.Find("BasePivot");
            _barrelPivot = transform.Find("BasePivot/BarrelPivot");
            _spawnPoint = transform.Find("BasePivot/BarrelPivot/Barrel/SpawnPoint");
            _recoilTransform = transform.Find("BasePivot/BarrelPivot/Barrel");
        }

        private void Awake()
        {
            Guard.AgainstNull(_basePivot, nameof(_basePivot));
            Guard.AgainstNull(_barrelPivot, nameof(_barrelPivot));
            Guard.AgainstNull(_spawnPoint, nameof(_spawnPoint));

            if (_recoilTransform == null)
            {
                _recoilTransform = _spawnPoint.parent;
            }

            ApplyPolishLimits();
            _basePivot.localRotation = Quaternion.identity;
            _barrelPivot.localRotation = Quaternion.identity;
            EnsureBarrelRestPose();
            CacheRestPose();
        }

        private void EnsureBarrelRestPose()
        {
            if (_recoilTransform == null)
            {
                return;
            }

            if ((_recoilTransform.localPosition - DefaultBarrelLocalPosition).sqrMagnitude > 0.02f)
            {
                _recoilTransform.localPosition = DefaultBarrelLocalPosition;
            }
        }

        private void OnDisable()
        {
            ResetRecoilPosition();
        }

        private void CacheRestPose()
        {
            _basePivotLocalRotation = _basePivot.localRotation;
            _barrelPivotLocalRotation = _barrelPivot.localRotation;
            _recoilLocalRestPosition = _recoilTransform != null
                ? _recoilTransform.localPosition
                : Vector3.zero;
            _targetBaseLocalRotation = _basePivotLocalRotation;
            _targetBarrelLocalRotation = _barrelPivotLocalRotation;
        }

        public void RestoreRestPose()
        {
            ResetRecoilPosition();
            _basePivot.localRotation = _basePivotLocalRotation;
            _barrelPivot.localRotation = _barrelPivotLocalRotation;
            _hasAimTarget = false;
            UpdateDebugAngles();
        }

        private void ResetRecoilPosition()
        {
            if (_recoilTransform == null)
            {
                return;
            }

            _recoilTransform.localPosition = _recoilLocalRestPosition;
        }

        private void Update()
        {
            if (!_hasAimTarget)
            {
                UpdateDebugAngles();
                return;
            }

            var blend = 1f - Mathf.Exp(-ResolveRotationSpeed() * Time.deltaTime);
            _basePivot.localRotation = Quaternion.Slerp(_basePivot.localRotation, _targetBaseLocalRotation, blend);
            _barrelPivot.localRotation = Quaternion.Slerp(_barrelPivot.localRotation, _targetBarrelLocalRotation, blend);
            UpdateDebugAngles();
        }

        public void AimAtTarget(Vector3 targetWorldPosition)
        {
            _aimTargetWorldPosition = targetWorldPosition;
            _debugTargetPosition = targetWorldPosition;
            _hasDebugTarget = true;
            ComputeTargetRotations(
                targetWorldPosition,
                out _targetBaseLocalRotation,
                out _targetBarrelLocalRotation,
                out _debugTargetYaw,
                out _debugTargetPitch);
            _hasAimTarget = true;
        }

        public void AimAtIdleTarget(Vector3 targetWorldPosition)
        {
            AimAtTarget(targetWorldPosition);
            SnapAimToTarget();
        }

        public void SnapAimToTarget()
        {
            if (!_hasAimTarget)
            {
                return;
            }

            ResetRecoilPosition();
            _basePivot.localRotation = _targetBaseLocalRotation;
            _barrelPivot.localRotation = _targetBarrelLocalRotation;
            UpdateDebugAngles();
        }

        public float GetCurrentAimErrorDegrees()
        {
            if (!_hasAimTarget || _spawnPoint == null)
            {
                return 0f;
            }

            var spawnPosition = _spawnPoint.position;
            var desiredDirection = CannonAiming.ComputeLaunchDirection(
                _aimTargetWorldPosition,
                spawnPosition);
            return Vector3.Angle(_spawnPoint.forward, desiredDirection);
        }

        public IEnumerator PlayResponsiveFireSequence(Action onFire)
        {
            ResetRecoilPosition();
            SnapAimToTarget();

            if (_recoilTransform == null)
            {
                onFire?.Invoke();
                yield break;
            }

            var timings = ResolveFireTimings();
            var elapsed = 0f;

            while (elapsed < timings.MaxPreFireWait)
            {
                if (GetCurrentAimErrorDegrees() <= timings.PullBackStartAngle)
                {
                    break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            SnapAimToTarget();

            if (_returnRoutine != null)
            {
                StopCoroutine(_returnRoutine);
                _returnRoutine = null;
            }

            var pullElapsed = 0f;
            var fired = false;

            while (pullElapsed < timings.PullBackDuration)
            {
                pullElapsed += Time.deltaTime;
                var t = Mathf.Clamp01(pullElapsed / timings.PullBackDuration);
                _recoilTransform.localPosition = ComputeRecoilLocalPosition(t);

                if (!fired && pullElapsed >= timings.ProjectileSpawnDelay)
                {
                    SnapAimToTarget();
                    onFire?.Invoke();
                    fired = true;
                }

                yield return null;
            }

            if (!fired)
            {
                SnapAimToTarget();
                onFire?.Invoke();
            }

            var pulledBack = ComputeRecoilLocalPosition(1f);
            _recoilTransform.localPosition = pulledBack;
            _returnRoutine = StartCoroutine(AnimateReturn(pulledBack, timings.ReturnDuration));
        }

        private Vector3 ComputeRecoilLocalPosition(float normalizedPull)
        {
            if (_recoilTransform == null || _recoilTransform.parent == null || _spawnPoint == null)
            {
                return _recoilLocalRestPosition;
            }

            var distance = ResolveFireTimings().RecoilDistance * Mathf.Clamp01(normalizedPull);
            var boreBack = _recoilTransform.parent.InverseTransformDirection(-_spawnPoint.forward).normalized;
            return _recoilLocalRestPosition + boreBack * distance;
        }

        private IEnumerator AnimateReturn(Vector3 pulledBack, float returnDuration)
        {
            var elapsed = 0f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / returnDuration);
                _recoilTransform.localPosition = Vector3.Lerp(pulledBack, _recoilLocalRestPosition, t);
                yield return null;
            }

            ResetRecoilPosition();
            _returnRoutine = null;
        }

        private void ComputeTargetRotations(
            Vector3 targetWorldPosition,
            out Quaternion baseLocalRotation,
            out Quaternion barrelLocalRotation,
            out float yawDegrees,
            out float pitchDegrees)
        {
            CannonAimSolver.Solve(
                _basePivot,
                _barrelPivot,
                _spawnPoint,
                _recoilTransform,
                _recoilLocalRestPosition,
                _basePivotLocalRotation,
                _barrelPivotLocalRotation,
                targetWorldPosition,
                _minYawDegrees,
                _maxYawDegrees,
                _minPitchDegrees,
                _maxPitchDegrees,
                out baseLocalRotation,
                out barrelLocalRotation,
                out yawDegrees,
                out pitchDegrees);
        }

        private void UpdateDebugAngles()
        {
            var baseOffset = Quaternion.Inverse(_basePivotLocalRotation) * _basePivot.localRotation;
            _debugCurrentYaw = NormalizeSignedAngle(baseOffset.eulerAngles.y);

            var barrelOffset = Quaternion.Inverse(_barrelPivotLocalRotation) * _barrelPivot.localRotation;
            _debugCurrentPitch = NormalizeSignedAngle(-barrelOffset.eulerAngles.x);
        }

        private static float NormalizeSignedAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f)
            {
                angle -= 360f;
            }

            return angle;
        }

        private void ApplyPolishLimits()
        {
            var polish = GameplayPolishSettings.Instance;
            if (polish == null)
            {
                return;
            }

            _minYawDegrees = polish.MinYawDegrees;
            _maxYawDegrees = polish.MaxYawDegrees;
            _maxPitchDegrees = polish.MaxPitchDegrees;
        }

        private static float ResolveRotationSpeed()
        {
            var physics = GameplayConfigProvider.ProjectilePhysics;
            if (physics != null)
            {
                return physics.RotationSpeed;
            }

            return GameplayPolishSettings.Instance != null
                ? GameplayPolishSettings.Instance.RotationSmoothness
                : 42f;
        }

        private static CannonFireTimings ResolveFireTimings()
        {
            var physics = GameplayConfigProvider.ProjectilePhysics;
            if (physics != null)
            {
                return new CannonFireTimings(
                    physics.MaxPreFireWaitSeconds,
                    physics.PullBackStartAngleDegrees,
                    physics.PullBackDuration,
                    physics.ProjectileSpawnDelay,
                    physics.ReturnDuration,
                    physics.RecoilDistance);
            }

            var polish = GameplayPolishSettings.Instance;
            return new CannonFireTimings(
                0.1f,
                18f,
                polish != null ? polish.RecoilDuration : 0.1f,
                0.04f,
                polish != null ? polish.ReturnDuration : 0.12f,
                polish != null ? polish.RecoilDistance : 0.18f);
        }

        private readonly struct CannonFireTimings
        {
            public CannonFireTimings(
                float maxPreFireWait,
                float pullBackStartAngle,
                float pullBackDuration,
                float projectileSpawnDelay,
                float returnDuration,
                float recoilDistance)
            {
                MaxPreFireWait = maxPreFireWait;
                PullBackStartAngle = pullBackStartAngle;
                PullBackDuration = pullBackDuration;
                ProjectileSpawnDelay = projectileSpawnDelay;
                ReturnDuration = returnDuration;
                RecoilDistance = recoilDistance;
            }

            public float MaxPreFireWait { get; }
            public float PullBackStartAngle { get; }
            public float PullBackDuration { get; }
            public float ProjectileSpawnDelay { get; }
            public float ReturnDuration { get; }
            public float RecoilDistance { get; }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_maxYawDegrees < _minYawDegrees)
            {
                _maxYawDegrees = _minYawDegrees;
            }

            if (_maxPitchDegrees < _minPitchDegrees)
            {
                _maxPitchDegrees = _minPitchDegrees;
            }

            if (_recoilTransform != null && !Application.isPlaying)
            {
                if ((_recoilTransform.localPosition - DefaultBarrelLocalPosition).sqrMagnitude > 0.02f)
                {
                    _recoilTransform.localPosition = DefaultBarrelLocalPosition;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!ShouldDrawAimDebug(_drawAimDebug) || _spawnPoint == null)
            {
                return;
            }

            var spawnPosition = _spawnPoint.position;
            var barrelForward = _spawnPoint.forward;

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(spawnPosition, spawnPosition + barrelForward * 2f);

            if (!_hasDebugTarget)
            {
                UnityEditor.Handles.color = Color.white;
                UnityEditor.Handles.Label(
                    spawnPosition + Vector3.up * 0.35f,
                    $"BarrelForward={barrelForward}\nAimError={GetCurrentAimErrorDegrees():F1}°");
                return;
            }

            var launchDirection = CannonAiming.ComputeLaunchDirection(_debugTargetPosition, spawnPosition);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_debugTargetPosition, 0.12f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(spawnPosition, _debugTargetPosition);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(spawnPosition, spawnPosition + launchDirection * 2f);

            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(
                spawnPosition + Vector3.up * 0.45f,
                $"Target={_debugTargetPosition}\nLaunchDirection={launchDirection}\n"
                + $"BarrelForward={barrelForward}\nAimError={GetCurrentAimErrorDegrees():F1}°");
        }

        private static bool ShouldDrawAimDebug(bool drawAimDebug)
        {
            if (!drawAimDebug)
            {
                return false;
            }

            var debug = Impacts.GameplayDebugSettings.Instance;
            return debug == null || debug.EnableGameplayDebugMode;
        }
#endif
    }
}