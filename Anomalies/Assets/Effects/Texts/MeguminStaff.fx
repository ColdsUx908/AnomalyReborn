sampler uImage0 : register(s0);
float uTime;
float2 uTextOrigin;
float uTextScale;
float uPalette;

float4 Main(float4 screenPos : VPOS, float2 uv : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    float2 p = (screenPos.xy - uTextOrigin) / max(uTextScale, 0.01);
    float wave = sin(p.x * 0.028 + p.y * 0.012 - uTime * 2.0) * 0.5 + 0.5;
    float3 ruby = float3(1.0, 0.19, 0.25);
    float3 ember = float3(1.0, 0.48, 0.13);
    float3 gold = float3(1.0, 0.91, 0.57);
    float3 color = lerp(ruby, ember, smoothstep(0.0, 0.52, wave));
    color = lerp(color, gold, smoothstep(0.5, 1.0, wave));
    if (uPalette > 0.5)
    {
        float loreWave = sin(p.x * 0.016 - p.y * 0.024 + uTime * 1.45 + 1.3) * 0.5 + 0.5;
        color = lerp(float3(0.78, 0.57, 1.0), float3(1.0, 0.59, 0.76), smoothstep(0.0, 0.6, loreWave));
        color = lerp(color, float3(1.0, 0.91, 0.79), smoothstep(0.52, 1.0, loreWave));
    }
    float alpha = tex2D(uImage0, uv).a;
    return float4(color, 1.0) * alpha * sampleColor;
}

technique Technique1
{
    pass Pass0 { PixelShader = compile ps_3_0 Main(); }
}
