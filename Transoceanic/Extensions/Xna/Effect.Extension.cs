namespace Transoceanic.Extensions.Xna;

/// <summary>
/// 提供向量、颜色、矩形、绘制和 Effect 参数扩展。
/// </summary>
public static partial class TOXnaExtensions
{
    /// <summary>
    /// 按名称设置着色器参数并返回原 Effect，支持连续调用；参数不存在时跳过设置。
    /// </summary>
    extension(Effect effect)
    {
        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, bool value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, bool[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, int value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, int[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, float value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, float[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, Matrix value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, Matrix[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, Quaternion value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, Quaternion[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, Vector2 value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, Vector2[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, Vector3 value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, Vector3[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, Vector4 value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, Vector4[] value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, Texture value)
        {
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。 当前图形后端的字符串 SetValue 在参数存在时抛出 NotImplementedException。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 Effect 实例，可继续设置其他参数。</returns>
        public Effect SetCustomParameter(string name, string value)
        {
            // 原 SetValue(string) 抛出 NotImplementedException，保持相同行为
            effect.Parameters[name]?.SetValue(value);
            return effect;
        }
    }
}

