float uLength;
float uPadding;
float uLineWidth;
float uGlowWidth;
float uOpacity;
float3 uColor;
float3 uSecondaryColor;

float4 Main(float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    // Work in the line quad's local coordinates, including round end caps.
    float2 p = (coords - 0.5) * float2(uLength + uPadding * 2.0, uPadding * 2.0);
    float distanceToLine = length(float2(max(abs(p.x) - uLength * 0.5, 0.0), p.y));
    float aa = max(fwidth(distanceToLine), 0.65);
    float core = 1.0 - smoothstep(uLineWidth * 0.5 - aa, uLineWidth * 0.5 + aa, distanceToLine);
    float outsideDistance = max(0.0, distanceToLine - uLineWidth * 0.5);
    float glow = exp(-outsideDistance * outsideDistance / (uGlowWidth * uGlowWidth));
    float halo = exp(-outsideDistance / uGlowWidth) * 0.15;
    float3 color = lerp(uColor, uSecondaryColor, core) * (core + glow * 0.5 + halo);
    return float4(color * uOpacity * sampleColor.rgb, 0.0);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
