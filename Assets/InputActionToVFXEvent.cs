using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.InputSystem;

namespace Pyro
{
    public sealed class InputActionToVFXEvent : MonoBehaviour
    {
        [SerializeField] VisualEffect _target = null;
        [SerializeField] string _eventName = null;
        [SerializeField] InputAction _action = null;

        VFXEventAttribute _attrib;
        int _eventID;

        static class IDs
        {
            public readonly static int alpha = Shader.PropertyToID("alpha");
        }

        void OnEnable()
        {
            if (_action != null)
            {
                _action.performed += OnPerformed;
                _action.Enable();
            }

            _eventID = Shader.PropertyToID(_eventName);
        }

        void OnDisable()
        {
            if (_action != null)
            {
                _action.performed -= OnPerformed;
                _action.Disable();
            }
        }

        void OnPerformed(InputAction.CallbackContext ctx)
        {
            if (_target == null) return;

            _attrib = _attrib ?? _target.CreateVFXEventAttribute();
            _attrib.SetFloat(IDs.alpha, ctx.ReadValue<float>());

            _target.SendEvent(_eventID, _attrib);
        }
    }
}
