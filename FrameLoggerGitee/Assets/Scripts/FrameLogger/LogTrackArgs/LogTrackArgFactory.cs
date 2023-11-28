using System;
using System.Collections.Generic;
using System.Reflection;

namespace FrameLogger
{
    public class LogTrackArgFactory
    {
        private static readonly Dictionary<Type, Delegate> s_valueTypeArgCreators = new Dictionary<Type, Delegate>();
        private static readonly Dictionary<Type, LogTrackArgAttribute> s_argTypeAttributes = new Dictionary<Type, LogTrackArgAttribute>();
        private static readonly Dictionary<byte, Type> s_argTypeMap = new Dictionary<byte, Type>();

        public static bool IsArgTypeSupported(string argType)
        {
            if (s_argTypeAttributes.Count == 0)
            {
                var types = typeof(LogTrackArgFactory).Assembly.GetTypes();
                foreach (var type in types)
                {
                    var attribute = type.GetCustomAttribute<LogTrackArgAttribute>();
                    if (attribute != null)
                    {
                        s_argTypeAttributes.Add(attribute.argType, attribute);
                    }
                }
            }

            foreach (var trackArgAttribute in s_argTypeAttributes.Values)
            {
                if (trackArgAttribute.systemTypeNames.Contains(argType))
                {
                    return true;
                }
            }

            return false;
        }
        
        public static ILogTrackArg CreateLogTrackArg<T>(T arg)
        {
            Type argType = typeof(T);
            Type returnType = typeof(ILogTrackArg);
            
            if (s_valueTypeArgCreators.Count == 0)
            {
                var types = typeof(LogTrackArgFactory).Assembly.GetTypes();
                foreach (var type in types)
                {
                    var attribute = type.GetCustomAttribute<LogTrackArgAttribute>();
                    if (attribute != null)
                    {
                        var methodInfo = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                        if (methodInfo != null)
                        {
                            Type funcType = typeof(Func<,>).MakeGenericType(attribute.argType, returnType);
                            var delegateMethod = Delegate.CreateDelegate(funcType, null, methodInfo);
                        
                            s_valueTypeArgCreators.Add(attribute.argType, delegateMethod);
                        }
                    }
                }
            }

            if (s_valueTypeArgCreators.TryGetValue(argType, out var creator))
            {
                return ((Func<T, ILogTrackArg>)creator)(arg);
            }
            
            throw new NotSupportedException($"Value type {argType} is not supported.");
        }

        public static ILogTrackArg CreateLogTrackArg(byte typeId, byte[] bytes)
        {
            if (s_argTypeMap.Count == 0)
            {
                var types = typeof(LogTrackArgFactory).Assembly.GetTypes();
                foreach (var type in types)
                {
                    var attribute = type.GetCustomAttribute<LogTrackArgAttribute>();
                    if (attribute != null)
                    {
                        s_argTypeMap[attribute.typeId] = type;
                    }
                }
            }

            if (s_argTypeMap.TryGetValue(typeId, out var argType))
            {
                var arg = Activator.CreateInstance(argType) as ILogTrackArg;
                arg?.Deserialize(bytes);
                
                return arg;
            }

            throw new NotSupportedException($"Value type {typeId} is not supported.");
        }
    }
}