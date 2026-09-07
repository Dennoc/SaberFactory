using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaberFactory.Helpers;
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
    public class SaberModel : IFactorySerializable
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

        public async Task FromJson(JObject obj, Serializer serializer)
        {
            obj.Populate(this);
            var piecesTkn = obj.Property(nameof(PieceCollection));
            if (piecesTkn != null)
            {
                var pieceList = (JArray)piecesTkn.Value;
                foreach (var pieceTkn in pieceList)
                {
                    var piece = await serializer.LoadPiece(pieceTkn["Path"]);
                    if (piece == null)
                    {
                        continue;
                    }

                    PieceCollection.AddPiece(piece.AssetTypeDefinition, piece.GetPiece(SaberSlot));
                    await piece.GetPiece(SaberSlot)?.FromJson((JObject)pieceTkn, serializer);
                }
            }
        }

        public async Task<JToken> ToJson(Serializer serializer)
        {
            var obj = JObject.FromObject(this);
            
            var pieceList = new JArray();

            foreach (BasePieceModel pieceModel in PieceCollection)
            {
                pieceList.Add(await pieceModel.ToJson(serializer));
            }
            
            obj.Add(nameof(PieceCollection), pieceList);
            return obj;
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