using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.UGUIWorldImage
{
    public enum RenderTextureSize
    {
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _768 = 768,
        _1024 = 1024,
        _1536 = 1536,
        _2048 = 2048
    }

    public static class RenderTextureSizeUtils
    {
        static List<int> s_SizeValues;

        static List<int> getSizeValuesInPixels()
        {
            if (s_SizeValues == null)
            {
                var iter = Enum.GetValues(typeof(RenderTextureSize));
                s_SizeValues = new List<int>();
                foreach (var value in iter)
                {
                    s_SizeValues.Add((int)value);
                }
            }

            return s_SizeValues;
        }

        public static int SizeToPixels(RenderTextureSize size)
        {
            return (int) size; 
        }

        public static int GetClosestSize(float size)
        {
            var sizes = getSizeValuesInPixels();
            float delta = float.MaxValue;
            int closest = 0;
            for (int i = 0; i < sizes.Count; i++)
            {
                float localDelta = Mathf.Abs(sizes[i] - size);
                if (localDelta < delta)
                {
                    delta = localDelta;
                    closest = i;
                }
            }

            return sizes[closest];
        }

        public static Vector2Int GetSnappedSizeInPixels(RenderTextureSize maxSideLength, float imageWidth, float imageHeight)
        {
            int maxSideLengthInPixels = SizeToPixels(maxSideLength);

            if (imageWidth > imageHeight)
            {
                float otherSide = (imageHeight / imageWidth) * maxSideLengthInPixels;
                int closestOtherSide = GetClosestSize(otherSide);
                return new Vector2Int(maxSideLengthInPixels, closestOtherSide);
            }
            else if (imageWidth < imageHeight)
            {
                float otherSide = (imageWidth / imageHeight) * maxSideLengthInPixels;
                int closestOtherSide = GetClosestSize(otherSide);
                return new Vector2Int(closestOtherSide, maxSideLengthInPixels);
            }
            else
            {
                return new Vector2Int(maxSideLengthInPixels, maxSideLengthInPixels);
            }
        }
    }
}