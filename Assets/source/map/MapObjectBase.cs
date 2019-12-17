using System;

namespace Illarion.Client.Map
{
    /// <summary>
    /// Data class for all map objects with the same object id 
    /// Contains shared properties
    /// </summary>
    [Serializable]
    public class MapObjectBase
    {
        public float ColorRed {get;}
        public float ColorGreen {get;}
        public float ColorBlue {get;}
        public float ColorAlpha {get;}

        public float SizeVariance {get;}

        public bool IsEmittingLight {get;}
        public Light Light {get;}

        public float Height {get;}

        public MapObjectBase(float red, float green, float blue, float alpha, float sizeVariance, int encodedLight, float height)
        {
            ColorRed = red;
            ColorGreen = green;
            ColorBlue = blue;
            ColorAlpha = alpha;

            SizeVariance = sizeVariance;

            Height = height;

            if (encodedLight == 0) 
            {
                IsEmittingLight = false;
            }
            else
            {
                IsEmittingLight = true;
                Light = new Light(encodedLight);
            }
        }
    }
}