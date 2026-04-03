namespace Transoceanic.Framework.Abstractions;

public interface IPublicizer
{
    public static abstract Type C_type { get; }
}

public abstract record Publicizer<T> : IPublicizer
{
    public static Type C_type { get; } = typeof(T);

    /// <inheritdoc cref="PublicizerHandler.GetInstanceField(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FieldInfo GetInstanceField(string name) => PublicizerHandler.GetInstanceField(C_type, name);
    /// <inheritdoc cref="PublicizerHandler.GetInstanceProperty(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PropertyInfo GetInstanceProperty(string name) => PublicizerHandler.GetInstanceProperty(C_type, name);
    /// <inheritdoc cref="PublicizerHandler.GetInstanceMethod(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetInstanceMethod(string name) => PublicizerHandler.GetInstanceMethod(C_type, name);
    /// <inheritdoc cref="PublicizerHandler.GetInstanceMethod(Type, string, Type[])"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetInstanceMethod(string name, Type[] parameterTypes) => PublicizerHandler.GetInstanceMethod(C_type, name, parameterTypes);
    /// <inheritdoc cref="PublicizerHandler.GetInstanceConstructor(Type, Type[])"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConstructorInfo GetInstanceConstructor(Type[] parameterTypes) => PublicizerHandler.GetInstanceConstructor(C_type, parameterTypes);
    /// <inheritdoc cref="PublicizerHandler.GetInstanceEvent(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EventInfo GetInstanceEvent(string name) => PublicizerHandler.GetInstanceEvent(C_type, name);
    /// <inheritdoc cref="PublicizerHandler.GetStaticField(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FieldInfo GetStaticField(string name) => PublicizerHandler.GetStaticField(C_type, name);
    /// <inheritdoc cref="PublicizerHandler.GetStaticProperty(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PropertyInfo GetStaticProperty(string name) => PublicizerHandler.GetStaticProperty(C_type, name);
    /// <inheritdoc cref="PublicizerHandler.GetStaticMethod(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetStaticMethod(string name) => PublicizerHandler.GetStaticMethod(C_type, name);
    /// <inheritdoc cref="PublicizerHandler.GetStaticMethod(Type, string, Type[])"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetStaticMethod(string name, Type[] parameterTypes) => PublicizerHandler.GetStaticMethod(C_type, name, parameterTypes);
    /// <inheritdoc cref="PublicizerHandler.GetStaticConstructor(Type)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConstructorInfo GetStaticConstructor() => PublicizerHandler.GetStaticConstructor(C_type);
    /// <inheritdoc cref="PublicizerHandler.GetStaticEvent(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EventInfo GetStaticEvent(string name) => PublicizerHandler.GetStaticEvent(C_type, name);
}

public abstract record InstancedPublicizer<T>(T Source) : Publicizer<T>;
