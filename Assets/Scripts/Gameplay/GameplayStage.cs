using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Positions the aim plane behind the platform to match GameplayRectangle dimensions.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-50)]
    public sealed class GameplayStage : MonoBehaviour
    {
        private const float AimPlaneDepth = 0.08f;
        private const float AimPlaneZOffset = 0.12f;

        [Header("References")]
        [SerializeField] private GameplayRectangle _gameplayRectangle;
        [SerializeField] private AimPlane _aimPlane;
        [SerializeField] private Transform _platform;

        private void Awake()
        {
            ApplyLayout();
        }

        private void ApplyLayout()
        {
            if (_gameplayRectangle == null || _aimPlane == null)
            {
                return;
            }

            var center = _gameplayRectangle.Center;
            var aimPosition = center + Vector3.forward * AimPlaneZOffset;
            _aimPlane.transform.SetPositionAndRotation(aimPosition, _gameplayRectangle.transform.rotation);
            _aimPlane.ConfigureSize(
                Vector3.zero,
                new Vector3(_gameplayRectangle.Width, _gameplayRectangle.Height, AimPlaneDepth));
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                ApplyLayout();
            }
        }

        private void Reset()
        {
            _gameplayRectangle = GetComponentInChildren<GameplayRectangle>();
            _aimPlane = GetComponentInChildren<AimPlane>();
            _platform = transform.Find("Platform");
        }
#endif
    }
}
