using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

#if !V_1_29_1
using AssetBundleLoadingTools.Utilities;
#endif
using Newtonsoft.Json;
using SaberFactory.DataStore;
using SaberFactory.Helpers;
using SaberFactory.Models.Whacker;
using SiraUtil.Logging;
using UnityEngine;

namespace SaberFactory.Loaders
{
    internal class WhackerAssetLoader : AssetBundleLoader
    {
        public WhackerAssetLoader()
        { }

        public override string HandledExtension => ".whacker";

        public override ISet<AssetMetaPath> CollectFiles(PluginDirectories dirs)
        {
            var paths = new HashSet<AssetMetaPath>();
            foreach (var path in dirs.CustomSaberDir.EnumerateFiles($"*{HandledExtension}", SearchOption.AllDirectories))
            {
                paths.Add(new AssetMetaPath(path, dirs.Cache.GetFile(path.Name + ".meta").FullName));
            }
            return paths;
        }

        public override async Task<StoreAsset> LoadStoreAssetAsync(string relativePath)
        {
            var fullPath = PathTools.ToFullPath(relativePath);
            if (!File.Exists(fullPath))
                return null;

            using var zip = ZipFile.OpenRead(fullPath);

            var manifestEntry = zip.GetEntry("package.json");
            if (manifestEntry == null)
            {
                Debug.LogWarning($"{relativePath}: missing package.json");
                return null;
            }

            WhackerManifest manifest;
            using (var sr = new StreamReader(manifestEntry.Open()))
            {
                manifest = JsonConvert.DeserializeObject<WhackerManifest>(sr.ReadToEnd());
            }

            if (manifest == null)
            {
                Debug.LogWarning($"{relativePath}: invalid package.json");
                return null;
            }

            var bundleEntry = zip.GetEntry(manifest.PcFileName);
            if (bundleEntry == null)
            {
                Debug.LogWarning(
                    $"{relativePath}: Android bundle entry '{manifest.AndroidFileName}' not found"
                );
                return null;
            }

            byte[] bundleBytes;
            using (var bundleStream = bundleEntry.Open())
            using (var ms = new MemoryStream())
            {
                bundleStream.CopyTo(ms);
                bundleBytes = ms.ToArray();
            }

            // First load the bundle and asset.
            #if !V_1_29_1
            var result = await Readers.LoadAssetFromAssetBundleSafeAsync<GameObject>(
                bundleBytes,
                "_Whacker"
            );
            #else
            var result = await Readers.LoadAssetFromAssetBundleAsync<GameObject>(
                bundleBytes,
                "_Whacker"
            );
            #endif

            
            if (result == null)
            {
                Debug.LogError(
                    $"{relativePath}: failed to load Android AssetBundle '{manifest.AndroidFileName}'"
                );
                return null;
            }

            // Bundle loaded successfully, but the modern prefab name wasn't found.
            if (result.Item1 == null)
            {
                Debug.LogWarning(
                    $"{relativePath}: '_Whacker' not found, retrying legacy '_CustomSaber'"
                );

                #if !V_1_29_1
                result = await Readers.LoadAssetFromAssetBundleSafeAsync<GameObject>(
                    bundleBytes,
                    "_CustomSaber"
                );
                #else
                result = await Readers.LoadAssetFromAssetBundleAsync<GameObject>(
                    bundleBytes,
                    "_CustomSaber"
                );
                #endif

                if (result == null || result.Item1 == null)
                {
                    Debug.LogWarning(
                        $"{relativePath}: neither '_Whacker' nor '_CustomSaber' could be loaded"
                    );
                    return null;
                }
            }

            result.Item1.hideFlags |= HideFlags.DontUnloadUnusedAsset;

            try
            {
                #if !V_1_29_1
                var info = await ShaderRepair.FixShadersOnGameObjectAsync(result.Item1);

                if (!info.AllShadersReplaced)
                {
                    Debug.LogWarning($"Missing shader replacement data for {relativePath}:");

                    foreach (var shaderName in info.MissingShaderNames)
                    {
                        Debug.LogWarning($"\t- {shaderName}");
                    }
                }
                
                #endif
            }
            finally
            {
                result.Item1.hideFlags &= ~HideFlags.DontUnloadUnusedAsset;
            }

            return new WhackerStoreAsset(
                relativePath,
                result.Item1,
                result.Item2,
                manifest
            );
        }

        public override Task<StoreAsset> LoadStoreAssetFromBundleAsync(AssetBundle bundle, string saberName)
            => Task.FromResult<StoreAsset>(null); // not applicable for whackers
    }
}