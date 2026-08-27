using SaberFactory.Models.Whacker;
using UnityEngine;

namespace SaberFactory.DataStore
{
    internal class WhackerStoreAsset : StoreAsset
    {
        public readonly WhackerManifest Manifest;

        public WhackerStoreAsset(string relativePath, GameObject prefab, AssetBundle assetBundle, WhackerManifest manifest)
            : base(relativePath, prefab, assetBundle)
        {
            Manifest = manifest;
        }
    }
}