using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.VFX;

namespace Minis.Utility
{
    //
    // 
    //
    public sealed class VfxMidiNoteAnimator : MonoBehaviour
    {
        #region Public enum definitions

        public enum Channel
        {
            All = -1,
            Ch1, Ch2, Ch3, Ch4, Ch5, Ch6, Ch7, Ch8,
            Ch9, Ch10, Ch11, Ch12, Ch13, Ch14, Ch15, Ch16
        }

        public enum Source { AllNotes, SingleNote, NoteRange }

        #endregion

        #region Editable properties

        // Target VFX
        [SerializeField] VisualEffect _target = null;

        // Note input
        [SerializeField] Channel _channel = Channel.All;
        [SerializeField] Source _source = Source.AllNotes;
        [SerializeField] int _noteNumber = 60;
        [SerializeField] int _lowestNote = 0;
        [SerializeField] int _highestNote = 127;

#if UNITY_EDITOR
        // Triggers for testing
        [SerializeField] InputAction _testTrigger = null;
#endif

        #endregion

        #region Local members

        bool _isOn = false;
        float _timeOn = 1e+6f;
        float _timeOff = 1e+6f;

        bool NoteFilter(MidiNoteControl note)
        {
            var device = (Minis.MidiDevice)note.device;

            if (_channel != Channel.All && (int)_channel != device.channel)
                return false;

            var number = note.noteNumber;

            switch (_source)
            {
            case Source.SingleNote:
                return number == _noteNumber;
            case Source.NoteRange:
                return _lowestNote <= number && number <= _highestNote;
            default: // Source.AllNotes:
                return true;
            }
        }

        #endregion

        #region Delegate functions

        // Device change callback
        // Subscribe the note on/off events if the device has MIDI capability.
        void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change != InputDeviceChange.Added) return;

            var midi = device as Minis.MidiDevice;
            if (midi == null) return;

            midi.onWillNoteOn += OnMidiNoteOn;
            midi.onWillNoteOff += OnMidiNoteOff;
        }

        // Enumerate all MIDI devices and subscribe/unsubscribe them.
        void EditMidiDeviceSubscription(bool flag)
        {
            foreach (var dev in InputSystem.devices)
            {
                var midi = dev as Minis.MidiDevice;
                if (midi == null) continue;

                if (flag)
                {
                    midi.onWillNoteOn += OnMidiNoteOn;
                    midi.onWillNoteOff += OnMidiNoteOff;
                }
                else
                {
                    midi.onWillNoteOn -= OnMidiNoteOn;
                    midi.onWillNoteOff -= OnMidiNoteOff;
                }
            }
        }

        // Note on callback for MIDI device
        void OnMidiNoteOn(MidiNoteControl note, float velocity)
        {
            if (NoteFilter(note)) OnNoteOn(velocity);
        }

        // Note on callback body
        void OnNoteOn(float velocity)
        {
            _isOn = true;
            _timeOn = _timeOff = 0;

            // Update the velocity property.
            if (_target != null && _target.HasFloat("Velocity"))
                _target.SetFloat("Velocity", velocity);
        }

        // Note off callback for MIDI device
        void OnMidiNoteOff(MidiNoteControl note)
        {
            if (NoteFilter(note)) OnNoteOff();
        }

        // Note off callback body
        void OnNoteOff()
        {
            _isOn = false;
        }

#if UNITY_EDITOR
        // Test trigger callback
        void OnTestTrigger(InputAction.CallbackContext context)
        {
            var v = context.ReadValue<float>();
            if (v > 0) OnNoteOn(v); else OnNoteOff();
        }
#endif

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            // Subscribe the device event.
            EditMidiDeviceSubscription(true);
            InputSystem.onDeviceChange += OnDeviceChange;

#if UNITY_EDITOR
            // Activate the test trigger callbacks.
            _testTrigger.started += OnTestTrigger;
            _testTrigger.canceled += OnTestTrigger;
            _testTrigger.Enable();
#endif
        }

        void OnDestroy()
        {
            // Unsubscribe the Input System.
            InputSystem.onDeviceChange -= OnDeviceChange;
            EditMidiDeviceSubscription(false);

#if UNITY_EDITOR
            // Deactivate the test trigger callbacks.
            _testTrigger.started -= OnTestTrigger;
            _testTrigger.canceled -= OnTestTrigger;
            _testTrigger.Disable();
#endif
        }

        void Update()
        {
            // Time accumulation
            if (_isOn)
                _timeOn += Time.deltaTime;
            else
                _timeOff += Time.deltaTime;

            // Property update
            if (_target != null && _target.HasFloat("NoteOnTime"))
                _target.SetFloat("NoteOnTime", _timeOn);
            if (_target != null && _target.HasFloat("NoteOffTime"))
                _target.SetFloat("NoteOffTime", _timeOff);
        }

        #endregion
    }
}
