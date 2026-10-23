float4x4 TransformMatrix;
float StartDistance;
float EndDistance;
float SpearMode;
struct VertexInput { float4 Position : POSITION0; float4 Color : COLOR0; float2 UV : TEXCOORD0; };
struct PixelInput { float4 Position : POSITION0; float4 Color : COLOR0; float2 UV : TEXCOORD0; };
PixelInput Transform(VertexInput input)
{
    PixelInput output;
    output.Position = mul(input.Position, TransformMatrix);
    output.Color = input.Color;
    output.UV = input.UV;
    return output;
}
float4 Main(PixelInput input) : COLOR0
{
    if (SpearMode > 0.5)
    {
        float acrossSpear = abs(input.UV.y * 2.0 - 1.0);
        float body = exp(-acrossSpear * acrossSpear * 5.0);
        float coreSpear = exp(-acrossSpear * acrossSpear * 18.0);
        float rimGlow = exp(-acrossSpear * acrossSpear * 3.5) * 0.2;
        // 光晕在网格边界前平滑归零，首尾同时淡出，隐藏矛形折点与尖端截面。
        float edgeFade = 1.0 - smoothstep(0.45, 1.0, acrossSpear);
        float endFade = smoothstep(0.0, 45.0, input.UV.x - StartDistance)
            * smoothstep(0.0, 75.0, EndDistance - input.UV.x);
        float alphaSpear = (body * 0.45 + coreSpear * 0.4 + rimGlow) * edgeFade * endFade * input.Color.a;
        float3 spearColor = lerp(input.Color.rgb, float3(0.35, 1.0, 0.96), coreSpear * 0.22);
        // 保留青色自发光，避免半透明填充在明亮背景上只剩一层难辨的薄雾。
        // AlphaBlend 下输出较低遮挡 alpha，同时保留完整发光颜色。
        return float4(spearColor * alphaSpear, alphaSpear * 0.35);
    }
    // 六像素半宽的顶点带：约 2.5 像素亮线，外侧仅保留低强度柔光。
    float across = abs(input.UV.y * 2.0 - 1.0);
    float along = min(input.UV.x - StartDistance, EndDistance - input.UV.x);
    // 回收端与矛尖均为柔和圆帽，不能把六像素宽的四边形截面直接暴露出来。
    float cap = max(0.0, 1.5 - along);
    float distance = length(float2(across * 6.0, cap));
    float aa = max(fwidth(distance), 0.08);
    float core = 1.0 - smoothstep(1.0 - aa * 0.5, 1.5 + aa * 0.5, distance);
    float glow = exp(-across * across * 6.0) * 0.15;
    float bounds = 1.0 - smoothstep(0.78, 1.0, across);
    float capFade = smoothstep(0.0, 4.0, along);
    float alpha = (core * 0.78 + glow * capFade) * bounds * input.Color.a;
    float3 color = lerp(input.Color.rgb, float3(0.86, 0.97, 1.0), core * 0.24);
    return float4(color * alpha, alpha);
}
technique Technique1
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 Transform();
        PixelShader = compile ps_3_0 Main();
    }
}
