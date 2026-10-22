float4x4 TransformMatrix;
float Time;
texture Starfield;
sampler Stars = sampler_state
{
    Texture = <Starfield>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
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
    float2 StarUV : TEXCOORD1;
};
PixelInput Transform(VertexInput input)
{
    PixelInput output;
    output.Position = mul(input.Position, TransformMatrix);
    output.Color = input.Color;
    output.UV = input.UV;
    // 像 MEAC 的 Cosmic 遮罩一样在刀光内部采样星空，以世界像素固定纹理比例。
    output.StarUV = input.Position.xy / 850.0;
    return output;
}
float4 Main(PixelInput input) : COLOR0
{
    float2 uv = input.UV;
    float taper = pow(saturate(sin(uv.x * 3.14159265)), 0.65);
    // 两端使用零斜率淡出；宽度收尖与透明度分别控制，避免突然截断。
    float ends = smoothstep(0.0, 0.20, uv.x) * (1.0 - smoothstep(0.80, 1.0, uv.x));
    float radial = uv.y / max(0.04, taper);
    float edge = exp(-pow((radial - 0.035) / 0.030, 2.0));
    float halo = exp(-radial * 9.0);
    // 内侧用宽渐变取代平方根硬收边，星空也随主体一同淡出。
    float body = 1.0 - smoothstep(0.20, 1.0, radial);
    float outerFade = smoothstep(0.0, 0.025, radial);
    float filaments = pow(saturate(sin(uv.x * 58.0 - radial * 15.0 - Time * 12.0)), 18.0)
        * exp(-radial * 5.0) * ends;
    float3 cosmos = tex2D(Stars, input.StarUV + float2(Time * 0.008, -Time * 0.005)).rgb;
    float luminance = dot(cosmos, float3(0.3, 0.45, 0.25));
    float stars = pow(saturate(luminance * 1.8), 3.0);
    float twinkle = 0.8 + 0.2 * sin(Time * 3.5 + input.StarUV.x * 47.0 + input.StarUV.y * 31.0);
    float3 purple = float3(0.58, 0.09, 0.98);
    float3 nebula = float3(0.12, 0.015, 0.26) + cosmos * float3(0.85, 0.32, 1.35)
        + purple * luminance * 0.6;
    float3 color = nebula + purple * (halo * 1.3 + filaments * 0.7)
        + float3(0.93, 0.64, 1.0) * edge * 1.8
        + float3(0.9, 0.83, 1.0) * stars * twinkle * 1.4;
    float alpha = saturate(body * 0.92 + halo * 0.35) * outerFade * ends * input.Color.a;
    return float4(color * alpha * input.Color.rgb, alpha);
}
technique Technique1
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 Transform();
        PixelShader = compile ps_3_0 Main();
    }
}
