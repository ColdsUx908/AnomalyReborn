// Reconstructed from Radiance Effects/Trails/RadiantBeamMarker.xnb.
// Visible color output only; distortion render target intentionally omitted.
float2 uBeamSize;
float uMarkerOpacity;
float uGlow;

float4 Main(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float distance = (uv.y - 0.5) * uBeamSize.x * 2.0;
    float envelope = smoothstep(0.0, 100.0, uv.x * uBeamSize.y)
        * sqrt(saturate(1.0 - pow(abs(uv.y - 0.5) * 2.0, 2.0)));
    float core = uMarkerOpacity * (1.0 - saturate(abs(distance) / 8.0));
    float glow = uGlow / (abs(distance) + 0.03);
    return color * ((core + glow) * envelope);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
