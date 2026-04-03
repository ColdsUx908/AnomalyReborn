using System.Collections.Concurrent;
using System.Dynamic;

namespace Transoceanic.DataStructures;

public sealed class StaticTypeWrapper : DynamicObject
{
    private readonly Type _targetType;

    private static readonly ConditionalWeakTable<Type, TypeCache> _typeCache = [];

    public StaticTypeWrapper() : this(typeof(object)) { }

    public StaticTypeWrapper(Type targetType) => _targetType = targetType ?? throw new ArgumentNullException(nameof(targetType));

    public static StaticTypeWrapper Create(Type targetType) => new(targetType);
    public static StaticTypeWrapper Create<T>() => new(typeof(T));

    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        result = null;

        try
        {
            //查找匹配的方法
            if (TryInvokeMethod(binder.Name, args, out result, binder.CallInfo))
                return true;

            //查找属性并调用其getter
            if (args.Length == 0 && TryGetProperty(binder.Name, out PropertyInfo property) && property?.GetMethod?.IsStatic == true)
            {
                result = property.GetValue(null);
                return true;
            }

            //查找索引器
            if (TryGetIndexer(binder.Name, args, out result))
                return true;

            return false;
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to invoke member '{binder.Name}' on type {_targetType}", ex);
        }
    }

    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        result = null;

        try
        {
            //查找静态属性
            if (TryGetProperty(binder.Name, out PropertyInfo property) && property?.GetMethod?.IsStatic == true)
            {
                result = property.GetValue(null);
                return true;
            }

            //查找静态字段
            if (TryGetField(binder.Name, out FieldInfo field) && field?.IsStatic == true)
            {
                result = field.GetValue(null);
                return true;
            }

            //查找事件
            if (TryGetEvent(binder.Name, out EventInfo eventInfo) && eventInfo?.GetAddMethod(true)?.IsStatic == true)
            {
                //返回一个事件访问器包装器
                result = new StaticEventAccessor(eventInfo);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get member '{binder.Name}' on type {_targetType}", ex);
        }
    }

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        try
        {
            //设置静态属性
            if (TryGetProperty(binder.Name, out PropertyInfo property) && property?.SetMethod?.IsStatic == true)
            {
                object convertedValue = ConvertValue(value, property.PropertyType);
                property.SetValue(null, convertedValue);
                return true;
            }

            //设置静态字段
            if (TryGetField(binder.Name, out FieldInfo field) && field?.IsStatic == true)
            {
                object convertedValue = ConvertValue(value, field.FieldType);
                field.SetValue(null, convertedValue);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set member '{binder.Name}' on type {_targetType}", ex);
        }
    }

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        result = null;

        try
        {
            //查找静态索引器
            PropertyInfo[] indexers = GetIndexers();
            foreach (PropertyInfo indexer in indexers)
            {
                if (indexer.GetMethod?.IsStatic == true && ParametersMatch(indexer.GetIndexParameters(), indexes))
                {
                    result = indexer.GetValue(null, indexes);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get index on type {_targetType}", ex);
        }
    }

    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
    {
        try
        {
            //查找静态索引器
            PropertyInfo[] indexers = GetIndexers();
            foreach (PropertyInfo indexer in indexers)
            {
                if (indexer.SetMethod?.IsStatic == true && ParametersMatch(indexer.GetIndexParameters(), indexes))
                {
                    object convertedValue = ConvertValue(value, indexer.PropertyType);
                    indexer.SetValue(null, convertedValue, indexes);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to set index on type {_targetType}", ex);
        }
    }

    public override bool TryConvert(ConvertBinder binder, out object result)
    {
        //如果请求转换为目标类型，返回目标类型本身
        if (binder.Type == _targetType)
        {
            result = _targetType;
            return true;
        }

        result = null;
        return false;
    }

    private bool TryInvokeMethod(string methodName, object[] args, out object result, CallInfo callInfo)
    {
        result = null;

        //从缓存获取方法
        TypeCache cache = GetOrCreateTypeCache();
        MethodInfo[] methods = cache.GetMethods(methodName);

        if (methods.Length == 0)
            return false;

        //优先查找非泛型方法
        foreach (MethodInfo method in methods.Where(m => !m.IsGenericMethodDefinition))
        {
            if (ParametersMatch(method, args, callInfo, out object[] convertedArgs))
            {
                result = method.Invoke(null, convertedArgs);
                return true;
            }
        }

        //尝试泛型方法
        foreach (MethodInfo method in methods.Where(m => m.IsGenericMethodDefinition))
        {
            if (TryMatchGenericMethod(method, args, callInfo, out MethodInfo constructedMethod, out object[] convertedArgs))
            {
                result = constructedMethod.Invoke(null, convertedArgs);
                return true;
            }
        }

        return false;
    }

    private static bool TryMatchGenericMethod(MethodInfo genericMethod, object[] args, CallInfo callInfo, out MethodInfo constructedMethod, out object[] convertedArgs)
    {
        constructedMethod = null;
        convertedArgs = null;

        try
        {
            //获取泛型参数
            Type[] genericParams = genericMethod.GetGenericArguments();
            Type[] typeArgs = new Type[genericParams.Length];

            //简单的类型推断：从参数类型推断泛型参数
            ParameterInfo[] parameters = genericMethod.GetParameters();
            for (int i = 0; i < Math.Min(parameters.Length, args.Length); i++)
            {
                if (args[i] is not null)
                {
                    Type paramType = parameters[i].ParameterType;
                    if (paramType.IsGenericParameter)
                    {
                        int index = Array.IndexOf(genericParams, paramType);
                        if (index >= 0 && typeArgs[index] is null)
                            typeArgs[index] = args[i].GetType();
                    }
                    else if (paramType.IsGenericType) //尝试从泛型类型参数中推断
                        InferGenericArguments(paramType, args[i].GetType(), genericParams, typeArgs);
                }
            }

            //检查是否所有泛型参数都已推断
            for (int i = 0; i < typeArgs.Length; i++)
            {
                if (typeArgs[i] is null)
                    typeArgs[i] = typeof(object); // 默认使用object
            }

            //构造泛型方法
            constructedMethod = genericMethod.MakeGenericMethod(typeArgs);

            //转换参数
            if (ParametersMatch(constructedMethod, args, callInfo, out convertedArgs))
                return true;
        }
        catch { } //类型推断失败

        return false;
    }

    private static void InferGenericArguments(Type sourceType, Type argumentType, Type[] genericParams, Type[] typeArgs)
    {
        if (sourceType.IsGenericType && argumentType.IsGenericType &&
            sourceType.GetGenericTypeDefinition() == argumentType.GetGenericTypeDefinition())
        {
            Type[] sourceArgs = sourceType.GetGenericArguments();
            Type[] argumentArgs = argumentType.GetGenericArguments();

            for (int i = 0; i < sourceArgs.Length; i++)
            {
                if (sourceArgs[i].IsGenericParameter)
                {
                    int index = Array.IndexOf(genericParams, sourceArgs[i]);
                    if (index >= 0)
                        typeArgs[index] = argumentArgs[i];
                }
            }
        }
    }

    private static bool ParametersMatch(MethodInfo method, object[] args, CallInfo callInfo, out object[] convertedArgs)
    {
        convertedArgs = null;
        ParameterInfo[] parameters = method.GetParameters();

        //处理命名参数
        if (callInfo is not null && callInfo.ArgumentNames.Count > 0) //重新排序参数以匹配方法参数顺序
            args = ReorderNamedArguments(method, args, callInfo);

        //检查参数数量
        if (!method.HasAttribute<ParamArrayAttribute>() && parameters.Length != args.Length)
            return false;

        //转换参数
        convertedArgs = new object[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            if (i < args.Length)
            {
                if (!CanConvert(args[i], parameters[i].ParameterType, out object convertedValue))
                    return false;

                convertedArgs[i] = convertedValue;
            }
            else if (parameters[i].HasDefaultValue)
                convertedArgs[i] = parameters[i].DefaultValue;
            else if (parameters[i].ParameterType.IsValueType)
                convertedArgs[i] = Activator.CreateInstance(parameters[i].ParameterType);
            else
                convertedArgs[i] = null;
        }

        //处理params参数
        if (method.HasAttribute<ParamArrayAttribute>() && args.Length >= parameters.Length - 1)
        {
            ParameterInfo paramsParameter = parameters[^1];
            Type paramsType = paramsParameter.ParameterType.GetElementType();
            Array paramsArray = Array.CreateInstance(paramsType, args.Length - parameters.Length + 1);

            for (int i = parameters.Length - 1; i < args.Length; i++)
            {
                if (!CanConvert(args[i], paramsType, out object convertedValue))
                    return false;

                paramsArray.SetValue(convertedValue, i - parameters.Length + 1);
            }

            convertedArgs[^1] = paramsArray;
        }

        return true;
    }

    private static bool ParametersMatch(ParameterInfo[] parameters, object[] args)
    {
        if (parameters.Length != args.Length)
            return false;

        for (int i = 0; i < parameters.Length; i++)
        {
            if (!CanConvert(args[i], parameters[i].ParameterType, out _))
                return false;
        }

        return true;
    }

    private static object[] ReorderNamedArguments(MethodInfo method, object[] args, CallInfo callInfo)
    {
        ParameterInfo[] parameters = method.GetParameters();
        object[] result = new object[parameters.Length];
        int argIndex = 0;

        //填充命名参数
        for (int i = 0; i < callInfo.ArgumentNames.Count; i++)
        {
            int paramIndex = FindParameterIndex(parameters, callInfo.ArgumentNames[i]);
            if (paramIndex >= 0 && argIndex < args.Length)
                result[paramIndex] = args[argIndex++];
        }

        //填充剩余的位置参数
        for (int i = 0; i < parameters.Length; i++)
        {
            if (result[i] is null && argIndex < args.Length)
                result[i] = args[argIndex++];
        }

        return result;
    }

    private static int FindParameterIndex(ParameterInfo[] parameters, string name)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            if (string.Equals(parameters[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private static bool CanConvert(object value, Type targetType, out object convertedValue)
    {
        convertedValue = null;

        if (value is null) //null可以赋值给引用类型或可空值类型
            return !targetType.IsValueType || (targetType.IsGenericType &&
                   targetType.GetGenericTypeDefinition() == typeof(Nullable<>));

        Type sourceType = value.GetType();

        //检查是否可以直接赋值
        if (targetType.IsAssignableFrom(sourceType))
        {
            convertedValue = value;
            return true;
        }

        //尝试类型转换
        try
        {
            //处理可空类型
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type underlyingType = Nullable.GetUnderlyingType(targetType);
                object converted = Convert.ChangeType(value, underlyingType);
                convertedValue = Activator.CreateInstance(targetType, converted);
                return true;
            }

            //基本类型转换
            convertedValue = Convert.ChangeType(value, targetType);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static object ConvertValue(object value, Type targetType)
    {
        if (CanConvert(value, targetType, out object convertedValue))
            return convertedValue;

        throw new InvalidCastException($"Cannot convert {value?.GetType().Name ?? "null"} to {targetType.Name}");
    }

    private bool TryGetIndexer(string name, object[] args, out object result)
    {
        result = null;

        //查找名为"Item"的索引器
        if (name == "Item")
            return TryGetIndex(null, args, out result);

        return false;
    }

    private PropertyInfo[] GetIndexers()
    {
        TypeCache cache = GetOrCreateTypeCache();
        return cache.GetIndexers();
    }

    private bool TryGetProperty(string propertyName, out PropertyInfo property)
    {
        TypeCache cache = GetOrCreateTypeCache();
        property = cache.GetProperty(propertyName);
        return property is not null;
    }

    private bool TryGetField(string fieldName, out FieldInfo field)
    {
        TypeCache cache = GetOrCreateTypeCache();
        field = cache.GetField(fieldName);
        return field is not null;
    }

    private bool TryGetEvent(string eventName, out EventInfo eventInfo)
    {
        TypeCache cache = GetOrCreateTypeCache();
        eventInfo = cache.GetEvent(eventName);
        return eventInfo is not null;
    }

    private TypeCache GetOrCreateTypeCache() => _typeCache.GetValue(_targetType, t => new TypeCache(t));

    public static void ClearCache(Type type) => _typeCache.Remove(type);

    private class TypeCache
    {
        private readonly ConcurrentDictionary<string, MethodInfo[]> _methodCache = new();
        private readonly ConcurrentDictionary<string, PropertyInfo> _propertyCache = new();
        private readonly ConcurrentDictionary<string, FieldInfo> _fieldCache = new();
        private readonly ConcurrentDictionary<string, EventInfo> _eventCache = new();
        private PropertyInfo[] _indexers;
        private readonly Type _type;

        public TypeCache(Type type) => _type = type ?? throw new ArgumentNullException(nameof(type));

        public MethodInfo[] GetMethods(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
                return [];

            return _methodCache.GetOrAdd(methodName, name =>
            {
                MethodInfo[] methods = [.. _type.GetMethods(TOReflectionUtils.StaticBindingFlags).Where(m => string.Equals(m.Name, name, StringComparison.Ordinal))];
                Array.Sort(methods, (a, b) => a.GetParameters().Length.CompareTo(b.GetParameters().Length)); //按参数数量排序，方便匹配
                return methods;
            });
        }

        public PropertyInfo GetProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;

            return _propertyCache.GetOrAdd(propertyName, name =>
                _type.GetProperty(name, TOReflectionUtils.StaticBindingFlags));
        }

        public FieldInfo GetField(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return null;

            return _fieldCache.GetOrAdd(fieldName, name =>
                _type.GetField(name, TOReflectionUtils.StaticBindingFlags));
        }

        public EventInfo GetEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return null;

            return _eventCache.GetOrAdd(eventName, name =>
                _type.GetEvent(name, TOReflectionUtils.StaticBindingFlags));
        }

        public PropertyInfo[] GetIndexers()
        {
            _indexers ??= [.. _type.GetProperties(TOReflectionUtils.StaticBindingFlags).Where(p => p.GetIndexParameters().Length > 0)];
            return _indexers;
        }
    }

    private class StaticEventAccessor : DynamicObject
    {
        private readonly EventInfo _eventInfo;

        public StaticEventAccessor(EventInfo eventInfo) => _eventInfo = eventInfo ?? throw new ArgumentNullException(nameof(eventInfo));

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;

            if (args.Length == 1 && args[0] is Delegate handler)
            {
                switch (binder.Name.ToLowerInvariant())
                {
                    case "add":
                    case "subscribe":
                        _eventInfo.AddEventHandler(null, handler);
                        return true;
                    case "remove":
                    case "unsubscribe":
                        _eventInfo.RemoveEventHandler(null, handler);
                        return true;
                    default: //尝试通过反射调用方法
                        MethodInfo method = _eventInfo.GetType().GetMethod(binder.Name,
                            TOReflectionUtils.StaticBindingFlags);

                        if (method is not null)
                        {
                            method.Invoke(_eventInfo, [handler]);
                            return true;
                        }
                        break;
                }
            }

            return false;
        }
    }
}