using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JonasWischeropp.Unity.EditorTools.Hierarchy {
    [FilePath("Hierarchy/ActiveToggle/Settings.asset", FilePathAttribute.Location.PreferencesFolder)]
    internal class ActiveToggleSettings : ScriptableSingleton<ActiveToggleSettings> {
        [field: SerializeField]
        public bool Enabled {get; private set;} = true;
        
        [field: SerializeField]
        public ToggleAlignment Alignment {get; private set;} = ToggleAlignment.Left;

        [field: SerializeField]
        public bool MiddleClickToggle {get; private set;} = true;

        [SettingsProvider]
        public static SettingsProvider CreateHierarchyActiveToggleSettingsProvider() {
            var keywords = new string[]{"Hierarchy", "Alignment"};
            return new SettingsProvider("Preferences/Hierarchy/Active Toggle", SettingsScope.User, keywords) {
                guiHandler = (searchContext) => {
                    EditorGUI.BeginChangeCheck();

                    instance.Enabled = EditorGUILayout.Toggle(
                        new GUIContent("Active", "Enable/Disable the package"), instance.Enabled);
                    instance.Alignment = (ToggleAlignment)EditorGUILayout.EnumPopup(
                        new GUIContent("Alignment", "Change alignment"), instance.Alignment);
                    instance.MiddleClickToggle = EditorGUILayout.Toggle(
                        new GUIContent("Middle Click Toggle",
                            "Enable/Disable the ability to enable/disable gameobjects by clicking on them in the hierarchy"),
                            instance.MiddleClickToggle);
                    
                    if (EditorGUI.EndChangeCheck())
                        instance.Save(true);
                }
            };
        }
    }

    internal enum ToggleAlignment {
        Left,
        Right
    }
}
