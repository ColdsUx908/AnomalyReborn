// Reconstructed from Radiance Effects/Trails/RadiantBeam.xnb.
// Visible color output only; pinch and firing distortion intentionally omitted.
float2 uBeamSize;
float uBeamStrength;
float uGlobalTime;

float4 Main(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float phase = uv.x * uBeamSize.y * 0.02 - uGlobalTime * 50.0;
    float wave = sin(phase) * 7.0 * (1.0 - uBeamStrength * 0.5)
        * (1.0 + sin(phase * 0.7100001) * 0.25);
    float distance = (uv.y - 0.5) * uBeamSize.x + wave;
    float envelope = min(uv.x * 10.0, 1.0) * pow(saturate(1.0 - abs(uv.y - 0.5) * 2.0), 3.0);
    // 保持原有包络和总宽度，略放宽核心，外沿用柔和亮度压缩避免形成实色描边。
    float intensity = uBeamStrength * 8.0 * envelope / (abs(distance) + 0.001);
    float core = 1.0 - smoothstep(0.0, 4.0, abs(distance));
    float3 edgeColor = color.rgb * (intensity / (1.0 + intensity * 0.25));
    float3 coreColor = lerp(color.rgb, float3(1.0, 1.0, 1.0), 0.8) * intensity;
    return float4(lerp(edgeColor, coreColor, core), color.a * intensity);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
