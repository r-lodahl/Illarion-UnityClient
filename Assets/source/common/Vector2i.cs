using System;

namespace Illarion.Client.Common
{
    [Serializable]
    public struct Vector2i
    {
        public readonly int x;
        public readonly int y;
        
        public Vector2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}