using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using SaberFactory.UI.Lib.BSML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SaberFactory.UI.Lib.PropCells
{
    internal class FloatPropCell : BasePropCell
    {
        [UIComponent("bg")] private readonly Image _backgroundImage = null;
        [UIComponent("val-increment")] private readonly IncrementSetting _incrementSetting = null;
        [UIComponent("val-increment")] private readonly TextMeshProUGUI _sliderSettingText = null;

        public override void SetData(PropertyDescriptor data)
        {
            if (!(data.PropObject is float val))
            {
                return;
            }

            OnChangeCallback = data.ChangedCallback;

            #if V_1_29_1
            if (data.AddtionalData is Vector2 minMax && val > minMax.x && val < minMax.y)
            {
                _incrementSetting.minValue = minMax.x;
                _incrementSetting.maxValue = minMax.y;

                // 1% of total, also idk if 0.01f is better or division by 100?
                _incrementSetting.increments = (minMax.y - minMax.x) * 0.01f;
            }
            else
            {
                _incrementSetting.minValue = -1000;
                _incrementSetting.maxValue = 1000;
                _incrementSetting.increments = 0.1f;
            }
            #else
            if (data.AddtionalData is Vector2 minMax && val > minMax.x && val < minMax.y)
            {
                _incrementSetting.MinValue = minMax.x;
                _incrementSetting.MaxValue = minMax.y;

                // 1% of total, also idk if 0.01f is better or division by 100?
                _incrementSetting.Increments = (minMax.y - minMax.x) * 0.01f;
            }
            else
            {
                _incrementSetting.MinValue = -1000;
                _incrementSetting.MaxValue = 1000;
                _incrementSetting.Increments = 0.1f;
            }
            #endif
            


            _incrementSetting.Value = val;
            _incrementSetting.ReceiveValue();
            _sliderSettingText.text = data.Text;

            if (ThemeManager.GetDefinedColor("prop-cell", out var bgColor))
            {
                _backgroundImage.type = Image.Type.Sliced;
                _backgroundImage.color = bgColor;
            }
        }

        [UIAction("slider-changed")]
        private void SliderChanged(float val)
        {
            OnChangeCallback?.Invoke(val);
        }
    }
}