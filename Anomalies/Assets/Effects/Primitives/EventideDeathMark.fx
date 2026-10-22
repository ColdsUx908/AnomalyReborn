float4x4 TransformMatrix;
float Time;
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
    // 暗紫色断环，像封印包裹四角魂星。
    float angle = atan2(p.y, p.x);
    float ring = exp(-pow((radius - 0.49) / 0.025, 2.0))
        * smoothstep(0.3, 0.8, abs(sin(angle * 2.0 + Time * 0.12)));
    float white = (1.0 - smoothstep(0.18, 0.82, shape)) * 0.9 + rim * 0.65;
    float3 color = float3(0.11, 0.008, 0.25) + float3(0.42, 0.055, 0.83) * (glow + ring)
        + float3(0.95, 0.88, 1.0) * white;
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
