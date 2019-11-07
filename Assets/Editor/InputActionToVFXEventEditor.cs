using UnityEngine;
using UnityEditor;

namespace Pyro
{
    [CustomEditor(typeof(InputActionToVFXEvent)), CanEditMultipleObjects]
    sealed class InputActionToVFXEventEditor : Editor
    {
        SerializedProperty _target;
        SerializedProperty _eventName;
        SerializedProperty _action;

        void OnEnable()
        {
            _target = serializedObject.FindProperty("_target");
            _eventName = serializedObject.FindProperty("_eventName");
            _action = serializedObject.FindProperty("_action");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_target);
            EditorGUILayout.PropertyField(_eventName);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_action);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Input value (axis, pressure, etc.) will be set as an alpha " +
                "value of the event attribute.",
                MessageType.None
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
