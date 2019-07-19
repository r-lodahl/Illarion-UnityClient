using System;

namespace Illarion.Client.Common
{
    [Serializable]
    public struct Vector3i
    {
        public readonly int x;
        public readonly int y;
        public readonly int z;
        
        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}