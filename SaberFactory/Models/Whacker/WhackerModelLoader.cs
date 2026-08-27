using SaberFactory.Configuration;
using SaberFactory.DataStore;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SaberFactory.Models.Whacker
{
    internal class WhackerModelLoader : IStoreAssetParser
    {
        private readonly PluginConfig _config;

        public WhackerModelLoader(PluginConfig config)
        {
            _config = config;
        }

        public ModelComposition GetComposition(StoreAsset storeAsset)
        {
            var (leftSaber, rightSaber) = GetSabers(storeAsset.Prefab.transform);

            if (rightSaber == null)
            {
                var newParent = new GameObject("RightSaber").transform;
                newParent.parent = storeAsset.Prefab.transform;

                rightSaber = Object.Instantiate(leftSaber, newParent, false);
                rightSaber.transform.localScale = new Vector3(-1, 1, 1);
                rightSaber.name = "RightSaberMirror";

                rightSaber = newParent.gameObject;
                rightSaber.SetActive(false);
            }

            var manifest = ((WhackerStoreAsset)storeAsset).Manifest;
            var storeAssetLeft = new WhackerStoreAsset(storeAsset.RelativePath, leftSaber, storeAsset.AssetBundle, manifest);
            var storeAssetRight = new WhackerStoreAsset(storeAsset.RelativePath, rightSaber, storeAsset.AssetBundle, manifest);

            var modelLeft = new WhackerModel(storeAssetLeft) { SaberSlot = ESaberSlot.Left };
            var modelRight = new WhackerModel(storeAssetRight) { SaberSlot = ESaberSlot.Right };

            var composition = new ModelComposition(AssetTypeDefinition.CustomSaber, modelLeft, modelRight, storeAsset.Prefab);
            composition.SetFavorite(_config.IsFavorite(storeAsset.RelativePath));

            return composition;
        }

        private (GameObject leftSaber, GameObject rightSaber) GetSabers(Transform root)
        {
            GameObject leftSaber = null, rightSaber = null;
            foreach (Transform t in root)
            {
                if (t.name == "LeftSaber") leftSaber = t.gameObject;
                else if (t.name == "RightSaber") rightSaber = t.gameObject;
                if (leftSaber != null && rightSaber != null) break;
            }
            return (leftSaber, rightSaber);
        }
    }
}