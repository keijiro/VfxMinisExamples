using UnityEngine;
using UnityEditor;

namespace Minis.Utility
{
    [CustomEditor(typeof(VfxMidiNoteAnimator))]
    sealed class VfxMidiNoteAnimatorEditor : Editor
    {
        SerializedProperty _target;
        SerializedProperty _channel;
        SerializedProperty _source;
        SerializedProperty _noteNumber;
        SerializedProperty _lowestNote;
        SerializedProperty _highestNote;
        SerializedProperty _testTrigger;

        static readonly string [] _noteNames =
            { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        static readonly int [] _noteIndices =
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        static readonly string [] _octaveNames =
            { "-1", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        static readonly int [] _octaveIndices =
            { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        static void NoteOctaveSelector(string label, SerializedProperty property)
        {
            var note = property.intValue % 12;
            var octave = property.intValue / 12;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;
            EditorGUILayout.PrefixLabel(label);
            EditorGUI.indentLevel--;
            note = EditorGUILayout.IntPopup(note, _noteNames, _noteIndices);
            octave = EditorGUILayout.IntPopup(octave, _octaveNames, _octaveIndices);
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
                property.intValue = note + octave * 12;
        }

        void OnEnable()
        {
            _target = serializedObject.FindProperty("_target");
            _channel = serializedObject.FindProperty("_channel");
            _source = serializedObject.FindProperty("_source");
            _noteNumber = serializedObject.FindProperty("_noteNumber");
            _lowestNote = serializedObject.FindProperty("_lowestNote");
            _highestNote = serializedObject.FindProperty("_highestNote");
            _testTrigger = serializedObject.FindProperty("_testTrigger");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_target);
            EditorGUILayout.PropertyField(_channel);
            EditorGUILayout.PropertyField(_source);

            var source = (VfxMidiNoteAnimator.Source)_source.enumValueIndex;

            switch (source)
            {
            case VfxMidiNoteAnimator.Source.SingleNote:
                NoteOctaveSelector("Note", _noteNumber);
                break;
            case VfxMidiNoteAnimator.Source.NoteRange:
                NoteOctaveSelector("Lowest Note", _lowestNote);
                NoteOctaveSelector("Highest Note", _highestNote);
                break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_testTrigger);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
