sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float2 uScreenResolution;
float2 uPosition;
float uTime;
float uPhase2Fade;

float3 GetColor(float2 screenCoords)
{
    float3 colorBase = float3(0.95, 0.95, 0.9);
    float3 colorTop = float3(0.95, 0.95, 0.95);
    
    float coords = sin(screenCoords.x * 30 + uTime * 2.4) * 0.5 + 0.5;
    float txt = tex2D(uImage1, screenCoords * float2(1.5, 3) + float2(uTime * -0.07, uTime * -0.12)).r;
    float txtVal = pow(max(txt, 0), 1.2);
    float3 gradientColor = lerp(colorBase, colorTop, coords) * min(1.4, 0.75 + txtVal * 0.7);
    float stripeMask = coords * coords;
    float3 goldColor = float3(1.0, 0.75, 0.16);
    gradientColor = lerp(gradientColor, goldColor, stripeMask * uPhase2Fade * 0.9);
    return gradientColor;
}

float4 Main(float4 screenPos : VPOS, float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    float2 screenCoords = (screenPos.xy - uPosition) / uScreenResolution;
    // Preserve the 16:9 pattern scale while removing viewport aspect distortion.
    screenCoords.x *= (uScreenResolution.x / uScreenResolution.y) / (16.0 / 9.0);
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
