sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float2 uScreenResolution;
float2 uPosition;
float uTime;

float3 GetColor(float2 screenCoords)
{
    screenCoords.x *= uScreenResolution.x / uScreenResolution.y;
    screenCoords *= 0.7;
    
    float3 colorTop = float3(0xe3, 0xda, 0xc9) / 255;
    
    float coords = sin(screenCoords.x * 30 + uTime * 2.4) * 0.5 + 0.5;
    float txt = tex2D(uImage1, screenCoords + float2(uTime * -0.07, uTime * -0.12)).r;
    float txtVal = txt;
    float3 gradientColor = colorTop * clamp(0.03 + txtVal, 0, 1);
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
