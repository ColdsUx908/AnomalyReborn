sampler uImage0 : register(s0);
float2 uDirection;
float uIntensity;

// Average the original and ten scaled copies, matching the HTML compositing sequence.
float4 Main(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, uv);
    [unroll]
    for (int i = 1; i <= 10; i++)
    {
        float scale = 1.0 + uIntensity * (float(i) / 10.0);
        float2 sampleUv = uDirection + (uv - uDirection) / scale;
        color += tex2D(uImage0, saturate(sampleUv));
    }
    return color / 11.0;
}

technique Technique1
{
    pass Pass0 { PixelShader = compile ps_3_0 Main(); }
}
