using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Test")]

namespace FlatBuffers
{
    public static class FlatBuffersInitializer
    {
        static bool initialized = false;

        public static void Initialize(Assembly protocolAssembly)
        {
            if (initialized)
                throw new InvalidOperationException("FlatBuffersInitializer.Initialize() can be called only once");

            foreach (Type type in protocolAssembly.GetExportedTypes())
            {
                if (type.IsSealed && type.IsSubclassOf(typeof(Table)))
                {
                    InstancePool.Add(type);
                    MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                    for (int i = 0; i < methods.Length; ++i)
                    {
                        MethodInfo m = methods[i];
                        if (m.ReturnType == typeof(VectorOffset))
                        {
                            ParameterInfo[] pis = m.GetParameters();
                            if (pis.Length == 2)
                            {
                                Type offsetType = OffsetType(pis[1]);
                                if (null != offsetType)
                                    OffsetArrayPool.Add(offsetType);
                            }
                        }
                    }
                }
            }
            ByteBufferPool.Initialize();
            initialized = true;
        }

        static Type OffsetType(ParameterInfo pi)
        {
            Type type = pi.ParameterType;
            if (type.IsArray)
            {
                Type eleType = type.GetElementType();
                if (eleType.IsGenericType)
                {
                    Type[] genericTypeArgs = eleType.GetGenericArguments();
                    return genericTypeArgs.Length == 1 ? genericTypeArgs[0] : null;
                }
            }
            return null;
        }
    }
}
