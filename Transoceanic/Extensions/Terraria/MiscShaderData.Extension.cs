using global::Terraria.Graphics.Shaders;

namespace Transoceanic.Extensions.Terraria;

/// <summary>
/// 提供 Terraria 实体、生成、世界与着色器的便捷扩展。
/// </summary>
public static partial class TOTerrariaExtensions
{
    extension(MiscShaderData data)
    {
        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, bool value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, bool[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, int value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, int[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, float value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, float[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, Matrix value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, Matrix[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, Quaternion value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, Quaternion[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, Vector2 value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, Vector2[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, Vector3 value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, Vector3[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, Vector4 value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, Vector4[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, Texture value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        /// <summary>
        /// 按名称设置着色器参数；参数不存在时跳过，类型不匹配等 SetValue 异常原样传播，返回原对象以便连续调用。 当前图形后端的字符串 SetValue 在参数存在时抛出 NotImplementedException。
        /// </summary>
        /// <param name="name">目标 Effect 参数名称；不存在时忽略本次设置。</param>
        /// <param name="value">与目标参数类型和数组维度匹配的值；不进行类型转换。</param>
        /// <returns>原 MiscShaderData 实例，可继续设置其他参数。</returns>
        public MiscShaderData SetCustomParameter(string name, string value)
        {
            // 原 SetValue(string) 抛出 NotImplementedException，保持相同行为
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }
    }
}

