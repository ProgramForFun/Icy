using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.UGUIWorldImage
{
    public class RenderTexturePool
    {
        // Static API
        
        static RenderTexturePool s_mainPool = new RenderTexturePool(maxCapacity: 10);

        public static RenderTexture GetTexture(int width, int height, int depth, RenderTextureFormat renderTextureFormat)
        {
            return s_mainPool.getTextureInternal(width, height, depth, renderTextureFormat);
        }

        public static void ReturnTexture(RenderTexture texture)
        {
            if (texture == null)
                return;

            s_mainPool.returnTextureInternal(texture);
        }

        // Pool class

        public int MaxCapacity;
        public List<RenderTexture> Textures;

        public RenderTexturePool(int maxCapacity)
        {
            MaxCapacity = maxCapacity;
            Textures = new List<RenderTexture>(MaxCapacity);
        }

        RenderTexture getTextureInternal(int width, int height, int depth, RenderTextureFormat renderTextureFormatForNewTexture)
        {
            defrag();
            ShrinkPoolIfNecessary();

            for (int i = Textures.Count-1; i >= 0; i--)
            {
                var texture = Textures[i];
                if (texture.width == width &&
                    texture.height == height &&
                    texture.depth == depth)
                {
                    Textures.RemoveAt(i);
                    return texture;
                }
            }

            var newTexture = new RenderTexture(width, height, depth, renderTextureFormatForNewTexture);
            newTexture.name = "Pooled Render Texture " + UnityEngine.Random.Range(1000, 9999);

            // Debug.Log("New RenderTexture from Pool: " + newTexture.name);

            ShrinkPoolIfNecessary();
            return newTexture;
        }

        private void defrag()
        {
            for (int i = Textures.Count - 1; i >= 0; i--)
            {
                if (Textures[i] == null)
                {
                    Textures.RemoveAt(i);
                }
            }
        }

        public void ShrinkPoolIfNecessary()
        {
            if (Textures.Count > MaxCapacity)
            {
                for (int i = 0; i < Textures.Count - MaxCapacity; i++)
                {
                    Textures[0].Release();
                    Textures.RemoveAt(0);
                }
            }
        }

        public void returnTextureInternal(RenderTexture texture)
        {
            if (texture == null)
                return;

            if (!Textures.Contains(texture))
            {
                Textures.Add(texture);
            }

            ShrinkPoolIfNecessary();
        }
    }
}