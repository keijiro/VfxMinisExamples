using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using System.Collections.Generic;
using System.Linq;

namespace Minis.Utility
{
    //
    // MIDI note animator class that controls visual effect with elapsed time
    // from note-on/off.
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

        public enum Source { AllNotes, NoteNumbers, NoteRange }

        #endregion

        #region Public methods

        // Manual note on/off

        public void NoteOn(int note, float velocity)
        {
            OnNoteOn(note, velocity);
        }

        public void NoteOff(int note)
        {
            OnNoteOff(note);
        }

        #endregion

        #region Editable properties

        // Target VFX
        [SerializeField] VisualEffect [] _targets = null;

        // Note input
        [SerializeField] Channel _channel = Channel.All;
        [SerializeField] Source _source = Source.AllNotes;
        [SerializeField] int [] _noteNumbers = new [] { 60 };
        [SerializeField] int _lowestNote = 0;
        [SerializeField] int _highestNote = 127;

        #endregion

        #region Local members

        // A class used to store a state of a note slot
        class NoteSlot
        {
            public VisualEffect Vfx { get; set; }
            public int Note { get; set; } = -1;
            public float TimeOn { get; set; } = 1e+6f;
            public float TimeOff { get; set; } = 1e+6f;
        }

        NoteSlot [] _slots;
        Queue<NoteSlot> _freeSlotQueue;

        // Check if a note matchs the note options.
        bool NoteFilter(MidiNoteControl note)
        {
            var device = (Minis.MidiDevice)note.device;

            if (_channel != Channel.All && (int)_channel != device.channel)
                return false;

            var number = note.noteNumber;

            switch (_source)
            {
            case Source.NoteNumbers:
                foreach (var n in _noteNumbers) if (n == number) return true;
                return false;
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
            if (NoteFilter(note)) OnNoteOn(note.noteNumber, velocity);
        }

        // Note on callback body
        void OnNoteOn(int note, float velocity)
        {
            if (_freeSlotQueue.Count == 0) return;
            var slot = _freeSlotQueue.Dequeue();

            // Reset the note slot state.
            slot.Note = note;
            slot.TimeOn = slot.TimeOff = 0;

            // Update the vfx properties.
            var vfx = slot.Vfx;
            if (vfx.HasUInt("NoteNumber")) vfx.SetUInt("NoteNumber", (uint)note);
            if (vfx.HasFloat("Velocity")) vfx.SetFloat("Velocity", velocity);

            // VFX note on event
            vfx.SendEvent("OnNoteOn");
        }

        // Note off callback for MIDI device
        void OnMidiNoteOff(MidiNoteControl note)
        {
            if (NoteFilter(note)) OnNoteOff(note.noteNumber);
        }

        // Note off callback body
        void OnNoteOff(int note)
        {
            foreach (var slot in _slots)
            {
                if (slot.Note == note)
                {
                    // VFX note on event
                    slot.Vfx.SendEvent("OnNoteOff");

                    // Release the note slot.
                    slot.Note = -1;
                    _freeSlotQueue.Enqueue(slot);
                    break;
                }
            }
        }

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            // Initialize the note slots.
            _slots = _targets.Select(vfx => new NoteSlot{Vfx = vfx}).ToArray();
            _freeSlotQueue = new Queue<NoteSlot>(_slots);

            // Subscribe the device event.
            EditMidiDeviceSubscription(true);
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        void OnDestroy()
        {
            // Unsubscribe the Input System.
            InputSystem.onDeviceChange -= OnDeviceChange;
            EditMidiDeviceSubscription(false);
        }

        void Update()
        {
            // Update the note slots.
            foreach (var slot in _slots)
            {
                if (slot.Note >= 0)
                    slot.TimeOn += Time.deltaTime;
                else
                    slot.TimeOff += Time.deltaTime;

                var vfx = slot.Vfx;
                if (vfx.HasFloat("NoteOnTime")) vfx.SetFloat("NoteOnTime", slot.TimeOn);
                if (vfx.HasFloat("NoteOffTime")) vfx.SetFloat("NoteOffTime", slot.TimeOff);
            }
        }

        #endregion
    }
}
