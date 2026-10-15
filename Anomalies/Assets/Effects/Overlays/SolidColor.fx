sampler uImage0 : register(s0);
float4 uColor;

float4 Main(float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    return float4(uColor.rgb, baseColor.a * uColor.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}