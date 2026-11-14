sampler uImage0 : register(s0);
float2 uBlurStep;
float2 uDownsampleStep;
float uStrength;

float4 Main(float2 uv : TEXCOORD0, uniform float mode) : COLOR0
{
    if (mode > 1.5)
    {
        float4 downsample = 0;
        [unroll]
        for (int y = 0; y < 4; y++)
        {
            [unroll]
            for (int x = 0; x < 4; x++)
                downsample += tex2D(uImage0, uv + (float2(x, y) - 1.5) * uDownsampleStep);
        }
        return downsample / 16.0;
    }
    if (mode > 0.5)
        return float4(saturate(tex2D(uImage0, uv).rgb * uStrength), 0.0);
    float4 sum = 0;
    float weightSum = 0;
    [unroll]
    for (int i = -6; i <= 6; i++)
    {
        float weight = exp(-float(i * i) / 8.0);
        sum += tex2D(uImage0, uv + uBlurStep * i) * weight;
        weightSum += weight;
    }
    return sum / weightSum;
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main(0.0);
    }
    pass Halo
    {
        PixelShader = compile ps_3_0 Main(1.0);
    }
    pass Downsample
    {
        PixelShader = compile ps_3_0 Main(2.0);
    }
}
