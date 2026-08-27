using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaberFactory.DataStore;
using SaberFactory.Instances;
using SaberFactory.Instances.CustomSaber;
using SaberFactory.Instances.Whacker;
using SaberFactory.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace SaberFactory.Models.Whacker
{
    internal class WhackerModel : BasePieceModel
    {
        public override Type InstanceType { get; protected set; } = typeof(WhackerInstance);
        
        private TrailModel _trailModel;
        private bool? _hasTrail;

        public bool HasTrail => _hasTrail ??= CheckTrail();

        public TrailModel TrailModel => _trailModel ??= GrabTrail();

        public WhackerModel(StoreAsset storeAsset) : base(storeAsset)
        {
            PropertyBlock = new PropHandler.CustomSaberPropertyBlock();
        }

        public override ModelMetaData GetMetaData()
        {
            var manifest = ((WhackerStoreAsset)StoreAsset).Manifest;
            var name = manifest.Descriptor.ObjectName ?? StoreAsset.NameWithoutExtension;
            var author = manifest.Descriptor.Author ?? "Unknown";
            return new ModelMetaData(name, author, LoadCoverSprite(), false);
        }

        private Sprite LoadCoverSprite()
        {
            var manifest = ((WhackerStoreAsset)StoreAsset).Manifest;
            if (string.IsNullOrEmpty(manifest.Descriptor.CoverImage))
            {
                return null;
            }

            var fullPath = Helpers.PathTools.ToFullPath(StoreAsset.RelativePath);
            using var zip = System.IO.Compression.ZipFile.OpenRead(fullPath);
            var entry = zip.GetEntry(manifest.Descriptor.CoverImage);
            if (entry == null) return null;

            using var ms = new System.IO.MemoryStream();
            entry.Open().CopyTo(ms);

            var tex = new Texture2D(2, 2);
            if (!tex.LoadImage(ms.ToArray())) return null;

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        private bool CheckTrail()
        {
            var manifest = ((WhackerStoreAsset)StoreAsset).Manifest;
            var info = manifest.Config?.ToObject<SaberInfo>();
            return info != null && info.HasTrail;
        }
        
        private TrailModel GrabTrail()
        {
            if (!HasTrail) return null;

            var manifest = ((WhackerStoreAsset)StoreAsset).Manifest;
            var trailInfos = manifest.Config?["trailInfo"]?.ToObject<TrailInfo[]>();
            if (trailInfos == null || trailInfos.Length == 0) return null;

            var saberRoot = SaberSlot == ESaberSlot.Left
                ? Prefab.transform.Find("LeftSaber")
                : Prefab.transform.Find("RightSaber");
            if (saberRoot == null) saberRoot = Prefab.transform;

            Transform top = null, bottom = null;
            foreach (var text in saberRoot.GetComponentsInChildren<Text>(true))
            {
                TrailObject marker;
                try { marker = JsonConvert.DeserializeObject<TrailObject>(text.text); }
                catch { continue; }

                if (marker == null || marker.TrailId != trailInfos[0].TrailId) continue;
                if (marker.IsTop) top = text.transform; else bottom = text.transform;
            }

            if (top == null || bottom == null) return null;

            var meshRenderer = top.parent != null
                ? top.parent.GetComponent<MeshRenderer>()
                : null;
            if (meshRenderer == null || meshRenderer.material == null) return null;

            var topPos = top.position - Prefab.transform.position;
            var bottomPos = bottom.position - Prefab.transform.position;

            return new TrailModel(
                topPos - bottomPos,
                0.5f,
                trailInfos[0].Length,
                new MaterialDescriptor(meshRenderer.material),
                trailInfos[0].WhiteStep,
                null,
                StoreAsset.RelativePath);
        }
    }
}