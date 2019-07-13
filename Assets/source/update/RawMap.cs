using System.Collections.Generic;
using Illarion.Client.Map;
using Illarion.Client.Common;

namespace Illarion.Client.Update
{
    public class RawMap
    {
        public int Layer {get;set;}
        public int StartX {get;set;}
        public int StartY {get;set;}
        public int Width {get;set;}
        public int Height {get;set;}

        public int[,] MapArray {get;set;}

        public Dictionary<Vector2i, MapObject[]> Items {get;set;}
        public Dictionary<Vector2i, Vector3i> Warps {get;set;}
    }
}
