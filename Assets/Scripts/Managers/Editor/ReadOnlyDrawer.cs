using UnityEngine;
using UnityEditor;

namespace DuckHunt.Managers
{
    #if UNITY_EDITOR
    /// <summary>
    /// Property drawer for ReadOnly attribute to display fields as read-only in inspector
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Store the original GUI enabled state
            bool previousGUIState = GUI.enabled;
            
            // Disable GUI to make it read-only
            GUI.enabled = false;
            
            // Draw the property field
            EditorGUI.PropertyField(position, property, label);
            
            // Restore the original GUI enabled state
            GUI.enabled = previousGUIState;
        }
    }
    #endif
}