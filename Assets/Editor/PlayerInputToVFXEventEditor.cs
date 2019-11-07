using UnityEngine;
using UnityEditor;

namespace Pyro
{
    [CustomEditor(typeof(PlayerInputToVFXEvent)), CanEditMultipleObjects]
    sealed class PlayerInputToVFXEventEditor : Editor
    {
        SerializedProperty _input;
        SerializedProperty _target;

        void OnEnable()
        {
            _input = serializedObject.FindProperty("_input");
            _target = serializedObject.FindProperty("_target");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_input);
            EditorGUILayout.PropertyField(_target);

            EditorGUILayout.HelpBox(
                "This component receives every action from the input object " +
                "then sends an event to the target VFX with adding \"On\" " +
                "to the name of the action (e.g., \"Fire\" action >> " +
                "\"OnFire\" event). The input value (axis, pressure, etc.) " +
                "will be attached to the event as an alpha attribute value.",
                MessageType.None
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}

