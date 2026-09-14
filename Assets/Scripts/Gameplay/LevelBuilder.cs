using UnityEngine;

namespace ImpactRush.Gameplay
{
    /// <summary>
    /// Procedurally generates the target stack: rows of boxes, each carrying a
    /// <see cref="LevelTarget"/> marker and a <see cref="StackPiece"/> physics driver.
    /// Row objects are named <c>Row_{index}</c> so <see cref="TargetStackPhysics"/> can wake
    /// them row-by-row.
    /// <para>
    /// RECONSTRUCTED FILE: the editor scene builder (<c>UIFrameworkBuilder</c>) added this
    /// component and wired its serialized <c>_targetStack</c> / <c>_targetMaterials</c> fields,
    /// but the class itself was never committed to git. The serialized field names/types match
    /// those call sites exactly; the generation logic is a best-effort reconstruction and can be
    /// tuned in the Unity editor. It is deliberately self-contained (no dependency on the other
    /// never-committed types) so the project compiles and produces a playable stack.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-70)]
    public sealed class LevelBuilder : MonoBehaviour
    {
        [SerializeField] private Transform _targetStack;
        [SerializeField] private Material[] _targetMaterials = System.Array.Empty<Material>();
        [SerializeField] private int _rows = 4;
        [SerializeField] private int _columns = 3;
        [SerializeField] private Vector3 _pieceSize = new Vector3(0.6f, 0.6f, 0.6f);
        [SerializeField] private float _spacing = 0.05f;
        [SerializeField] private bool _buildOnStart = true;

        private void Start()
        {
            if (_buildOnStart)
            {
                Build();
            }
        }

        /// <summary>
        /// Rebuilds the target stack under <c>_targetStack</c> (or this transform if unset).
        /// </summary>
        public void Build()
        {
            var root = _targetStack != null ? _targetStack : transform;
            ClearExisting(root);

            var stepX = _pieceSize.x + _spacing;
            var stepY = _pieceSize.y + _spacing;
            var originX = -(_columns - 1) * 0.5f * stepX;

            for (var row = 0; row < _rows; row++)
            {
                var rowObject = new GameObject($"Row_{row}").transform;
                rowObject.SetParent(root, false);
                rowObject.localPosition = new Vector3(0f, row * stepY, 0f);

                for (var col = 0; col < _columns; col++)
                {
                    CreatePiece(rowObject, new Vector3(originX + col * stepX, 0f, 0f), (row * _columns) + col);
                }
            }

            var stackPhysics = root.GetComponent<TargetStackPhysics>();
            if (stackPhysics != null)
            {
                stackPhysics.OnLevelBuilt();
            }
        }

        private void CreatePiece(Transform parent, Vector3 localPosition, int index)
        {
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = $"Target_{index}";
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localScale = _pieceSize;

            piece.AddComponent<LevelTarget>();
            piece.AddComponent<StackPiece>();

            if (_targetMaterials != null && _targetMaterials.Length > 0)
            {
                var pieceRenderer = piece.GetComponent<Renderer>();
                var material = _targetMaterials[index % _targetMaterials.Length];
                if (pieceRenderer != null && material != null)
                {
                    pieceRenderer.sharedMaterial = material;
                }
            }
        }

        private static void ClearExisting(Transform root)
        {
            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (!child.name.StartsWith("Row_") && child.GetComponent<LevelTarget>() == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}
