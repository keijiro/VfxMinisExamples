using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

sealed public class DeviceStatus : MonoBehaviour
{
    IEnumerable<string> MidiDeviceNames 
      => InputSystem.devices.OfType<Minis.MidiDevice>().
           Select(dev => $"{dev.description.product}");

    void UpdateInfo()
    {
        var devs = MidiDeviceNames;
        var text = devs.Any() ?
          "Detected MIDI devices:\n" + string.Join("\n", devs.ToArray()) :
          "Waiting for MIDI input...";

        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Label>("info").text = text;
    }

    void OnDeviceChange(InputDevice dev, InputDeviceChange change)
      => UpdateInfo();

    void Start()
    {
        UpdateInfo();
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDestroy()
      => InputSystem.onDeviceChange -= OnDeviceChange;
}
