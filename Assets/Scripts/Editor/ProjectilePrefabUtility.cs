using UnityEditor;
using UnityEngine;

namespace ImpactRush.Editor
{
    /// <summary>
    /// Creates the default projectile prefab when missing.
    /// </summary>
    public static class ProjectilePrefabUtility
    {
        public const string PrefabPath = "Assets/Prefabs/Projectile.prefab";

        [MenuItem("Impact Rush/Ensure Projectile Prefab")]
        public static GameObject EnsureProjectilePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
            {
                return existing;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Projectile";
            sphere.transform.localScale = Vector3.one * 0.5f;

            var collider = sphere.GetComponent<SphereCollider>();
            collider.isTrigger = false;

            var prefab = PrefabUtility.SaveAsPrefabAsset(sphere, PrefabPath);
            Object.DestroyImmediate(sphere);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created projectile prefab at {PrefabPath}.");
            return prefab;
        }
    }
}
