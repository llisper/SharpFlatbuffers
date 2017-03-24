using System;
using System.Collections.Generic;

namespace FlatBuffers
{
    public static class InstancePool
    {
        internal static Dictionary<Type, object> sInstances = new Dictionary<Type, object>();

        internal static object Add(Type type)
        {
            if (!type.IsSubclassOf(typeof(Table)) &&
                !type.IsSubclassOf(typeof(Struct)))
            {
                throw new ArgumentException(type.Name + " is neither a FlatBuffers.Table not a FlatBuffers.Struct");
            }

            object inst = Activator.CreateInstance(type);
            sInstances.Add(type, inst);
            return inst;
        }

        public static T Get<T>()
        {
            Type type = typeof(T);
            object inst;
            if (!sInstances.TryGetValue(type, out inst))
                inst = Add(type);
            return (T)inst;
        }
    }
}
