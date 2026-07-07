using System;
using System.Reflection;
using System.Text;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.Tags;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SaberFactory.Helpers
{
    public static class DebugTools
    {
        public static void LogError(object message)
        {
            Debug.LogError(message);
        }

        public static GameObject CreateBall(BallOptions options)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = options.Pos;
            go.transform.SetParent(options.Parent, false);
            go.transform.localScale = Vector3.one * (options.Size ?? 0.03f);
            if (options.Color.HasValue) go.GetComponent<Renderer>().material.color = options.Color.Value.ColorWithAlpha(0);
            return go;
        }
        public static FloatingScreen CreateScreen(DebugCallbackString[] debugStrings, Vector2? size = null, Vector3? pos = null)
        {
            var screen = FloatingScreen.CreateFloatingScreen(size ?? new Vector2(500, 500), false, pos ?? new Vector3(0, 1, 1), Quaternion.identity);
            var bg = new BackgroundTag().CreateObject(screen.transform);
            bg.GetComponentInChildren<Backgroundable>().ApplyBackground("round-rect-panel");
            var vertical = new VerticalLayoutTag().CreateObject(bg.transform);
            foreach (var debugString in debugStrings)
            {
                var horizontal = new HorizontalLayoutTag().CreateObject(vertical.transform);
                new TextTag().CreateObject(horizontal.transform).GetComponentInChildren<TextMeshProUGUI>().text = debugString.Title;
                var str = new TextTag().CreateObject(horizontal.transform).GetComponentInChildren<TextMeshProUGUI>();
                str.text = debugString.Value;
                debugString.OnValueUpdated += s =>
                {
                    str.text = s;
                };
            }
            return screen;
        }
        public struct BallOptions
        {
            public Vector3 Pos;
            public float? Size;
            public Transform Parent;
            public Color? Color;
        }
    }
    public class DebugCallbackString
    {
        public event Action<string> OnValueUpdated;
        public string Value;
        public string Title;
        public DebugCallbackString(string title)
        {
            Title = title;
        }
        public void SetValue(string newValue)
        {
            Value = newValue;
            OnValueUpdated?.Invoke(newValue);
        }
    }
}