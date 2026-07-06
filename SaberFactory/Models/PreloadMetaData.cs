using System;
using System.IO;
using BeatSaberMarkupLanguage;
using SaberFactory.Loaders;
using SaberFactory.UI;
using SaberFactory.UI.Lib;
using UnityEngine;

namespace SaberFactory.Models
{
    internal class PreloadMetaData : ICustomListItem
    {
        public AssetTypeDefinition AssetTypeDefinition { get; private set; }

        public Texture2D CoverTex
        {
            get
            {
                if (_coverTex == null)
                {
                    _coverTex = LoadTexture();
                }

                return _coverTex;
            }
        }

        public Sprite CoverSprite
        {
            get
            {
                if (_coverSprite == null)
                {
                    _coverSprite = LoadSprite();
                }

                return _coverSprite;
            }
        }

        internal readonly AssetMetaPath AssetMetaPath;

        private byte[] _coverData;
        private Sprite _coverSprite;
        private Texture2D _coverTex;

        internal PreloadMetaData(AssetMetaPath assetMetaPath)
        {
            AssetMetaPath = assetMetaPath;
        }

        internal PreloadMetaData(AssetMetaPath assetMetaPath, ICustomListItem customListItem, AssetTypeDefinition assetTypeDefinition)
        {
            AssetMetaPath = assetMetaPath;
            AssetTypeDefinition = assetTypeDefinition;
            ListName = customListItem.ListName;
            ListAuthor = customListItem.ListAuthor;
            _coverSprite = customListItem.ListCover;
        }

        public string ListName { get; private set; }

        public string ListAuthor { get; private set; }

        public Sprite ListCover => CoverSprite;

        public bool IsFavorite { get; set; }

        public void SaveToFile()
        {
            if (AssetMetaPath.HasMetaData)
            {
                File.Delete(AssetMetaPath.MetaDataPath);
            }

            byte[] coverData = null;
            if (_coverSprite != null)
            {
                var tex = _coverSprite.texture;
                coverData = GetTextureData(tex);
            }

            using (var fs = new FileStream(AssetMetaPath.MetaDataPath, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write(ListName ?? string.Empty);
                writer.Write(ListAuthor ?? string.Empty);
                writer.Write((int)AssetTypeDefinition.AssetType);
                writer.Write((int)AssetTypeDefinition.AssetSubType);

                if (coverData != null)
                {
                    writer.Write(coverData.Length);
                    writer.Write(coverData);
                }
                else
                {
                    writer.Write(0);
                }
            }
        }

        public void LoadFromFile()
        {
            LoadFromFile(AssetMetaPath.MetaDataPath);
        }

        public void LoadFromFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(fs))
            {
                ListName = reader.ReadString();
                ListAuthor = reader.ReadString();
                var assetType = (EAssetType)reader.ReadInt32();
                var assetSubType = (EAssetSubType)reader.ReadInt32();
                AssetTypeDefinition = new AssetTypeDefinition(assetType, assetSubType);

                var coverLen = reader.ReadInt32();
                _coverData = coverLen > 0 ? reader.ReadBytes(coverLen) : null;
            }

            LoadSprite();
        }

        public void SetFavorite(bool isFavorite)
        {
            IsFavorite = isFavorite;
        }

        /// <summary>
        ///     Get Texture png data from non-readable texture
        /// </summary>
        /// <param name="tex">The texture to read from</param>
        /// <returns>png bytes</returns>
        private byte[] GetTextureData(Texture2D tex)
        {
            var tmp = RenderTexture.GetTemporary(
                tex.width,
                tex.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Default);

            Graphics.Blit(tex, tmp);

            var previous = RenderTexture.active;
            RenderTexture.active = tmp;
            var myTexture2D = new Texture2D(tex.width, tex.height);
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            return myTexture2D.EncodeToPNG();
        }

        private Texture2D LoadTexture()
        {
            return _coverData == null ? null : Utilities.LoadTextureRaw(_coverData);
        }

        private Sprite LoadSprite()
        {
            return CoverTex == null ? null : Utilities.LoadSpriteFromTexture(CoverTex);
        }
    }
}