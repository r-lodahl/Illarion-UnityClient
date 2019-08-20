using System;

namespace Illarion.Client.Map
{
    [Serializable]
    public class SimpleObjectBase : MapObjectBase
    {
        public float[] Offset {get;}

        public SimpleObjectBase(float[] offset, float red, float green, float blue, float alpha, float sizeVariance, int encodedLight) : base(red, green, blue, alpha, sizeVariance, encodedLight)
        {
            Offset = offset;
        }
    }
}