using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace JonasWischeropp.Unity.EditorTools.Hierarchy {
    [InitializeOnLoad]
    internal class ActiveToggle {
        private static readonly Color prefabModifiedColor = new Color(0.05882353f, 0.5058824f, 0.7450981f); // 8AD9FF
        private static readonly Color backgroundColor = new Color(0.2196078f, 0.2196078f, 0.2196078f); // 383838

        static Type hierarchyWindowType = Type.GetType("UnityEditor.SceneHierarchyWindow, UnityEditor");
        static PropertyInfo searchFilterProperty = hierarchyWindowType.GetProperty("searchText");

        static ActiveToggle() {
            EditorApplication.hierarchyWindowItemOnGUI += Draw;
        }

        private static void Draw(int instanceID, Rect selectionRect) {
            if (!ActiveToggleSettings.instance.Enabled)
                return;

            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null)
                return;

            var window = Resources.FindObjectsOfTypeAll(hierarchyWindowType)[0];
            string searchFilter = searchFilterProperty.GetValue(window) as string;
            bool searching = searchFilter.Length > 0;

            int parentCount;
            if (searching)
                parentCount = -1;
            else if (ActiveToggleSettings.instance.Alignment == ToggleAlignment.Left)
                parentCount = CountParents(go.transform);
            else
                parentCount = 0;

            float xPosition = CalculateRectXValue(selectionRect, parentCount + 1);

            // Redraw the line indicating a prefab that was modified because it is under the toggle box now
            if (PrefabUtility.IsPartOfAnyPrefab(go)
                && PrefabUtility.GetOutermostPrefabInstanceRoot(go) == go
                && PrefabUtility.HasPrefabInstanceAnyOverrides(go, false)) {
                bool selected = Selection.objects.Contains(go);
                Color color = selected ? Color.white : prefabModifiedColor;
                Rect prefabLineRect = new Rect(32, selectionRect.y + 1, 2, selectionRect.height - 2);
                // Hide old line
                EditorGUI.DrawRect(prefabLineRect, backgroundColor);
                prefabLineRect.x = CalculateRectXValue(selectionRect, parentCount);
                // Draw new line
                EditorGUI.DrawRect(prefabLineRect, color);
            }

            // Gray out when not active
            Color oldColor = GUI.color;
            if (!go.activeInHierarchy)
                GUI.color = new Color(1,1,1,0.5f);

            Rect rect = new Rect(xPosition, selectionRect.y, selectionRect.height, selectionRect.height);
            if (go.activeSelf != EditorGUI.Toggle(rect, go.activeSelf)) {
                ToggleGameObjectActiveState(go);
            }

            GUI.color = oldColor;

            if (ActiveToggleSettings.instance.MiddleClickToggle)
                HandleMiddleMouseClick(selectionRect, go);
        }

        private static void HandleMiddleMouseClick(Rect selectionRect, GameObject gameObject) {
            bool middleMouseButtonPressed = Event.current.button == 2
                && Event.current.type == EventType.MouseDown;
            if (selectionRect.Contains(Event.current.mousePosition)  && middleMouseButtonPressed) {
                ToggleGameObjectActiveState(gameObject);
            }
        }

        private static void ToggleGameObjectActiveState(GameObject gameObject) {
            Undo.RecordObject(gameObject, gameObject.activeSelf ? "Disable Object" : "Enable Object");
            gameObject.SetActive(!gameObject.activeSelf);
        }

        // Incase that this leads to performance issues:
        // It would be possible to cache the last object for each level
        // and then look up the parent to get its level.
        private static int CountParents(Transform transform) {
            if (transform == null)
                return 0;
            transform = transform.parent;
            int counter = 0;
            while (transform != null) {
                counter++;
                transform = transform.parent;
            }
            return counter;
        }

        // This function is a modified version of the one found here:
        // https://github.com/dyrdadev/hierarchy-tree-for-unity/blob/main/Editor/HierarchyTree.cs
        private static float CalculateRectXValue(Rect rect, int graphDistance) {
            return rect.x - 14.0f - graphDistance * (rect.height - 2);
        }
    }
}
