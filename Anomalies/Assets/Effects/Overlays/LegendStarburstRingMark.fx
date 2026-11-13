float4x4 uInverseViewMatrix;
float2 uScreenPosition;
float2 uResolution;
float2 uCenter;
float uScale;
float uInnerRadius;
float uFront;
float uBody;
float uBoundaryRadius;
float uBoundaryOpacity;
float uBoundaryAlpha;
float3 uPale;
texture uGradient;
sampler uGradientSampler : register(s1) = sampler_state
{
    Texture = <uGradient>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 Main(float2 uv : TEXCOORD0) : COLOR0
{
    // Work in design pixels; viewport aspect and camera zoom are already inverted.
    float2 world = mul(float4(uv * uResolution, 0, 1), uInverseViewMatrix).xy + uScreenPosition;
    float r = length((world - uCenter) / uScale);
    float aa = max(fwidth(r), 0.001);
    float lineCoverage = saturate((0.75 - abs(r - uBoundaryRadius)) / aa + 0.5);
    float lineAlpha = lineCoverage * uBoundaryAlpha;
    float3 lineColor = uPale * lineCoverage * uBoundaryOpacity;
    float band = uFront - uInnerRadius;
    float t = saturate((r - uInnerRadius) / max(band, 0.001));
    float coverage = saturate((r - uInnerRadius) / aa + 0.5)
        * saturate((uFront - r) / aa + 0.5) * step(1.0, band);
    float4 body = tex2D(uGradientSampler, float2((t * 511.0 + 0.5) / 512.0, 0.5)) * uBody * coverage;
    return float4(body.rgb + lineColor * (1.0 - body.a), body.a + lineAlpha * (1.0 - body.a));
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
