namespace Transoceanic.Extensions.Xna;

public static partial class TOXnaExtensions
{
    /// <summary>
    /// 按名称设置着色器参数并返回原 Effect，支持连续调用；参数不存在时跳过设置。
    /// </summary>
    extension(Effect effect)
    {
        public Effect SetCustomParameter(string name, bool value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, bool[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, int value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, int[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, float value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, float[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, Matrix value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, Matrix[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, Quaternion value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, Quaternion[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, Vector2 value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, Vector2[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, Vector3 value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, Vector3[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, Vector4 value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, Vector4[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, Texture value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        public Effect SetCustomParameter(string name, string value)
        {
            // 原 SetValue(string) 抛出 NotImplementedException，保持相同行为
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }
    }
}

