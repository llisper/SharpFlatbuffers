using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace FlatBuffers
{
    public static class OffsetArrayPool
    {
        #region array type 
        public class Array<T> where T : class
        {
            public Offset<T>[] offsets;
            public int position;
            public readonly int length;

            internal Array(int length)
            {
                offsets = new Offset<T>[length];
                position = 0;
                this.length = length;
            }

            public void Clear()
            {
                position = 0;
            }
        }
        #endregion array type 

        #region internal
        internal const int minSizePOT = 2; // min size => pow(2, minSizePOT) == 4
        internal const int maxSizePOT = 6; // max size => pow(2, maxSizePOT) == 64
        internal const int initialArraySize = 2;
        internal static Dictionary<Type, IList> sArrays = new Dictionary<Type, IList>();

        internal static int RoundupPOT(int size)
        {
            return Helpers.RoundupPOT(size, minSizePOT, maxSizePOT);
        }

        internal static List<Array<T>> GetList<T>() where T : class
        {
            Type type = typeof(T);
            IList list;
            if (!sArrays.TryGetValue(type, out list))
                list = Add(type);

            return (List<Array<T>>)list;
        }

        internal static IList Add(Type type)
        {
            IList list;
            if (!sArrays.TryGetValue(type, out list))
            {
                // initial content: 4,8,16,32,64 <= each size reserved 2 arrays
                Type arrayType = typeof(Array<>).MakeGenericType(type);
                Type listType = typeof(List<>).MakeGenericType(arrayType);
                ConstructorInfo ci = arrayType.GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new Type[] { typeof(int) },
                    null);
                list = (IList)Activator.CreateInstance(listType);
                if (null != ci)
                {
                    for (int i = minSizePOT; i <= maxSizePOT; ++i)
                    {
                        for (int j = 0; j < initialArraySize; ++j)
                        {
                            object array = ci.Invoke(new object[] { 1 << i });
                            list.Add(array);
                        }
                    }
                }
                sArrays.Add(type, list);
            }
            return list;
        }
        #endregion internal

        #region public
        public static Array<T> Alloc<T>(int size) where T : class
        {
            if (size <= 0)
                throw new ArgumentException("size <= 0");

            size = RoundupPOT(size);
            List<Array<T>> list = GetList<T>();
            for (int i = 0; i < list.Count; ++i)
            {
                Array<T> array = list[i];
                if (array.length == size)
                {
                    list.RemoveAt(i);
                    return array;
                }
            }

            for (int i = 0; i < initialArraySize - 1; ++i)
            {
                Array<T> array = new Array<T>(size);
                list.Add(array);
            }

            return new Array<T>(size);
        }

        public static void Dealloc<T>(ref Array<T> array) where T : class
        {
            if (null == array)
                throw new ArgumentNullException("array");

            List<Array<T>> list = GetList<T>();
            array.Clear();
            list.Add(array);
            array = null;
        }
        #endregion public
    }
}
