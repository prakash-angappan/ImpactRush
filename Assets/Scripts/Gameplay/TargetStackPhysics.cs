using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Applies stack box physics to all BoxCollider children under this TargetStack root.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-60)]
    public sealed class TargetStackPhysics : MonoBehaviour
    {
        [SerializeField] private float _mass = 1f;
        [SerializeField] private float _drag = 0.15f;
        [SerializeField] private float _angularDrag = 0.05f;
        [SerializeField] private PhysicsMaterial _contactMaterial;

        private void Awake()
        {
            var colliders = GetComponentsInChildren<BoxCollider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                var boxObject = colliders[i].gameObject;
                if (boxObject == gameObject)
                {
                    continue;
                }

                var stackBox = boxObject.GetComponent<StackBox>();
                if (stackBox == null)
                {
                    stackBox = boxObject.AddComponent<StackBox>();
                }

                stackBox.Configure(_mass, _drag, _angularDrag, _contactMaterial);
            }
        }
    }
}
