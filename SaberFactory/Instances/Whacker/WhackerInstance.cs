using System.Collections.Generic;
using CustomSaber;
using HarmonyLib;
using SaberFactory.Helpers;
using SaberFactory.Instances.PostProcessors;
using SaberFactory.Instances.Trail;
using SaberFactory.Models;
using SaberFactory.Models.Whacker;
using UnityEngine;

namespace SaberFactory.Instances.Whacker
{
    internal class WhackerInstance : BasePieceInstance
    {
        public InstanceTrailData InstanceTrailData { get; private set; }

        public WhackerInstance(
            WhackerModel model,
            List<IPartPostProcessor> postProcessors)
            : base(model, postProcessors)
        {
            if (model.HasTrail)
            {
                InitializeTrailData(GameObject, model.TrailModel);
            }
        }

        public override PartEvents GetEvents()
        {
            return PartEvents.FromCustomSaber(GameObject);
        }

        protected override GameObject Instantiate()
        {
            var model = Model.Cast<WhackerModel>();

            var instance = Object.Instantiate(
                model.Prefab,
                Vector3.zero,
                Quaternion.identity);

            instance.SetActive(true);

            _postProcessors.Do(x => x.ProcessPart(instance));

            return instance;
        }

        private void InitializeTrailData(
            GameObject whackerObject,
            TrailModel trailModel)
        {
            if (whackerObject == null || trailModel == null)
            {
                return;
            }

            var trail = whackerObject.AddComponent<CustomTrail>();

            trail.Length = trailModel.Length;
            trail.TrailMaterial = trailModel.Material?.Material;

            trail.PointStart =
                whackerObject.CreateGameObject("PointStart").transform;

            trail.PointEnd =
                whackerObject.CreateGameObject("PointEnd").transform;

            trail.PointStart.localPosition = Vector3.zero;
            trail.PointEnd.localPosition = trailModel.TrailPosOffset;

            var pointStart = trail.PointStart;
            var pointEnd = trail.PointEnd;

            var isTrailReversed =
                pointStart.localPosition.z > pointEnd.localPosition.z;

            if (isTrailReversed)
            {
                pointStart = trail.PointEnd;
                pointEnd = trail.PointStart;
            }

            InstanceTrailData = new InstanceTrailData(
                trailModel,
                pointStart,
                pointEnd,
                isTrailReversed,
                null);
        }

        protected override void GetColorableMaterials(List<Material> materials)
        {
            void AddMaterial(
                Renderer renderer,
                Material[] rendererMaterials,
                int index)
            {
                rendererMaterials[index] =
                    new Material(rendererMaterials[index]);

                renderer.sharedMaterials = rendererMaterials;
                materials.Add(rendererMaterials[index]);
            }

            foreach (var renderer in GameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is null)
                    continue;

                var rendererMaterials = renderer.sharedMaterials;

                for (var i = 0; i < rendererMaterials.Length; i++)
                {
                    var material = rendererMaterials[i];

                    if (material is null ||
                        !material.HasProperty(MaterialProperties.MainColor))
                    {
                        continue;
                    }

                    if (material.TryGetFloat(
                            MaterialProperties.CustomColors,
                            out var val))
                    {
                        if (val > 0)
                            AddMaterial(renderer, rendererMaterials, i);
                    }
                    else if (material.TryGetFloat(
                                 MaterialProperties.Glow,
                                 out val) &&
                             val > 0)
                    {
                        AddMaterial(renderer, rendererMaterials, i);
                    }
                    else if (material.TryGetFloat(
                                 MaterialProperties.Bloom,
                                 out val) &&
                             val > 0)
                    {
                        AddMaterial(renderer, rendererMaterials, i);
                    }
                }
            }
        }
    }
}