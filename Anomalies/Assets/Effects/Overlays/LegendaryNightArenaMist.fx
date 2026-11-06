sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float2 uScreenResolution;
float2 uScreenPosition;
float4x4 uInverseViewMatrix;
float uTime;

float RestoreWaveSample(float value)
{
    return clamp(value * 0.223908176 + 0.015706734, 0.000000000, 0.247058824);
}

float4 GetMist(float2 screenCoords)
{
    screenCoords.x *= uScreenResolution.x / uScreenResolution.y;
    screenCoords *= 0.7;
    float wave = RestoreWaveSample(tex2D(uImage1, screenCoords + float2(uTime * -0.07, uTime * -0.12)).r);
    float detail = RestoreWaveSample(tex2D(uImage1, screenCoords * 1.6 + float2(uTime * 0.025, uTime * -0.035)).r);
    // 透明度完全来自波纹密度；无固定底色，最浓处也只遮挡少量背景。
    // Restore the original Wave brightness before the existing 0..63 normalization.
    float noise = saturate((wave * 0.75 + detail * 0.25) * (255.0 / 63.0));
    float density = smoothstep(0.08, 0.85, noise);
    float opacity = density * 0.38;
    float3 mistColor = float3(0xe3, 0xda, 0xc9) / 255;
    return float4(mistColor * opacity, opacity);
}

float4 Main(float4 screenPos : VPOS, float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    // Undo the camera transform before sampling noise in world space.
    float2 worldPosition = mul(float4(screenPos.xy, 0, 1), uInverseViewMatrix).xy + uScreenPosition;
    float2 screenCoords = worldPosition / uScreenResolution;
    float alpha = tex2D(uImage0, coords).a;
    return GetMist(screenCoords) * alpha * sampleColor;
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
