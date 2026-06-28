using ImpactRush.Gameplay;
using UnityEditor;
using UnityEngine;

namespace ImpactRush.Editor
{
    [CustomEditor(typeof(ProjectileLauncher))]
    public sealed class ProjectileLauncherEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            AssignMissingReferences((ProjectileLauncher)target);
        }

        public override void OnInspectorGUI()
        {
            AssignMissingReferences((ProjectileLauncher)target);
            DrawDefaultInspector();
        }

        private static void AssignMissingReferences(ProjectileLauncher launcher)
        {
            if (launcher == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(launcher);
            var prefabProperty = serializedObject.FindProperty("_projectilePrefab");
            var spawnPointProperty = serializedObject.FindProperty("_spawnPoint");
            var changed = false;

            var projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabUtility.PrefabPath);
            if (projectilePrefab == null)
            {
                projectilePrefab = ProjectilePrefabUtility.EnsureProjectilePrefab();
            }

            if (prefabProperty.objectReferenceValue != projectilePrefab)
            {
                prefabProperty.objectReferenceValue = projectilePrefab;
                changed = true;
            }

            if (spawnPointProperty.objectReferenceValue == null)
            {
                var spawnPoint = launcher.transform.Find("BasePivot/BarrelPivot/SpawnPoint");
                if (spawnPoint == null)
                {
                    foreach (var child in launcher.GetComponentsInChildren<Transform>(true))
                    {
                        if (child.name is "SpawnPoint" or "ProjectileSpawnPoint")
                        {
                            spawnPoint = child;
                            break;
                        }
                    }
                }

                if (spawnPoint != null)
                {
                    spawnPointProperty.objectReferenceValue = spawnPoint;
                    changed = true;
                }
            }

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
