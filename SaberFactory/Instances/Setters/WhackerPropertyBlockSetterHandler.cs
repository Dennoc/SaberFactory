using SaberFactory.Models.CustomSaber;
using SaberFactory.Models.PropHandler;
using SaberFactory.Models.Whacker;
using UnityEngine;

namespace SaberFactory.Instances.Setters
{
    internal class WhackerPropertyBlockSetterHandler : PropertyBlockSetterHandler
    {
        public TransformDataSetter TransformDataSetter;

        public WhackerPropertyBlockSetterHandler(GameObject gameObject, WhackerModel model)
        {
            var propBlock = (WhackerPropertyBlock)model.PropertyBlock;
            TransformDataSetter = new TransformDataSetter(gameObject, propBlock.TransformProperty);
        }
    }
}