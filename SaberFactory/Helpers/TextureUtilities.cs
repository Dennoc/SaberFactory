using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaberFactory.Helpers
{
    public static class TextureUtilities
    {
        
        public static Sprite LoadSpriteFromTexture(Texture2D t, float pixelsPerUnit = 100f)
        {
            return Sprite.Create(t, new Rect(0f, 0f, (float)t.width, (float)t.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }

        
        public static Texture2D LoadTextureRaw(byte[] data)
        {
            if (data != null && data.Length != 0)
            {
                Texture2D texture2D = new Texture2D(2, 2);
                if (texture2D.LoadImage(data))
                {
                    return texture2D;
                }
            }
            return null;
        }

        
        public static Sprite LoadSpriteFromResource(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            Sprite sprite;
            if (TextureUtilities._spriteCache.TryGetValue(path, out sprite) && sprite != null)
            {
                return sprite;
            }
            if (path.StartsWith("#"))
            {
                string spriteName = path.Substring(1);
                Sprite sprite2 = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault<Sprite>((Sprite x) => x.name == spriteName);
                if (sprite2 != null)
                {
                    TextureUtilities._spriteCache[path] = sprite2;
                }
                return sprite2;
            }
            Texture2D texture2D = TextureUtilities.LoadTextureRaw(Readers.ReadResource(path));
            if (texture2D != null)
            {
                Sprite sprite3 = TextureUtilities.LoadSpriteFromTexture(texture2D, 100f);
                TextureUtilities._spriteCache[path] = sprite3;
                return sprite3;
            }
            return null;
        }

        
        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
    }
    

    
}
