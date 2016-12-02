using System.Collections.Generic;

namespace FlatBuffers
{
    public static class ByteBufferPool
    {
        #region internal
        internal const int minSizePOT = 6; // min size => pow(2, minSizePOT) == 64
        internal const int maxSizePOT = 14; // max size => pow(2, maxSizePOT) == 16384
        internal const int initialArraySize = 4;

        internal static List<ByteBuffer>[] sPool = new List<ByteBuffer>[maxSizePOT - minSizePOT + 1];
        internal static Dictionary<int, int> sIndexLookup = new Dictionary<int, int>();

        internal static int RoundupPOT(int size)
        {
            return Helpers.RoundupPOT(size, minSizePOT, maxSizePOT);
        }

        internal static void Initialize()
        {
            for (int i = minSizePOT; i <= maxSizePOT; ++i)
            {
                int size = (1 << i);
                int index = i - minSizePOT;
                sIndexLookup[size] = index;
                List<ByteBuffer> pool = new List<ByteBuffer>(initialArraySize);
                for (int j = 0; j < initialArraySize; ++j)
                    pool.Add(new ByteBuffer(new byte[size]));
                sPool[index] = pool;
            }
        }

        internal static int AvailableSlots(List<ByteBuffer> pool)
        {
            int slots = 0;
            for (; slots < pool.Count && null != pool[slots]; ++slots) ;
            return slots;
        }

        internal static void Swap(List<ByteBuffer> pool, int i, int j)
        {
            if (i != j)
            {
                ByteBuffer tmp = pool[i];
                pool[i] = pool[j];
                pool[j] = tmp;
            }
        }

        internal static int EnsureCapacity(List<ByteBuffer> pool, int size)
        {
            int slots = AvailableSlots(pool);
            if (0 == slots)
            {
                int inc = pool.Count;
                pool.Capacity += inc;
                for (int i = 0; i < inc; ++i)
                {
                    pool.Add(new ByteBuffer(new byte[size]));
                    Swap(pool, slots++, pool.Count - 1);
                }
            }
            return slots - 1;
        }
        #endregion internal

        #region public
        public static ByteBuffer Alloc(int size)
        {
            size = RoundupPOT(size);
            List<ByteBuffer> pool = sPool[sIndexLookup[size]];
            int i = EnsureCapacity(pool, size);
            ByteBuffer buffer = pool[i];
            pool[i] = null;
            return buffer;
        }

        public static void Dealloc(ref ByteBuffer buffer)
        {
            if (null == buffer)
                return;

            int size = buffer.Data.Length;
            if (0 == size || 0 != (size & (size - 1)))
                return;

            if (size > (1 << maxSizePOT))
                return;

            List<ByteBuffer> pool = sPool[sIndexLookup[size]];
            int slots = AvailableSlots(pool);
            if (slots < pool.Count)
            {
                buffer.Reset();
                pool[slots] = buffer;
                buffer = null;
            }
        }
        #endregion public
    }
}
