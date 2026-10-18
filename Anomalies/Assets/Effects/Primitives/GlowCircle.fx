float2 uScreenPosition;
float4x4 uInverseViewMatrix;
float2 uCenter;
float uRadius;
float uLineWidth;
float uGlowWidth;
float uOpacity;
float3 uColor;
float3 uSecondaryColor;

float4 Main(float4 screenPos : VPOS, float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    float2 p = mul(float4(screenPos.xy, 0, 1), uInverseViewMatrix).xy + uScreenPosition - uCenter;
    float distanceToLine = abs(length(p) - uRadius);
    // Screen-space derivatives keep the core antialiased at different zoom levels.
    float aa = max(fwidth(distanceToLine), 0.65);
    float core = 1.0 - smoothstep(uLineWidth * 0.5 - aa, uLineWidth * 0.5 + aa, distanceToLine);
    float outsideDistance = max(0.0, distanceToLine - uLineWidth * 0.5);
    float glow = exp(-outsideDistance * outsideDistance / (uGlowWidth * uGlowWidth));
    float halo = exp(-outsideDistance / uGlowWidth) * 0.15;
    float3 color = lerp(uColor, uSecondaryColor, core) * (core + glow * 0.5 + halo);
    // Premultiplied alpha: zero alpha adds light without covering the scene.
    return float4(color * uOpacity * sampleColor.rgb, 0.0);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
