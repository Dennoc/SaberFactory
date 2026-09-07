using System.Collections.Generic;
using CustomSaber;
using HarmonyLib;
using SaberFactory.Helpers;
using SaberFactory.Instances.PostProcessors;
using SaberFactory.Instances.Setters;
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
            InitializeTrailData(GameObject, model);
        }

        /// <summary>
        /// Creates an <see cref="InstanceTrailData" /> object
        /// with the correct trail transforms.
        /// </summary>
        /// <param name="whackerObject">
        /// The Whacker GameObject the <see cref="CustomTrail" /> component is on.
        /// </param>
        /// <param name="model">
        /// The <see cref="WhackerModel" /> to use.
        /// </param>
        private void InitializeTrailData(GameObject whackerObject, WhackerModel model)
        {
            var trailModel = model.TrailModel;

            if (whackerObject is null || trailModel is null)
                return;

            var trail = whackerObject.GetComponent<CustomTrail>();

            if (trail is null)
                return;

            trail.Length = trailModel.Length;
            trail.TrailMaterial = trailModel.Material?.Material;

            var pointStart = new GameObject("PointStart").transform;
            var pointEnd = new GameObject("PointEnd").transform;

            pointStart.SetParent(whackerObject.transform, false);
            pointEnd.SetParent(whackerObject.transform, false);

            pointStart.localPosition = model.TrailPointStartPosition;
            pointEnd.localPosition = model.TrailPointEndPosition;

            InstanceTrailData = new InstanceTrailData(
                trailModel,
                pointStart,
                pointEnd,
                trailModel.Flip);
        }

        public override PartEvents GetEvents()
        {
            return PartEvents.FromCustomSaber(GameObject);
        }

        protected override GameObject Instantiate()
        {
            var model = Model.Cast<WhackerModel>();
            // model.FixTrailParents();

            var instance = Object.Instantiate(
                model.Prefab,
                Vector3.zero,
                Quaternion.identity
            );

            instance.SetActive(true);

            PropertyBlockSetterHandler =
                new WhackerPropertyBlockSetterHandler(
                    instance,
                    model
                );

            _postProcessors.Do(x => x.ProcessPart(instance));

            return instance;
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
                {
                    continue;
                }

                var rendererMaterials = renderer.sharedMaterials;
                var materialCount = rendererMaterials.Length;

                for (var i = 0; i < materialCount; i++)
                {
                    var material = rendererMaterials[i];

                    if (material is null ||
                        !material.HasProperty(MaterialProperties.MainColor))
                    {
                        continue;
                    }

                    // Always color materials if "_CustomColors" is > 0.
                    // If "_CustomColors" exists but is 0, don't color it.
                    if (material.TryGetFloat(
                            MaterialProperties.CustomColors,
                            out var val))
                    {
                        if (val > 0)
                        {
                            AddMaterial(renderer, rendererMaterials, i);
                        }
                    }
                    // If "_CustomColors" isn't present, fall back to Glow.
                    else if (material.TryGetFloat(
                                 MaterialProperties.Glow,
                                 out val) &&
                             val > 0)
                    {
                        AddMaterial(renderer, rendererMaterials, i);
                    }
                    // Finally fall back to Bloom.
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