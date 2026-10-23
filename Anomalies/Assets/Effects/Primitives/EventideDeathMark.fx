float4x4 TransformMatrix;
float Time;
float CompanionPresence;
struct VertexInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};
struct PixelInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};
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
    // 凹弧四角星，而非矩形十字；连续缩放留出光晕空间。
    float breath = 0.78 + 0.065 * sin(Time * 2.8);
    float2 p = (input.UV - 0.5) * 2.0 / breath;
    float shape = pow(abs(p.x) + 0.0001, 0.55) + pow(abs(p.y) + 0.0001, 0.55);
    float body = 1.0 - smoothstep(0.78, 1.04, shape);
    float rim = exp(-pow((shape - 0.78) / 0.14, 2.0));
    float glow = exp(-max(0.0, shape - 0.78) * 5.5);
    float radius = length(p);
    // 四段封印环始终关于水平、竖直轴对称；不使用随时间旋转的角相位。
    float diagonal = abs(2.0 * p.x * p.y) / max(dot(p, p), 0.0001);
    float ring = exp(-pow((radius - 0.49) / 0.025, 2.0))
        * smoothstep(0.3, 0.8, diagonal) * (0.85 + 0.1 * sin(Time * 2.8));
    // 扩大冷白亮芯，外围仍保留深紫主体与冷紫刃缘。
    float core = (1.0 - smoothstep(0.16, 0.72, shape)) * 1.1;
    // 与外侧轮回齿轮共存时收敛外围雾光，保留四角星亮芯及轴对称。
    glow *= lerp(1.0, 0.62, CompanionPresence);
    ring *= lerp(1.0, 0.72, CompanionPresence);
    float3 color = float3(0.03, 0.001, 0.07) + float3(0.23, 0.008, 0.48) * body
        + float3(0.20, 0.015, 0.45) * (glow * 0.7 + ring * 0.9)
        + float3(0.42, 0.12, 0.7) * rim * 0.55
        + float3(1.0, 0.97, 1.0) * core;
    float bounds = 1.0 - smoothstep(0.86, 1.0, length((input.UV - 0.5) * 2.0));
    float alpha = saturate(body * 0.8 + glow * 0.32 + ring * 0.45) * bounds;
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
