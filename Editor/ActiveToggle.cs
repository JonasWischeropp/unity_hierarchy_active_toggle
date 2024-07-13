using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JonasWischeropp.Unity.EditorTools.Hierarchy {
    [InitializeOnLoad]
    internal class ActiveToggle {
        private static readonly Color prefabModifiedColor = new Color(0.05882353f, 0.5058824f, 0.7450981f); // 8AD9FF
        
        static ActiveToggle() {
            EditorApplication.hierarchyWindowItemOnGUI += Draw;
        }
        
        private static void Draw(int instanceID, Rect selectionRect) {
            if (!ActiveToggleSettings.instance.Enabled)
                return;

            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null)
                return;

            int parentCount = ActiveToggleSettings.instance.Alignment == ToggleAlignment.Left ? CountParents(go.transform) : 0;
            GUILayout.BeginArea(new Rect(CalculateRectXValue(selectionRect, parentCount + 1),
                selectionRect.y, selectionRect.height, selectionRect.height));

            // Grey out when not active
            Color oldColor = GUI.color;
            if (!go.activeInHierarchy)
                GUI.color = new Color(1,1,1,0.5f);

            if (go.activeSelf != GUILayout.Toggle(go.activeSelf, "")) {
                Undo.RecordObject(go, go.activeSelf ? "Disable Object" : "Enable Object");
                go.SetActive(!go.activeSelf);
            }

            // Redraw the line indicating a prefab that was modified because it is under the toggle box now
            if (PrefabUtility.IsPartOfRegularPrefab(go)
                && PrefabUtility.GetOutermostPrefabInstanceRoot(go) == go
                && PrefabUtility.GetObjectOverrides(go).Count != 0) {
                bool selected = Selection.objects.Contains(go);
                Color color = selected ? Color.white : prefabModifiedColor;
                EditorGUI.DrawRect(new Rect(selectionRect.height - 2, 1, 3, selectionRect.height - 2), color);
            }
            GUI.color = oldColor;
            GUILayout.EndArea();
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