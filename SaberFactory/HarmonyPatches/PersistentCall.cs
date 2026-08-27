using System;
using System.Reflection;
using HarmonyLib;

namespace SaberFactory.HarmonyPatches
{
    
    [HarmonyPatch]
    internal class PersistentCall_OnAfterDeserialize
    {
        private static readonly Type PersistentCallType = Type.GetType("UnityEngine.Events.PersistentCall, UnityEngine.CoreModule", true);
        private static readonly MethodInfo OnAfterDeserializeMethod = AccessTools.DeclaredMethod(PersistentCallType, "OnAfterDeserialize");
        private static readonly FieldInfo TargetAssemblyTypeName = AccessTools.DeclaredField(PersistentCallType, "m_TargetAssemblyTypeName");

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(object __instance)
        {
            TargetAssemblyTypeName.SetValue(__instance, null);
        }

        public static MethodBase TargetMethod() => OnAfterDeserializeMethod;
    }
}
