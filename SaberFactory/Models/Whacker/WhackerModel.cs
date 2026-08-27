using System;
using System.Linq;
using System.Threading.Tasks;
using CustomSaber;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaberFactory.DataStore;
using SaberFactory.Helpers;
using SaberFactory.Instances;
using SaberFactory.Instances.CustomSaber;
using SaberFactory.Instances.Whacker;
using SaberFactory.Models.CustomSaber;
using SaberFactory.Serialization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SaberFactory.Models.Whacker
{
    public class WhackerModel : BasePieceModel
    {
        
        [Inject] private readonly PluginDirectories _pluginDirectories = null;
        
        public override Type InstanceType { get; protected set; } = typeof(WhackerInstance);
        
        private TrailModel _trailModel;
        private bool? _hasTrail;

        public bool HasTrail => _hasTrail ??= CheckTrail();

        public TrailModel TrailModel
        {
            get
            {
                if (_trailModel == null)
                {
                    var trailModel = GrabTrail(false);
                    if (trailModel == null)
                    {
                        _hasTrail = false;
                        return null;
                    }

                    _trailModel = trailModel;
                }

                return _trailModel;
            }

            set => _trailModel = value;
        }

        internal Vector3 TrailPointStartPosition { get; private set; }
        internal Vector3 TrailPointEndPosition { get; private set; }
        
        private bool _didReparentTrail;

        public WhackerModel(StoreAsset storeAsset) : base(storeAsset)
        {
            PropertyBlock = new PropHandler.CustomSaberPropertyBlock();
        }
        
        public override void OnLazyInit()
        {
            if (!HasTrail)
            {
                return;
            }

            var trailModel = TrailModel;

            var path = _pluginDirectories.Cache.GetFile(StoreAsset.NameWithoutExtension+".trail").FullName;
            var trail = QuickSave.LoadObject<CustomSaberModel.TrailProportions>(path);
            if (trail == null)
            {
                return;
            }

            trailModel.Length = trail.Length;
            trailModel.Width = trail.Width;
        }

        public override void SaveAdditionalData()
        {
            if (!HasTrail)
            {
                return;
            }

            var trailModel = TrailModel;

            var path = _pluginDirectories.Cache.GetFile(StoreAsset.NameWithoutExtension+".trail").FullName;
            var trail = new CustomSaberModel.TrailProportions
            {
                Length = trailModel.Length,
                Width = trailModel.Width
            };
            QuickSave.SaveObject(trail, path);
        }
        

        public override ModelMetaData GetMetaData()
        {
            var manifest = ((WhackerStoreAsset)StoreAsset).Manifest;
            var name = manifest.Descriptor.ObjectName ?? StoreAsset.NameWithoutExtension;
            var author = manifest.Descriptor.Author ?? "Unknown";
            return new ModelMetaData(name, author, LoadCoverSprite(), false);
        }

        public override void SyncFrom(BasePieceModel otherModel)
        {
            base.SyncFrom(otherModel);

            var otherWhacker = (WhackerModel)otherModel;

            if (otherWhacker.HasTrail || otherWhacker.TrailModel is { })
            {
                _trailModel ??= new TrailModel();

                TrailModel.TrailOriginTrails = otherWhacker.TrailModel.TrailOriginTrails;

                var originalMaterial = TrailModel.Material?.Material;

                TrailModel.CopyFrom(otherWhacker.TrailModel);

                var otherMat = TrailModel.Material.Material;

                if (originalMaterial != null && (string.IsNullOrWhiteSpace(TrailModel.TrailOrigin) ||
                                                 originalMaterial.shader.name == otherMat.shader.name))
                {
                    foreach (var prop in otherMat.GetProperties(MaterialAttributes.HideInSf))
                    {
                        originalMaterial.SetProperty(prop.Item2, prop.Item1, prop.Item3);
                    }

                    TrailModel.Material.Material = originalMaterial;
                }
                else
                {
                    originalMaterial.TryDestoryImmediate();
                }
            }
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
        
        public TrailModel GrabTrail(bool addTrailOrigin)
        {
            if (!HasTrail)
                return null;

            var saberRoot = SaberSlot == ESaberSlot.Left
                ? Prefab.transform.Find("LeftSaber")
                : Prefab.transform.Find("RightSaber");

            if (saberRoot == null)
                saberRoot = Prefab.transform;

            var texts = saberRoot.GetComponentsInChildren<Text>(true);

            TrailInfo trailInfo = null;
            Material material = null;

            foreach (var text in texts.Where(text => text.text.Contains("\"trailColor\":")))
            {
                var renderer = text.GetComponent<MeshRenderer>();
                if (renderer == null || renderer.material == null)
                    continue;

                try
                {
                    trailInfo = JsonConvert.DeserializeObject<TrailInfo>(text.text);
                }
                catch
                {
                    continue;
                }

                if (trailInfo == null)
                    continue;

                material = renderer.material;
                break;
            }

            if (trailInfo == null || material == null)
                return null;

            Transform top = null;
            Transform bottom = null;

            foreach (var text in texts.Where(text => text.text.Contains("\"isTop\":")))
            {
                TrailObject marker;

                try
                {
                    marker = JsonConvert.DeserializeObject<TrailObject>(text.text);
                }
                catch
                {
                    continue;
                }

                if (marker == null || marker.TrailId != trailInfo.TrailId)
                    continue;

                if (marker.IsTop)
                {
                    top = text.transform;
                }
                else
                {
                    bottom = text.transform;
                }
            }

            if (top == null || bottom == null)
                return null;

            var topPos =
                Prefab.transform.InverseTransformPoint(top.position);

            var bottomPos =
                Prefab.transform.InverseTransformPoint(bottom.position);

            TrailPointStartPosition = bottomPos;
            TrailPointEndPosition = topPos;
            
            FixTrailParents();

            return new TrailModel(
                Vector3.zero,
                Mathf.Abs(topPos.z - bottomPos.z),
                trailInfo.Length,
                new MaterialDescriptor(material),
                trailInfo.WhiteStep,
                null,
                addTrailOrigin ? StoreAsset.RelativePath : null);
        }
        
        public void FixTrailParents()
        {
            if (_didReparentTrail)
            {
                return;
            }
        
            _didReparentTrail = true;
        
            var trail = Prefab.GetComponent<CustomTrail>();
        
            if (trail is null)
            {
                return;
            }
        
            trail.PointStart.SetParent(Prefab.transform, true);
            trail.PointEnd.SetParent(Prefab.transform, true);
        }
        
        public override async Task FromJson(JObject obj, Serializer serializer)
        {
            await base.FromJson(obj, serializer);
            var trailModelToken = obj[nameof(TrailModel)];
            if (trailModelToken != null)
            {
                if (TrailModel == null)
                {
                    TrailModel = new TrailModel();
                }

                await TrailModel.FromJson((JObject)trailModelToken, serializer);
            }
        }

        public override async Task<JToken> ToJson(Serializer serializer)
        {
            var obj = (JObject)await base.ToJson(serializer);
            
            if (TrailModel != null)
            {
                obj.Add(nameof(TrailModel), await TrailModel.ToJson(serializer));
            }

            return obj;
        }
        
        internal class Factory : PlaceholderFactory<StoreAsset, WhackerModel>
        {
        }
    }
    
}
