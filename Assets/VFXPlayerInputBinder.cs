using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using UnityEngine.InputSystem;

namespace Pyro
{
    [VFXBinder("Input/Player Input")]
    public class VFXPlayerInputBinder : VFXBinderBase
    {
        public string Property { get { return (string)m_Property; } set { m_Property = value; } }

        [VFXPropertyBinding("System.Single"), SerializeField]
        protected ExposedProperty m_Property = "FloatProperty";
        public PlayerInput PlayerInput;
        public string ActionName = "fire";

        InputAction m_CachedAction = null;

        public override bool IsValid(VisualEffect component)
        {
            return PlayerInput?.actions.FindAction(ActionName) != null && component.HasFloat(m_Property);
        }

        public override void UpdateBinding(VisualEffect component)
        {
            m_CachedAction = m_CachedAction ?? PlayerInput?.actions.FindAction(ActionName);
            component.SetFloat(m_Property, m_CachedAction.ReadValue<float>());
        }

        public override string ToString()
        {
            return string.Format("Input Value: '{0}' -> {1}", m_Property, ActionName);
        }
    }
}
