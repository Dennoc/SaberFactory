using System;
using System.Reflection;
using System.Threading.Tasks;
using IPA.Utilities;
using SaberFactory.DataStore;
using SaberFactory.Models;
using UnityEngine;

namespace SaberFactory.Helpers
{
    internal static class CommonHelpers
    {
        public static SaberType ToSaberType(this ESaberSlot saberSlot)
        {
            return saberSlot == ESaberSlot.Left ? SaberType.SaberA : SaberType.SaberB;
        }

        public static void SetLayer(this GameObject obj, int layer)
        {
            if (obj == null)
            {
                return;
            }

            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                if (child == null)
                {
                    continue;
                }

                SetLayer(child.gameObject, layer);
            }
        }

        public static void SetLayer<T>(this GameObject obj, int layer) where T : Component
        {
            if (obj == null)
            {
                return;
            }

            foreach (var comp in obj.GetComponentsInChildren<T>())
            {
                comp.gameObject.layer = layer;
            }
        }

        public static T GetOrAdd<T>(this GameObject obj) where T : Component
        {
            if (obj.GetComponent<T>() is { } comp) return comp;
            return obj.AddComponent<T>();
        }

        public static T Cast<T>(this object obj)
        {
            return (T)obj;
        }

        public static T CastChecked<T>(this object obj)
        {
            if (obj is T ret)
            {
                return ret;
            }

            return default;
        }

        public static async Task WaitForFinish(this ILoadingTask loadingTask)
        {
            if (loadingTask.CurrentTask == null)
            {
                return;
            }

            await loadingTask.CurrentTask;
        }

        public static Component Upgrade(Component monoBehaviour, Type upgradingType)
        {
            var originalType = monoBehaviour.GetType();

            var gameObject = monoBehaviour.gameObject;
            var upgradedDummyComponent = Activator.CreateInstance(upgradingType);

            
            for (var type = originalType; type != null && type != typeof(MonoBehaviour); type = type.BaseType)
            {
                foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    info.SetValue(upgradedDummyComponent, info.GetValue(monoBehaviour));
                }
            }

            UnityEngine.Object.DestroyImmediate(monoBehaviour);
            bool goState = gameObject.activeSelf;
            gameObject.SetActive(false);
            var upgradedMonoBehaviour = gameObject.AddComponent(upgradingType);

            for (var type = upgradingType; type != null && type != typeof(MonoBehaviour); type = type.BaseType)
            {
                foreach (FieldInfo info in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    info.SetValue(upgradedMonoBehaviour, info.GetValue(upgradedDummyComponent));
                }
            }

            gameObject.SetActive(goState);
            return upgradedMonoBehaviour;
        }
    }
} 