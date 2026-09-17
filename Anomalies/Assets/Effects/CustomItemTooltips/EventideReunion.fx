sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float2 uScreenResolution;
float2 uPosition;
float uTime;

float3 GetColor(float2 screenCoords)
{
    float3 colorBase = float3(0.95, 0.55, 0);
    float3 colorTop = float3(0.95, 0.4, 0);
    
    float2 coords = screenCoords + float2(sin(screenCoords.x * 30 + uTime * 2.4), 0.0);
    float3 gradientColor = lerp(colorBase, colorTop, coords.x);
    
    return gradientColor;
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
