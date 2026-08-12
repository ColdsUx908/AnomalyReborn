using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;

namespace Transoceanic.Framework.Helpers;

public static partial class TOExtensions
{
    extension(MiscShaderData data)
    {
        public MiscShaderData SetCustomParameter(string name, bool value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, bool[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, int value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, int[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, float value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, float[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, Matrix value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, Matrix[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, Quaternion value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, Quaternion[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, Vector2 value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, Vector2[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, Vector3 value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, Vector3[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, Vector4 value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, Vector4[] value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, Texture value)
        {
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }

        public MiscShaderData SetCustomParameter(string name, string value)
        {
            // 原 SetValue(string) 抛出 NotImplementedException，保持相同行为
            data.Shader.Parameters[name]?.SetValue(value);
            return data;
        }
    }
}
