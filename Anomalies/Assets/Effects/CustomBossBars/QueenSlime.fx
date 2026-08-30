sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float2 uScreenResolution;
float2 uPosition;
float uTime;

float3 GetColor(float2 screenCoords)
{
    float3 colorBase = float3(0.36, 0.49, 0.7);
    float3 colorTop = float3(0.72, 0.24, 0.4);
    float txt1 = tex2D(uImage1, screenCoords * float2(7.2, 13.8) + float2(uTime * -0.25, uTime * -0.1)).r;
    float txt2 = tex2D(uImage1, screenCoords * float2(5.2, 10.8) + float2(uTime * -0.25, uTime * -0.1)).r;
    float txt = txt1 * 0.7 + pow(txt2, 0.4) * 0.6;

    float3 base = lerp(colorBase, colorTop, sin(screenCoords.x * 22.0 + uTime * -2.0) * 0.45);
    base *= 0.6 + txt * 0.7;
    base += txt1 * 0.3;
    base *= 1.7 - txt2 * 0.85 - txt1 * 0.5;
    base = clamp(base * 1.1, 0.25, 1.2);
    return base;
}

float4 Main(float4 screenPos : VPOS, float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    float2 screenCoords = (screenPos.xy - uPosition) / uScreenResolution;
    float alpha = tex2D(uImage0, coords).a;
    float3 gradientColor = GetColor(screenCoords);
    return float4(gradientColor, 1.0) * alpha * sampleColor;
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}