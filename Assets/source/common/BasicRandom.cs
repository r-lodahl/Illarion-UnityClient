using System;

namespace Illarion.Client.Common
{
    public class BasicRandom
    {
        private const int multiplier = 1664525;
        private const int increment = 1013904223;
        private const int modulus = int.MaxValue;

        private long _seed;
        public long Seed {
            set
            {
                _seed = (value ^ 0x5DEECE66DL) & ((1L << 48) - 1);
            }
        }

        private int Next(int bits) 
        {
            _seed = (_seed * 0x5DEECE66DL + 0xBL) & ((1L << 48) - 1);
            return (int) ((ulong)_seed >> (48 - bits));
        }

        public int NextInt() => Next(32);

        public float NextFloat() => Next(24) / (float) (1 << 24);

        public int NextInt(int n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException("n must be positive");

            if ((n & -n) == n) return (int) ((n * (long) Next(31)) >> 31);

            int bits, val;
            do
            {
                bits = Next(31);
                val = bits % n;
            } while (bits - val + (n- 1) < 0);

            return val;
        }
    }
}