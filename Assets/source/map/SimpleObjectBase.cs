using System;

namespace Illarion.Client.Map
{
    [Serializable]
    public class SimpleObjectBase : MapObjectBase
    {
        public float OffsetX {get;}
        public float OffsetY {get;}

        public SimpleObjectBase(float offsetX, float offsetY, float red, float green, float blue, float alpha, float sizeVariance, int encodedLight) : base(red, green, blue, alpha, sizeVariance, encodedLight)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
}