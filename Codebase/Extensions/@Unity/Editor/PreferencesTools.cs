using System.Collections;
using System.Reflection;
using UnityEngine;

namespace UnityEditor
{
#if !UNITY_2018_3_OR_NEWER
    public class PreferencesTools
    {
        /// <summary>
        /// Open preferences window and select specific section
        /// </summary>
        /// <param name="sectionName"></param>
        public static void ShowSection(string sectionName)
        {
            const string preferencesType = "UnityEditor.PreferencesWindow";
            const string addCustomSectionsMethodName = "AddCustomSections";
            const string showWindowMethodName = "ShowPreferencesWindow";
            const string sectionsFieldName = "m_Sections";
            const string refreshPreferencesFieldName = "m_RefreshCustomPreferences";
            const string selectedSectionPropertyName = "selectedSectionIndex";
            const string sectionTypeName = "Section";
            const string contentFiledName = "content";

            // find assemble wich contains PreferencesWindow
            var asm = Assembly.GetAssembly(typeof(EditorWindow));
            var prefType = asm.GetType(preferencesType);
            if (prefType == null)
            {
                Debug.LogWarning($"{preferencesType} not found in {asm.FullName}");
                return;
            }
            // find method that runs PreferencesWindow and invoke it
            var showMethod = prefType.GetMethod(showWindowMethodName, BindingFlags.NonPublic | BindingFlags.Static);
            if (showMethod == null)
            {
                Debug.LogWarning($"Methond {showWindowMethodName} not found in {preferencesType}");
                return;
            }
            showMethod.Invoke(null, null);
            var prefEditor = EditorWindow.GetWindow(prefType);
            if (prefEditor == null)
            {
                Debug.LogWarning($"{preferencesType} showed but can't find this window using");
                return;
            }

            // check is custom preferences added
            var refreshField = prefType.GetField(refreshPreferencesFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (refreshField == null)
            {
                Debug.LogWarning($"Field {refreshPreferencesFieldName} not found in {preferencesType}");
                return;
            }
            if ((bool)refreshField.GetValue(prefEditor))
            {
                // find method that runs PreferencesWindow and invoke it
                var refreshMethod = prefType.GetMethod(addCustomSectionsMethodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (refreshMethod == null)
                {
                    Debug.LogWarning($"Methond {addCustomSectionsMethodName} not found in {preferencesType}");
                    return;
                }
                refreshMethod.Invoke(prefEditor, null);
                refreshField.SetValue(prefEditor, false);
            }

            // find index of Protobuf section
            var sectionType = prefType.GetNestedType(sectionTypeName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (sectionType == null)
            {
                Debug.LogWarning($"{sectionTypeName} not found in {asm.FullName}");
                return;
            }
            var contentField = sectionType.GetField(contentFiledName, BindingFlags.Public | BindingFlags.Instance);
            if (contentField == null)
            {
                Debug.LogWarning($"Field {contentFiledName} not found in {sectionTypeName}");
                return;
            }
            var sectionsField = prefType.GetField(sectionsFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (sectionsField == null)
            {
                Debug.LogWarning($"Field {sectionsFieldName} not found in {preferencesType}");
                return;
            }
            var sections = sectionsField.GetValue(prefEditor) as IEnumerable;
            if (sections == null)
            {
                Debug.LogWarning($"Field {sectionsFieldName} is not {typeof(IEnumerable).Name}");
                return;
            }
            int sectionIndex = 0;
            bool found = false;
            foreach (var section in sections)
            {
                GUIContent content = (GUIContent)contentField.GetValue(section);
                if (content.text == sectionName)
                {
                    found = true;
                    break;
                }
                sectionIndex++;
            }
            if (!found)
            {
                Debug.LogWarning($"Section {sectionName} not found in {preferencesType}");
                return;
            }

            // select protobuf section
            var selectedProp = prefType.GetProperty(selectedSectionPropertyName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (selectedProp == null)
            {
                Debug.LogWarning($"Property {selectedSectionPropertyName} not found in {preferencesType}");
                return;
            }
            selectedProp.SetValue(prefEditor, sectionIndex);
        }
    }
#endif
}