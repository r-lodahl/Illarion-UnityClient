using System;

namespace Illarion.Client.Common
{
    /// <summary>
    /// Original functions to determine item and tile variances and item scales
    /// </summary>
    public static class MapVariance
    {
        private static BasicRandom random = new BasicRandom();

        public static int GetItemFrameVariance(int tileX, int tileY, int frameCount)
        {
            random.Seed = ((tileX * 1586337181) + (tileY * 6110869557)) * 3251474107;
            random.NextInt();
            random.NextInt();
            return random.NextInt(frameCount);
        }

        public static float GetItemScaleVariance(int tileX, int tileY, float variance) 
        {
            random.Seed = ((tileX * 1586337181L) + (tileY * 6110869557L)) * 3251474107L;
            random.NextInt();
            random.NextInt();
            random.NextInt();
            random.NextInt();

            return (1.0f - variance) + (2 * random.NextFloat() * variance);
        }

        public static int GetTileFrameVariance(int tileX, int tileY, int frameCount)
        {
            if ((frameCount != 4) && (frameCount != 9) && (frameCount != 16) && (frameCount != 25))
            {
                random.Seed = ((tileX * 5133879561L) + (tileY * 4154745775L)) * 1256671499;
                random.NextInt();
                random.NextInt();
                return random.NextInt(frameCount);
            }

            if (frameCount == 4) {
                return Math.Abs((tileX + 10000) % 2) + (Math.Abs((tileY + 10000) % 2) * 2);
            } else if (frameCount == 9) {
                return Math.Abs((tileX + 10000) % 3) + (Math.Abs((tileY + 10000) % 3) * 3);
            } else if (frameCount == 16) {
                return Math.Abs((tileX + 10000) % 4) + (Math.Abs((tileY + 10000) % 4) * 4);
            } else {
                return Math.Abs((tileX + 10000) % 5) + (Math.Abs((tileY + 10000) % 5) * 5);
            }
        }
    }
}