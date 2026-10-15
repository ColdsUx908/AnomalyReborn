sampler uImage0 : register(s0);
float4 uColor;
float uOpacity;

float4 Main(float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    float luminance = dot(baseColor.rgb, float3(0.299, 0.587, 0.114));
    return float4(uColor.rgb, (1 - luminance) * uOpacity);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}