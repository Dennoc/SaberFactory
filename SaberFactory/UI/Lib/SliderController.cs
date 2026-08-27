using System;
using System.Linq;
using BeatSaberMarkupLanguage.Components.Settings;
using HMUI;
using SaberFactory.Helpers;
using TMPro;

namespace SaberFactory.UI.Lib
{
    internal class SliderController : ComponentController
    {
        public float Value
        {
            #if V_1_29_1
            get => Slider.slider.value;
            set
            {
                Slider.slider.value = value;
                Slider.ReceiveValue();
            }
            #else
            get => Slider.Slider.value;
            set
            {
                Slider.Slider.value = value;
                Slider.ReceiveValue();
            }
            #endif
        }

        public int IntValue
        {
            get => (int)Value;
            set => Value = value;
        }

        public readonly SliderSetting Slider;
        private Action<RangeValuesTextSlider, float> _currentEvent;

        public SliderController(SliderSetting slider)
        {
            Slider = slider;
        }

        public void AddEvent(Action<RangeValuesTextSlider, float> action)
        {
            if (_currentEvent is { })
            {
                return;
            }

            _currentEvent = action;
            #if V_1_29_1
                Slider.slider.valueDidChangeEvent += _currentEvent;
            #else
                Slider.Slider.valueDidChangeEvent += _currentEvent;
            #endif
        }

        public override void RemoveEvent()
        {
            if (_currentEvent is null)
            {
                return;
            }

            #if V_1_29_1
                Slider.slider.valueDidChangeEvent -= _currentEvent;
            #else
                Slider.Slider.valueDidChangeEvent -= _currentEvent;
            #endif
            _currentEvent = null;
        }

        public override string GetId()
        {
            #if V_1_29_1
            return ExternalComponents.components.First(x => x.name == "NameText").Cast<TextMeshProUGUI>().text;
            #else
            return ExternalComponents.Components.First(x => x.name == "NameText").Cast<TextMeshProUGUI>().text;
            #endif
        }

        public override void SetValue(object val)
        {
            Value = (float)val;
        }

        public override object GetValue()
        {
            return Value;
        }
    }
}