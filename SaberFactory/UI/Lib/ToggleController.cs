using System;
using System.Linq;
using BeatSaberMarkupLanguage.Components.Settings;
using SaberFactory.Helpers;
using TMPro;
using UnityEngine.Events;

namespace SaberFactory.UI.Lib
{
    internal class ToggleController : ComponentController
    {
        public bool Value
        {
            get => Toggle.Value;
            set => Toggle.Value = value;
        }

        public readonly ToggleSetting Toggle;
        private UnityAction<bool> _event;

        public ToggleController(ToggleSetting toggle)
        {
            Toggle = toggle;
        }

        public void SetEvent(Action<bool> action)
        {
            RemoveEvent();
            _event = new UnityAction<bool>(action);
            #if V_1_29_1
            Toggle.toggle.onValueChanged.AddListener(_event);
            #else
            Toggle.Toggle.onValueChanged.AddListener(_event);
            #endif
        }

        public override void RemoveEvent()
        {
            if (_event != null)
            {
                #if V_1_29_1
                Toggle.toggle.onValueChanged.RemoveListener(_event);
                #else
                Toggle.Toggle.onValueChanged.RemoveListener(_event);
                #endif
            }
        }

        public override string GetId()
        {
            #if V_1_29_1
            return ExternalComponents.components.First(x => true).Cast<TextMeshProUGUI>().text;
            #else
            return ExternalComponents.Components.First(x => true).Cast<TextMeshProUGUI>().text;
            #endif
        }

        public override void SetValue(object val)
        {
            Value = (bool)val;
        }

        public override object GetValue()
        {
            return Value;
        }
    }
}