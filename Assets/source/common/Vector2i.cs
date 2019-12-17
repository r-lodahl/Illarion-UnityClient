using System;

namespace Illarion.Client.Common
{
    /// <summary>
    /// Integer based Vector2 class
    /// </summary>
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