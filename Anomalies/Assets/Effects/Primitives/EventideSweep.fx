// Adapted from MEAC/Effects/Trail.fx and its inner/outer vertex-strip rendering.
// Coordinates are local to the world-space spear arc; no screen-aspect correction is needed.
float4x4 TransformMatrix;
float Time;
texture Shape;
sampler _ShapeSampler = sampler_state
{
    Texture = <Shape>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

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
    float2 uv = input.UV;
    float shapeValue = tex2D(_ShapeSampler, uv).r;
    float radial = pow(saturate(1.0 - uv.y), 0.55);
    // 只淡出拖尾，不能把位于 x=1 的矛尖前沿也裁透明。
    float tail = smoothstep(0.0, 0.18, uv.x);
    float rim = exp(-uv.y * 42.0);
    float core = exp(-pow((uv.y - 0.10) / 0.075, 2.0));
    float flow = uv.x * 29.0 + uv.y * 19.0 - Time * 18.0;
    float filaments = pow(saturate(sin(flow) * 0.5 + 0.5), 12.0)
        * radial * (0.3 + shapeValue * 0.7);
    float leading = exp(-(1.0 - uv.x) * 24.0) * exp(-uv.y * 12.0);
    float3 orange = float3(1.0, 0.18, 0.012);
    float3 gold = float3(1.0, 0.62, 0.045);
    float3 white = float3(1.0, 0.94, 0.76);
    // 金刃、橙色刀身和沿挥动方向流动的金色纹理；白热只集中在刃口与前锋。
    float goldBand = exp(-pow((uv.y - 0.22) / 0.16, 2.0));
    float3 colorValue = lerp(orange, gold, saturate(radial * radial * 0.65 + goldBand * 0.45));
    colorValue = lerp(colorValue, orange, filaments * 0.35);
    colorValue = lerp(colorValue, white, saturate(rim * 0.7 + leading * 0.35));
    float3 radiance = colorValue * (0.55 + shapeValue * 0.45)
        + gold * (core * 0.25 + filaments * 0.45 + goldBand * 0.12)
        + white * (rim * 0.5 + leading * 0.45);
    float opacity = saturate(radial * (0.22 + shapeValue * 0.5) + rim * 0.45
        + core * 0.25 + filaments * 0.2 + leading * 0.3) * tail * input.Color.a;
    return float4(radiance * input.Color.rgb, opacity);
}
technique Technique1
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 Transform();
        PixelShader = compile ps_3_0 Main();
    }
}
