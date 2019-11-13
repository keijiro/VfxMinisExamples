using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Minis.Utility
{
    //
    // Custom inspector for VfxMidiNoteAnimator
    //
    [CustomEditor(typeof(VfxMidiNoteAnimator))]
    sealed class VfxMidiNoteAnimatorEditor : Editor
    {
        ReorderableList _targetList;

        SerializedProperty _channel;
        SerializedProperty _source;
        SerializedProperty _noteNumber;
        SerializedProperty _lowestNote;
        SerializedProperty _highestNote;

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
            _targetList = new ReorderableList(
                serializedObject, serializedObject.FindProperty("_targets"),
                true, false, true, true
            );

            _targetList.drawElementCallback = DrawTargetListElement;
            _targetList.headerHeight = 3;

            _channel = serializedObject.FindProperty("_channel");
            _source = serializedObject.FindProperty("_source");
            _noteNumber = serializedObject.FindProperty("_noteNumber");
            _lowestNote = serializedObject.FindProperty("_lowestNote");
            _highestNote = serializedObject.FindProperty("_highestNote");
        }

        void DrawTargetListElement(Rect rect, int index, bool active, bool focused)
        {
            rect.yMin++;
            rect.yMax--;

            EditorGUI.PropertyField(
                rect,
                _targetList.serializedProperty.GetArrayElementAtIndex(index),
                GUIContent.none
            );
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Target Visual Effects");
            _targetList.DoLayoutList();

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

            serializedObject.ApplyModifiedProperties();
        }
    }
}
