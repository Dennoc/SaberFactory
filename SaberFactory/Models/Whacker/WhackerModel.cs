using System;
using System.Linq;
using System.Threading.Tasks;
using CustomSaber;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaberFactory.DataStore;
using SaberFactory.Helpers;
using SaberFactory.Instances;
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

        public bool HasTrail
        {
            get
            {
                _hasTrail ??= CheckTrail();
                return _hasTrail.Value;
            }
        }

        private TrailModel _trailModel;
        private bool? _hasTrail;
        private bool _didReparentTrail;

        internal Vector3 TrailPointStartPosition { get; private set; }
        internal Vector3 TrailPointEndPosition { get; private set; }

        public WhackerModel(StoreAsset storeAsset) : base(storeAsset)
        {
            PropertyBlock = new PropHandler.WhackerPropertyBlock();
        }

        public override void OnLazyInit()
        {
            if (!HasTrail)
            {
                return;
            }

            var trailModel = TrailModel;

            var path = _pluginDirectories.Cache
                .GetFile(StoreAsset.NameWithoutExtension + ".trail")
                .FullName;

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

            var path = _pluginDirectories.Cache
                .GetFile(StoreAsset.NameWithoutExtension + ".trail")
                .FullName;

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

            var name = manifest.Descriptor.ObjectName
                       ?? StoreAsset.NameWithoutExtension;

            var author = manifest.Descriptor.Author
                         ?? "Unknown";

            return new ModelMetaData(
                name,
                author,
                LoadCoverSprite(),
                false
            );
        }

        public override void SyncFrom(BasePieceModel otherModel)
        {
            base.SyncFrom(otherModel);

            var otherTrailModel = otherModel switch
            {
                WhackerModel whacker => whacker.TrailModel,
                CustomSaberModel customSaber => customSaber.TrailModel,
                _ => null
            };

            if (otherTrailModel == null)
            {
                return;
            }

            TrailModel ??= new TrailModel();

            TrailModel.TrailOriginTrails = otherTrailModel.TrailOriginTrails;

            // Mirror CustomSaberModel's material handling.
            var originalMaterial = TrailModel.Material?.Material;

            TrailModel.CopyFrom(otherTrailModel);

            var otherMaterial = TrailModel.Material?.Material;

            if (originalMaterial != null &&
                otherMaterial != null &&
                (string.IsNullOrWhiteSpace(TrailModel.TrailOrigin) ||
                 originalMaterial.shader.name == otherMaterial.shader.name))
            {
                foreach (var prop in otherMaterial.GetProperties(MaterialAttributes.HideInSf))
                {
                    originalMaterial.SetProperty(prop.Item2, prop.Item1, prop.Item3);
                }

                TrailModel.Material.Material = originalMaterial;
            }
            else if (originalMaterial != null)
            {
                originalMaterial.TryDestoryImmediate();
            }

            // Whacker trails are generated onto the prefab, so keep the
            // CustomTrail component synchronized with the model material.
            var trail = Prefab.GetComponent<CustomTrail>();
            if (trail != null && TrailModel.Material?.Material != null)
            {
                trail.TrailMaterial = TrailModel.Material.Material;
            }
        }

        public TrailModel GrabTrail(bool addTrailOrigin)
        {
            if (!HasTrail)
            {
                return null;
            }

            var saberRoot = SaberSlot == ESaberSlot.Left
                ? Prefab.transform.Find("LeftSaber")
                : Prefab.transform.Find("RightSaber");

            CustomTrail SetupTrail(
                Vector3 startPosition,
                Vector3 endPosition,
                int length,
                Material material)
            {
                var existingTrail = Prefab.GetComponent<CustomTrail>();

                if (existingTrail != null)
                {
                    return existingTrail;
                }

                var trail = Prefab.AddComponent<CustomTrail>();

                trail.Length = length;
                trail.TrailMaterial = material;

                // trail.PointStart =
                //     Prefab.CreateGameObject("PointStart").transform;
                //
                // trail.PointEnd =
                //     Prefab.CreateGameObject("PointEnd").transform;
                //
                // trail.PointStart.localPosition = startPosition;
                // trail.PointEnd.localPosition = endPosition; // This seems to cause the crashes hahaha I wanna kms

                return trail;
            }

            saberRoot = saberRoot != null ? saberRoot : Prefab.transform;

            var texts = saberRoot.GetComponentsInChildren<Text>(true);

            TrailInfo trailInfo = null;
            Material material = null;

            foreach (var text in texts.Where(x => x.text.Contains("\"trailColor\":")))
            {
                var renderer = text.GetComponent<MeshRenderer>();

                if (renderer == null || renderer.material == null)
                {
                    continue;
                }

                try
                {
                    trailInfo = JsonConvert.DeserializeObject<TrailInfo>(text.text);
                }
                catch
                {
                    continue;
                }

                if (trailInfo == null)
                {
                    continue;
                }

                material = renderer.material;
                break;
            }

            if (trailInfo == null || material == null)
            {
                return null;
            }

            Transform top = null;
            Transform bottom = null;

            foreach (var text in texts.Where(x => x.text.Contains("\"isTop\":")))
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
                {
                    continue;
                }

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
            {
                return null;
            }

            var topPos = Prefab.transform.InverseTransformPoint(top.position);
            var bottomPos = Prefab.transform.InverseTransformPoint(bottom.position);

            TrailPointStartPosition = bottomPos;
            TrailPointEndPosition = topPos;

            SetupTrail(
                bottomPos,
                topPos,
                trailInfo.Length,
                material
            );
            
            var trailComp = Prefab.GetComponent<CustomTrail>();

            return new TrailModel(
                Vector3.zero,
                Mathf.Abs(topPos.z - bottomPos.z),
                trailInfo.Length,
                new MaterialDescriptor(trailComp.TrailMaterial),
                trailInfo.WhiteStep,
                null,
                addTrailOrigin ? StoreAsset.RelativePath : null
            );
        }

        private bool CheckTrail()
        {
            var manifest = ((WhackerStoreAsset)StoreAsset).Manifest;
            var info = manifest.Config?.ToObject<SaberInfo>();

            return info != null && info.HasTrail;
        }

        public void ResetTrail()
        {
            TrailModel = GrabTrail(false);
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

                await TrailModel.FromJson(
                    (JObject)trailModelToken,
                    serializer
                );
            }
        }

        public override async Task<JToken> ToJson(Serializer serializer)
        {
            var obj = (JObject)await base.ToJson(serializer);

            if (TrailModel != null)
            {
                obj.Add(
                    nameof(TrailModel),
                    await TrailModel.ToJson(serializer)
                );
            }

            return obj;
        }

        private Sprite LoadCoverSprite()
        {
            var manifest = ((WhackerStoreAsset)StoreAsset).Manifest;

            if (string.IsNullOrEmpty(manifest.Descriptor.CoverImage))
            {
                return null;
            }

            var fullPath = PathTools.ToFullPath(StoreAsset.RelativePath);

            using var zip = System.IO.Compression.ZipFile.OpenRead(fullPath);

            var entry = zip.GetEntry(manifest.Descriptor.CoverImage);

            if (entry == null)
            {
                return null;
            }

            using var ms = new System.IO.MemoryStream();
            entry.Open().CopyTo(ms);

            var tex = new Texture2D(2, 2);

            if (!tex.LoadImage(ms.ToArray()))
            {
                return null;
            }

            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        internal class Factory : Zenject.PlaceholderFactory<StoreAsset, WhackerModel>
        {
        }
    }
}