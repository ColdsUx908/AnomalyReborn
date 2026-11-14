sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float uFlowTime;
float uBeamTop;
float uBeamSpan;

float4 Main(float2 uv : TEXCOORD0, float4 vertexColor : COLOR0) : COLOR0
{
    float x = uv.x * 2.0 - 1.0;
    float y = uBeamTop + uv.y * uBeamSpan;
    float t = uFlowTime;
    float ax = abs(x);
    // Longitudinal noise moves toward the impact point; the beam remains straight and continuous.
    float flow = tex2D(uImage0, frac(float2(uv.x * 3.1 + t * 0.08, y * 5.0 - t * 3.8))).r;
    float detail = tex2D(uImage1, frac(float2(uv.x * 7.0 - t * 0.16, y * 14.0 - t * 6.0))).r;
    float turbulence = flow * 0.7 + detail * 0.3;
    float corona = (1.0 - smoothstep(0.34, 0.95, ax + (turbulence - 0.5) * 0.22)) * (0.25 + turbulence * 0.75);
    float inner = 1.0 - smoothstep(0.12, 0.38, ax);
    float core = 1.0 - smoothstep(0.026, 0.085, ax + sin(y * 85.0 - t * 18.0) * 0.008);
    float railPosition = 0.34 + sin(y * 35.0 - t * 13.0) * 0.018;
    float rails = 1.0 - smoothstep(0.012, 0.037, abs(ax - railPosition));
    float outerRails = (1.0 - smoothstep(0.008, 0.018, abs(ax - 0.53))) * (0.4 + 0.6 * flow);
    float3 light = float3(1.0, 0.035, 0.008) * corona * 1.1;
    light += float3(1.0, 0.32, 0.025) * inner * (0.45 + detail * 0.7);
    light += float3(1.0, 0.83, 0.42) * rails * 2.3;
    light += float3(1.0, 0.98, 0.87) * core * 3.2;
    light += float3(1.0, 0.15, 0.015) * outerRails;
    // Fine dotted ribbons and broken streaks echo the reference without cooling the explosion palette.
    float helix = sin(y * 46.0 - t * 11.0) * 0.23;
    float thread = 1.0 - smoothstep(0.008, 0.022, min(abs(x - helix), abs(x + helix)));
    float beads = pow(saturate(sin(y * 930.0 - t * 100.0)), 8.0);
    light += float3(1.0, 0.72, 0.19) * thread * (0.3 + beads * 1.7);
    float streak = pow(saturate(detail * flow * 2.0), 5.0) * (1.0 - smoothstep(0.42, 0.78, ax));
    light += float3(1.0, 0.44, 0.06) * streak * 0.7;
    // Zero-alpha premultiplied light adds to the world; no rectangular dark backdrop is written.
    // Caps lie outside the viewport. The mouse focus is never an endpoint or a fade boundary.
    return float4(light * vertexColor.rgb * vertexColor.a, 0.0);
}

technique Technique1
{
    pass Pass0 { PixelShader = compile ps_3_0 Main(); }
}
