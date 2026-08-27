using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SaberFactory.Models.Whacker
{
    internal class WhackerDescriptor
    {
        [JsonProperty("author")] public string Author;
        [JsonProperty("objectName")] public string ObjectName;
        [JsonProperty("description")] public string Description;
        [JsonProperty("coverImage")] public string CoverImage;
    }

    internal class WhackerManifest
    {
        [JsonProperty("androidFileName")] public string AndroidFileName;
        [JsonProperty("pcFileName")] public string PcFileName;
        [JsonProperty("descriptor")] public WhackerDescriptor Descriptor;
        [JsonProperty("config")] public JObject Config;
    }

    internal class SaberInfo
    {
        [JsonProperty("hasTrail")] public bool HasTrail;
        [JsonProperty("keepFakeGlow")] public bool KeepFakeGlow;
        [JsonProperty("isLegacy")] public bool IsLegacy;
    }

    internal class TrailInfo
    {
        [JsonProperty("trailId")]
        public int TrailId;

        [JsonProperty("colorType")]
        public int ColorType;

        [JsonProperty("trailColor")]
        public Color TrailColor;

        [JsonProperty("multiplierColor")]
        public Color MultiplierColor;

        [JsonProperty("length")]
        public int Length;

        [JsonProperty("whiteStep")]
        public float WhiteStep;
    }

    internal class TrailObject
    {
        [JsonProperty("trailId")]
        public int TrailId;

        [JsonProperty("isTop")]
        public bool IsTop;
    }
}