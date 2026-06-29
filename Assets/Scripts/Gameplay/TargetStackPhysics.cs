using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private float _mass = 1f;
        [SerializeField] private float _drag = 0.2f;
        [SerializeField] private float _angularDrag = 0.2f;
        [SerializeField] private PhysicsMaterial _contactMaterial;

        private void Start()
        {
            ConfigureStackPieces();
            StartCoroutine(ActivateStackAfterLayout());
        }

        private void ConfigureStackPieces()
        {
            var targets = GetComponentsInChildren<LevelTarget>(true);
            for (var i = 0; i < targets.Length; i++)
            {
                var targetObject = targets[i].gameObject;
                var stackPiece = targetObject.GetComponent<StackPiece>();
                if (stackPiece == null)
                {
                    stackPiece = targetObject.AddComponent<StackPiece>();
                }

                stackPiece.Configure(_mass, _drag, _angularDrag, _contactMaterial, startKinematic: true);
            }
        }

        private IEnumerator ActivateStackAfterLayout()
        {
            yield return new WaitForFixedUpdate();
            UnityEngine.Physics.SyncTransforms();

            var pieces = new List<StackPiece>(GetComponentsInChildren<StackPiece>(true));
            pieces.Sort((left, right) => left.transform.position.y.CompareTo(right.transform.position.y));

            for (var i = 0; i < pieces.Count; i++)
            {
                pieces[i].SetDynamic();
            }
        }
    }
}
