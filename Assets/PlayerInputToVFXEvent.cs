using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.InputSystem;

namespace Pyro
{
    public sealed class PlayerInputToVFXEvent : MonoBehaviour
    {
        [SerializeField] PlayerInput _input = null;
        [SerializeField] VisualEffect _target = null;

        VFXEventAttribute _attrib;

        static class IDs
        {
            public readonly static int alpha = Shader.PropertyToID("alpha");
        }

        void OnEnable()
        {
            if (_input != null)
                _input.onActionTriggered += OnActionTriggered;
        }

        void OnDisable()
        {
            if (_input != null)
                _input.onActionTriggered -= OnActionTriggered;
        }

        void OnActionTriggered(InputAction.CallbackContext ctx)
        {
            if (_target == null || !ctx.performed) return;

            _attrib = _attrib ?? _target.CreateVFXEventAttribute();
            _attrib.SetFloat(IDs.alpha, ctx.ReadValue<float>());

            _target.SendEvent("On" + ctx.action.name, _attrib);
        }
    }
}
