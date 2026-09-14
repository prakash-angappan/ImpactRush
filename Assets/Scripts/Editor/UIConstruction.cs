using ImpactRush.Audio;
using ImpactRush.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ImpactRush.Editor
{
    /// <summary>
    /// Builds the persistent UIRoot hierarchy that <c>UIFrameworkBuilder</c> saves as a prefab.
    /// <para>
    /// RECONSTRUCTED FILE: <c>UIFrameworkBuilder.BuildUIRootPrefab</c> calls
    /// <see cref="BuildUIRootHierarchy"/> and then saves the returned <see cref="GameObject"/> as a
    /// prefab, but this helper was never committed to git. It is rebuilt to produce a valid
    /// screen-space Canvas carrying the surviving <see cref="UIRoot"/> component, sized for the
    /// project's 1080x1920 portrait reference. Individual screen/popup wiring can be layered on
    /// top in the editor.
    /// </para>
    /// </summary>
    public static class UIConstruction
    {
        public static GameObject BuildUIRootHierarchy(AudioLibrary library)
        {
            var root = new GameObject("UIRoot", typeof(RectTransform));

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<UIRoot>();

            return root;
        }
    }
}
