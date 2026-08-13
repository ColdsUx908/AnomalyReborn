sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float2 uScreenResolution;
float2 uPosition;
float uTime;

float3 GetColor(float2 screenCoords)
{
    float3 colorBase = float3(0.85, 0.1, 0.25);
    float3 colorTop = float3(0.7, 0.15, 0.2);
    
    float2 coords = screenCoords + float2(sin(screenCoords.x * 30 + uTime * 2.4), 0.0);
    float txt = tex2D(uImage1, screenCoords * float2(1.5, 3) + float2(uTime * -0.07, uTime * -0.12)).r;
    float txtVal = pow(txt, 1.2);
    float3 gradientColor = lerp(colorBase, colorTop, coords.x) * min(1.4, 0.75 + txtVal * 0.7);
    return gradientColor;
}

float4 PixelShaderFunction(float4 screenPos : VPOS, float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
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
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
