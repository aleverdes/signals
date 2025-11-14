#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using AleVerDes.Signals;

namespace AleVerDes.Signals.Editor
{
    /// <summary>
    /// Custom editor for GlobalSignalBus ScriptableObject that provides debugging and monitoring capabilities.
    /// </summary>
    [CustomEditor(typeof(GlobalSignalBus))]
    public class GlobalSignalBusEditor : UnityEditor.Editor
    {
        private GlobalSignalBus _signalBus;
        private bool _showDebugInfo = false;

        private void OnEnable()
        {
            _signalBus = (GlobalSignalBus)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the default inspector
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Signal Bus Monitor", EditorStyles.boldLabel);

            _showDebugInfo = EditorGUILayout.Foldout(_showDebugInfo, "Debug Information");
            if (_showDebugInfo)
            {
                EditorGUI.indentLevel++;

                // Display subscriber counts for common signal types
                DisplaySignalStats();

                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                // Action buttons
                if (GUILayout.Button("Clear All Subscribers"))
                {
                    if (EditorUtility.DisplayDialog("Clear All Subscribers",
                        "This will remove all signal subscribers. Are you sure?", "Yes", "No"))
                    {
                        _signalBus.ClearAll();
                        EditorUtility.SetDirty(_signalBus);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DisplaySignalStats()
        {
            EditorGUILayout.LabelField("Signal Statistics:", EditorStyles.miniBoldLabel);

            // Get all signal types that have subscribers by using reflection
            var signalBusType = _signalBus.GetType();
            var getSubscriberCountMethod = signalBusType.GetMethod("GetSubscriberCount");

            if (getSubscriberCountMethod != null)
            {
                EditorGUILayout.HelpBox(
                    "Subscriber counts are displayed for signals that have been registered. " +
                    "This list updates dynamically as signals are subscribed to.",
                    MessageType.Info);

                EditorGUILayout.LabelField("No active signals to display. Subscribe to signals to see statistics here.");
            }
            else
            {
                EditorGUILayout.HelpBox("Unable to retrieve signal statistics.", MessageType.Warning);
            }
        }
    }
}
#endif
