using Klak.Math;
using Minis;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public sealed class VfxController : MonoBehaviour
{
    #region Exposed attributes

    [SerializeField] VisualEffect [] _targets = null;

    #endregion

    #region Callback implementation

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        var midiDevice = device as Minis.MidiDevice;
        if (midiDevice == null) return;
        DisconnectAllDevices();
        ConnectAllDevices();
    }

    void OnWillNoteOn(Minis.MidiNoteControl note, float velocity)
      => AllocateKeySlot()?.Trigger(note, velocity);

    void OnWillNoteOff(Minis.MidiNoteControl note)
      => FindKeySlotByNote(note.noteNumber)?.Untrigger();

    #endregion

    #region Device detection

    List<Minis.MidiDevice> _devices = new();

    void ConnectAllDevices()
    {
        foreach (var device in InputSystem.devices)
        {
            var midiDevice = device as Minis.MidiDevice;
            if (midiDevice == null) continue;
            midiDevice.onWillNoteOn += OnWillNoteOn;
            midiDevice.onWillNoteOff += OnWillNoteOff;
            _devices.Add(midiDevice);
        }
    }

    void DisconnectAllDevices()
    {
        foreach (var midiDevice in _devices)
        {
            midiDevice.onWillNoteOn -= OnWillNoteOn;
            midiDevice.onWillNoteOff -= OnWillNoteOff;
        }
        _devices.Clear();
    }

    #endregion

    #region Key slot management

    class KeySlot
    {
        VisualEffect _vfx;
        MidiNoteControl _note;
        float _intensity;

        public bool Available => _note == null;
        public int NoteNumber => _note?.noteNumber ?? -1;

        public KeySlot(VisualEffect vfx) => _vfx = vfx;

        public void Trigger(MidiNoteControl note, float velocity)
        {
            _note = note;
            _vfx.SetUInt("NoteNumber", (uint)NoteNumber);
        }

        public void Untrigger()
          => _note = null;

        public void Update()
        {
            var target = _note?.ReadValue() ?? 0.0f;
            _intensity = ExpTween.Step(_intensity, target, 8);
            _vfx.SetFloat("Intensity", _intensity);
        }
    }

    KeySlot[] _keySlots;

    void InitializeKeySlots()
      => _keySlots = _targets.Select(vfx => new KeySlot(vfx)).ToArray();

    KeySlot AllocateKeySlot()
      => _keySlots.FirstOrDefault(s => s.Available);

    KeySlot FindKeySlotByNote(int noteNumber)
      => _keySlots.FirstOrDefault(s => s.NoteNumber == noteNumber);

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        InitializeKeySlots();
        InputSystem.onDeviceChange += OnDeviceChange;
        ConnectAllDevices();
    }

    void OnDestroy()
    {
        DisconnectAllDevices();
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    void Update()
    {
        foreach (var slot in _keySlots) slot.Update();
    }

    #endregion
}
