using HarmonyLib;
using SaberFactory.Configuration;
using SaberFactory.Helpers;
using UnityEngine;

namespace SaberFactory.Instances.PostProcessors
{
    internal class MainSaberPostProcessor : ISaberPostProcessor
    {
        private readonly PluginConfig _config;

        internal MainSaberPostProcessor(PluginConfig config)
        {
            _config = config;
        }

        public void ProcessSaber(SaberInstance saberObject)
        {
            var gameobject = saberObject.GameObject;
            gameobject.SetLayer<Renderer>(12);
            gameobject.GetComponentsInChildren<Collider>().Do(x => x.enabled = false);
            gameobject.GetComponentsInChildren<AudioSource>(true).Do(x => x.volume *= _config.SaberAudioVolumeMultiplier);
            gameobject.GetComponentsInChildren<Renderer>(true).Do(x => { x.sortingOrder = 3; });

            
            
               
            
        }
    }
}