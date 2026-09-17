using Newtonsoft.Json;
using SaberFactory.Models.CustomSaber;
using SaberFactory.Models.Whacker;
using SaberFactory.Serialization;
using UnityEngine;

namespace SaberFactory.Models
{
    /// <summary>
    ///     Stores information on how to build a saber instance
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SaberModel 
    {
        public bool IsEmpty => PieceCollection.PieceCount == 0;
        public readonly PieceCollection<BasePieceModel> PieceCollection;

        public readonly ESaberSlot SaberSlot;
        [JsonProperty] [MapSerialize] public float SaberLength = 1;

        [JsonProperty] [MapSerialize] public float SaberWidth = 1;

        public TrailModel TrailModel;

        public SaberModel(ESaberSlot saberSlot)
        {
            SaberSlot = saberSlot;

            PieceCollection = new PieceCollection<BasePieceModel>();
        }

        public void SetModelComposition(ModelComposition composition)
        {
            PieceCollection[composition.AssetTypeDefinition] = SaberSlot == ESaberSlot.Left
                ? composition.GetLeft()
                : composition.GetRight();
        }

        public TrailModel GetTrailModel()
        {
            if (GetCustomSaberOrWhacker(out var model))
            {
                return model switch
                {
                    CustomSaberModel customSaber => customSaber.TrailModel,
                    WhackerModel whacker => whacker.TrailModel,
                    _ => null
                };
            }

            return TrailModel;
        }

        public void Sync()
        {
            foreach (BasePieceModel piece in PieceCollection)
            {
                piece.ModelComposition.Sync(piece);
            }
        }

        public bool GetCustomSaberOrWhacker(out BasePieceModel customSaber)
        {
            if (PieceCollection.TryGetPiece(
                AssetTypeDefinition.CustomSaber,
                out var model))
            {
                switch (model)
                {
                    case CustomSaberModel cs:
                    case WhackerModel wi:
                        customSaber = model;
                        return true;
                }
            }

            customSaber = null;
            return false;
        }
    }
}