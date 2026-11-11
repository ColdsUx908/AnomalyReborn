float uTrailLength;
float uTrailOpacity;
float uPadding;
float uInverseHeadRadius;
float uInverseTrailLength;
float3 uHeadColor;
float3 uTailColor;

float4 Main(float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    float2 p = float2(coords.x * (uTrailLength + 2.0 * uPadding) - uTrailLength - uPadding,
                      (coords.y * 2.0 - 1.0) * uPadding);
    float progress = saturate(-p.x * uInverseTrailLength);
    float fade = (1.0 - progress) * (1.0 - progress);
    float width = lerp(8.0, 1.0, progress);
    float lateral = abs(p.y);
    float tailMask = saturate(1.0 - p.x) * uTrailOpacity;
    float core = saturate((width - lateral) * 0.23);
    float glow = saturate(1.0 - lateral * 0.057);
    glow *= glow;
    float3 tail = lerp(uHeadColor, uTailColor, progress) * (core * 0.85 + glow * 0.48) * fade * tailMask;

    float2 head = p * uInverseHeadRadius;
    float radiusSquared = dot(head, head);
    float halo = saturate(1.0 - radiusSquared * 0.17);
    float innerGlow = saturate(1.0 - radiusSquared * 0.65);
    float3 headGlow = uHeadColor * (0.46 * halo * halo + 0.32 * innerGlow * innerGlow);
    return float4((tail + headGlow) * sampleColor.rgb, 0.0);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
