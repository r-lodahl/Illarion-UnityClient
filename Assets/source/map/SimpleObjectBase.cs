using System;

namespace Illarion.Client.Map
{
    /// <summary>
    /// Object base for map object that only have an offset and id
    /// </summary>
    [Serializable]
    public class SimpleObjectBase : MapObjectBase
    {
        public float[] Offset {get;}
        public int SpriteId {get;}

        public SimpleObjectBase(int spriteId, float[] offset, float red, float green, float blue, float alpha, float sizeVariance, int encodedLight, float height) : base(red, green, blue, alpha, sizeVariance, encodedLight, height)
        {
            SpriteId = spriteId;
            Offset = offset;
        }
    }
}