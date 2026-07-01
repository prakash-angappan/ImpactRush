using System.Collections;
using System.Collections.Generic;
using ImpactRush.Gameplay.Data;
using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Applies stack piece physics after the procedural layout has been built.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-60)]
    public sealed class TargetStackPhysics : MonoBehaviour
    {
        [SerializeField] private Transform _platform;
        [SerializeField] private float _mass = 1f;
        [SerializeField] private float _drag = 0.2f;
        [SerializeField] private float _angularDrag = 0.2f;
        [SerializeField] private PhysicsMaterial _contactMaterial;
        [SerializeField] private bool _wakeOnFirstShot = true;

        private bool _isAwake;
        private Coroutine _wakeRoutine;

        public void OnLevelBuilt()
        {
            ResolvePlatformReference();
            ConfigureStackPieces();
            AlignStackToPlatform();
            UnityEngine.Physics.SyncTransforms();

            if (!_wakeOnFirstShot)
            {
                WakeStack();
            }
        }

        /// <summary>
        /// Makes every stack piece dynamic so the full structure can react to physics together.
        /// </summary>
        public static void ActivateAllForGameplay()
        {
            var instance = FindFirstObjectByType<TargetStackPhysics>();
            instance?.ActivateAllImmediate();
        }

        public void ActivateAllImmediate()
        {
            _isAwake = true;
            if (_wakeRoutine != null)
            {
                StopCoroutine(_wakeRoutine);
                _wakeRoutine = null;
            }

            var pieces = GetComponentsInChildren<StackPiece>(true);
            for (var i = 0; i < pieces.Length; i++)
            {
                pieces[i].SetDynamic();
            }

            UnityEngine.Physics.SyncTransforms();
        }

        public void WakeStack()
        {
            if (_isAwake)
            {
                return;
            }

            _isAwake = true;
            if (_wakeRoutine != null)
            {
                StopCoroutine(_wakeRoutine);
            }

            _wakeRoutine = StartCoroutine(WakeStackRoutine());
        }

        private void ConfigureStackPieces()
        {
            var config = GameplayConfigProvider.Active;
            var cleanupDelay = config != null ? config.CubeCleanupDelay : 3.5f;
            var autoDestroyHeight = config != null
                ? config.CubeAutoDestroyHeight
                : -2.5f;

            var targets = GetComponentsInChildren<LevelTarget>(true);
            for (var i = 0; i < targets.Length; i++)
            {
                var targetObject = targets[i].gameObject;
                var stackPiece = targetObject.GetComponent<StackPiece>();
                if (stackPiece == null)
                {
                    stackPiece = targetObject.AddComponent<StackPiece>();
                }

                stackPiece.Configure(_mass, _drag, _angularDrag, _contactMaterial);
                stackPiece.SetSimulationRoot(transform);

                if (targetObject.GetComponent<PlatformOccupant>() == null)
                {
                    targetObject.AddComponent<PlatformOccupant>();
                }

                var cleanup = targetObject.GetComponent<FallenStackPieceCleanup>();
                if (cleanup == null)
                {
                    cleanup = targetObject.AddComponent<FallenStackPieceCleanup>();
                }

                cleanup.Configure(cleanupDelay, autoDestroyHeight);
            }
        }

        private IEnumerator WakeStackRoutine()
        {
            yield return new WaitForFixedUpdate();
            UnityEngine.Physics.SyncTransforms();

            var rowIndices = CollectRowIndices();
            for (var i = 0; i < rowIndices.Count; i++)
            {
                ActivateRow(rowIndices[i]);
                yield return new WaitForFixedUpdate();
                UnityEngine.Physics.SyncTransforms();
            }

            ActivateRemainingPieces();
            _wakeRoutine = null;
        }

        private List<int> CollectRowIndices()
        {
            var indices = new List<int>();
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (TryParseRowIndex(child.name, out var rowIndex))
                {
                    indices.Add(rowIndex);
                }
            }

            indices.Sort();
            return indices;
        }

        private void ActivateRow(int rowIndex)
        {
            var row = transform.Find($"Row_{rowIndex}");
            if (row == null)
            {
                return;
            }

            var pieces = row.GetComponentsInChildren<StackPiece>(true);
            for (var i = 0; i < pieces.Length; i++)
            {
                pieces[i].SetDynamic();
            }
        }

        private void ActivateRemainingPieces()
        {
            var pieces = GetComponentsInChildren<StackPiece>(true);
            for (var i = 0; i < pieces.Length; i++)
            {
                pieces[i].SetDynamic();
            }
        }

        private void AlignStackToPlatform()
        {
            if (_platform == null)
            {
                return;
            }

            var platformCollider = _platform.GetComponent<Collider>();
            if (platformCollider == null)
            {
                return;
            }

            var platformTopY = platformCollider.bounds.max.y;
            var lowestBottomY = GetLowestPieceBottomY();
            if (float.IsPositiveInfinity(lowestBottomY))
            {
                return;
            }

            var deltaY = platformTopY - lowestBottomY;
            if (Mathf.Abs(deltaY) <= 0.0001f)
            {
                return;
            }

            transform.position += new Vector3(0f, deltaY, 0f);
            UnityEngine.Physics.SyncTransforms();
        }

        private float GetLowestPieceBottomY()
        {
            var lowestBottomY = float.PositiveInfinity;
            var colliders = GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                if (collider.isTrigger)
                {
                    continue;
                }

                lowestBottomY = Mathf.Min(lowestBottomY, collider.bounds.min.y);
            }

            return lowestBottomY;
        }

        private void ResolvePlatformReference()
        {
            if (_platform != null)
            {
                return;
            }

            if (transform.parent != null)
            {
                _platform = transform.parent.Find("Platform");
            }
        }

        private static bool TryParseRowIndex(string objectName, out int rowIndex)
        {
            rowIndex = -1;
            const string prefix = "Row_";
            if (!objectName.StartsWith(prefix))
            {
                return false;
            }

            return int.TryParse(objectName.Substring(prefix.Length), out rowIndex);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            ResolvePlatformReference();
        }
#endif
    }
}
