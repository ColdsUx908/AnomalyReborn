sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
float2 uScreenResolution;
float2 uPosition;
float uTime;

float3 GetColor(float2 screenCoords)
{
    float3 colorBase = float3(0.88, 0.035, 0.055);
    float3 colorTop = float3(0.48, 0.16, 0.15);
    float3 colorIchorOrange = float3(1.0, 0.42, 0.015);
    float3 colorIchorYellow = float3(1.0, 0.95, 0.12);
    
    float coords = sin(screenCoords.x * 30 + uTime * 2.4) * 0.5 + 0.5;
    float txt = tex2D(uImage1, screenCoords * float2(1.5, 3) + float2(uTime * -0.07, uTime * -0.12)).r;
    float txtVal = pow(max(txt, 0), 1.2);
    float txt2 = tex2D(uImage2, screenCoords * float2(4, 3) + float2(uTime * -0.07, uTime * -0.12)).r;
    float txt2Val = pow(max(txt2 - 0.2, 0) * 1.25, 1.3);
    float3 gradientColor = lerp(colorBase, colorTop, coords) * min(1.4, 0.75 + txtVal * 0.7);
    float3 colorIchor = lerp(colorIchorOrange, colorIchorYellow, saturate(txt2Val));
    gradientColor = lerp(gradientColor, colorIchor, txt2Val);
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
