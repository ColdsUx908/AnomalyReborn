using Transoceanic.DataStructures;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension<T>(ref Union32 union) where T : unmanaged
    {
        public void SetValue(T value)
        {
            if (Unsafe.SizeOf<T>() != 4)
                throw new InvalidOperationException($"Type {typeof(T)} must be 4 bytes in size.");

            union.f = Unsafe.As<T, float>(ref value);
        }
    }

    extension<T>(ref Union64 union) where T : unmanaged
    {
        public void SetValue(T value)
        {
            if (Unsafe.SizeOf<T>() != 8)
                throw new InvalidOperationException($"Type {typeof(T)} must be 8 bytes in size.");

            union.d = Unsafe.As<T, double>(ref value);
        }
    }
}

