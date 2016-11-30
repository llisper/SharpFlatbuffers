using System;
using System.Collections.Generic;

namespace FlatBuffers
{
    public static class InstancePool
    {
        internal static Dictionary<Type, Table> sInstances = new Dictionary<Type, Table>();

        internal static Table Add(Type type)
        {
            if (!type.IsSubclassOf(typeof(Table)))
                throw new ArgumentException(type.Name + " is not a FlatBuffers.Table");

            Table table = (Table)Activator.CreateInstance(type);
            sInstances.Add(type, table);
            return table;
        }

        public static T Get<T>() where T : Table
        {
            Type type = typeof(T);
            Table table;
            if (!sInstances.TryGetValue(type, out table))
                table = Add(type);
            return (T)table;
        }
    }
}
