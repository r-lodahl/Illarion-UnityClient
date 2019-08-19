using System;

namespace Illarion.Client.Map
{
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

        public MapObjectBase(float red, float green, float blue, float alpha, float sizeVariance, int encodedLight)
        {
            ColorRed = red;
            ColorGreen = green;
            ColorBlue = blue;
            ColorAlpha = alpha;

            SizeVariance = sizeVariance;

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