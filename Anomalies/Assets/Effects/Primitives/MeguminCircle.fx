sampler uImage0 : register(s0);
float uFlowTime;
float uBrightness;

float Ring(float radius, float target, float sharpness)
{
    return exp(-abs(radius - target) * sharpness);
}

float Segment(float2 p, float2 a, float2 b)
{
    float2 d = b - a;
    float t = saturate(dot(p - a, d) / dot(d, d));
    return exp(-length(p - a - d * t) * 260.0);
}

float4 Main(float2 uv : TEXCOORD0, float4 vertexColor : COLOR0) : COLOR0
{
    float2 p = uv * 2.0 - 1.0;
    float radius = length(p);
    float angle = atan2(p.y, p.x) + uFlowTime * 0.3;
    float rings = Ring(radius, 0.86, 260.0) + Ring(radius, 0.82, 300.0)
        + Ring(radius, 0.64, 270.0) + Ring(radius, 0.61, 280.0) + Ring(radius, 0.22, 180.0);
    float sector = frac(angle / 6.283185 * 32.0);
    float band = step(0.68, radius) * step(radius, 0.78);
    float bars = step(0.19, sector) * step(sector, 0.28);
    bars += step(0.57, sector) * step(sector, 0.66) * step(radius, 0.75);
    bars += step(0.22, sector) * step(sector, 0.63) * (Ring(radius, 0.705, 500.0) + Ring(radius, 0.758, 500.0));
    float glyphs = bars * band;
    float star = 0.0;
    for (int i = 0; i < 6; i++)
    {
        float a = i * 1.04719755 + uFlowTime * 0.5;
        float b = a + 2.0943951;
        star += Segment(p, float2(cos(a), sin(a)) * 0.59, float2(cos(b), sin(b)) * 0.59);
    }
    float rays = pow(saturate(cos(angle * 12.0)), 32.0) * Ring(radius, 0.9, 75.0);
    float halo = Ring(radius, 0.84, 35.0) * 0.3 + Ring(radius, 0.62, 35.0) * 0.15;
    float glow = rings + glyphs * 0.9 + star * 0.75 + rays * 0.7;
    float alpha = saturate(glow + halo) * (1.0 - smoothstep(0.94, 1.0, radius));
    // Self-lit emission: preserve the original additive contribution and increase it fivefold before blending.
    // Zero alpha keeps the scene visible; the geometric mask also clips all light to the circular shape.
    float3 light = (glow * 2.4 + halo) * vertexColor.rgb * alpha * vertexColor.a * uBrightness;
    return float4(light, 0.0);
}

technique Technique1
{
    pass Pass0 { PixelShader = compile ps_3_0 Main(); }
}
