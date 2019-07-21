using System;

namespace Illarion.Client.Map
{
    [Serializable]
    public class MapObjectBase
    {
        public float OffsetX {get;}
        public float OffsetY {get;}

        public MapObjectBase(float offsetX, float offsetY)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
}