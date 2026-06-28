using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Applies stack box physics to all target cubes in the scene.
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
            var transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < transforms.Length; i++)
            {
                var boxObject = transforms[i].gameObject;
                if (!boxObject.name.StartsWith("Cube_"))
                {
                    continue;
                }

                if (!boxObject.TryGetComponent<BoxCollider>(out _))
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
