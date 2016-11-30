using System;

namespace FlatBuffers
{
    public static class Helpers
    {
        public static VectorOffset SetVector<T>(FlatBufferBuilder builder, OffsetArrayPool.Array<T> array) where T : class
        {
            for (int i = array.position - 1; i >= 0; --i)
                builder.AddOffset(array.offsets[i].Value);
            return builder.EndVector();
        }

        internal static int RoundupPOT(int size, int minSizePOT, int maxSizePOT)
        {
            for (int i = minSizePOT; i <= maxSizePOT; ++i)
            {
                int pot = (1 << i);
                if (size <= pot)
                    return pot;
            }
            throw new ArgumentOutOfRangeException("size", size, "max size(pot) allowed is " + (1 << maxSizePOT));
        }
    }
}
