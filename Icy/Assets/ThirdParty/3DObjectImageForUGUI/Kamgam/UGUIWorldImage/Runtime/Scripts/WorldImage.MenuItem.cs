#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Kamgam.UGUIWorldImage
{
    public partial class WorldImage
    {
        [MenuItem("GameObject/UI/World Image (uGUI)", false, 2009)]
        public static void AddImageToSelection()
        {
            WorldImage lastImage = null;

            // Create one for reach selected game object.
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                var go = new GameObject("World Image", typeof(RectTransform), typeof(CanvasRenderer));
                var image = go.AddComponent<WorldImage>();
                go.transform.SetParent(Selection.gameObjects[i].transform);
                go.transform.localPosition = Vector3.zero;
                var rectTransform = go.transform as RectTransform;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.localScale = Vector3.one;
                rectTransform.sizeDelta = new Vector2(100, 100);

                image.raycastTarget = false;

                lastImage = image;
            }

            if (lastImage != null)
            {
                Selection.objects = new GameObject[] { lastImage.gameObject };
            }
        }

        [MenuItem("GameObject/UI/World Image (uGUI)", true, 2009)]
        public static bool AddImageToSelectionValidation()
        {
            return Selection.count > 0 && Selection.gameObjects[0].GetComponent<RectTransform>() != null;
        }
    }
}
#endif