sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float uFlowTime;
float uMode;

float4 Main(float2 uv : TEXCOORD0, float4 vertexColor : COLOR0) : COLOR0
{
    float2 p = uv * 2.0 - 1.0;
    float time = uFlowTime;
    float2 scroll = uv * float2(1.7, 1.4) + float2(time * 0.07, time * 0.48);
    float cloud = tex2D(uImage0, frac(scroll)).r;
    float detail = tex2D(uImage1, frac(uv * 3.8 + float2(-time * 0.18, time * 0.7))).r;
    float noise = cloud * 0.7 + detail * 0.3;
    float radius = length(p + (noise - 0.5) * 0.14);
    float field = radius + (noise - 0.5) * 0.28;
    float alpha = 1.0 - smoothstep(0.64, 0.94, field);
    float heat = saturate(1.2 - radius * 0.34 + (cloud - 0.5) * 0.85 + (detail - 0.5) * 0.35);

    if (uMode > 0.5 && uMode < 1.5)
    {
        float strands = tex2D(uImage0, frac(float2(uv.x * 2.2 + time * 0.04, uv.y * 0.55 - time * 2.8))).r;
        float edge = abs(p.x) + (strands - 0.5) * 0.26;
        alpha = (1.0 - smoothstep(0.48, 0.95, edge)) * smoothstep(0.0, 0.045, uv.y) * smoothstep(0.0, 0.06, 1.0 - uv.y);
        heat = saturate(1.0 - abs(p.x) * 0.8 + strands * 0.6);
    }

    float3 color = lerp(float3(0.32, 0.012, 0.035), float3(1.0, 0.08, 0.018), saturate(heat * 2.1));
    color = lerp(color, float3(1.0, 0.62, 0.06), smoothstep(0.48, 0.74, heat));
    color = lerp(color, float3(1.0, 0.98, 0.78), smoothstep(0.78, 0.96, heat));
    // Cooler cells form deep red islands between white-hot convection currents.
    color *= 0.88 + detail * 0.2;
    if (uMode > 1.5)
    {
        alpha *= 0.86;
        color = lerp(float3(0.025, 0.018, 0.035), float3(0.19, 0.075, 0.065), noise);
        color += float3(0.17, 0.035, 0.006) * pow(saturate(noise), 4.0);
    }
    return float4(color * alpha, alpha) * vertexColor;
}

technique Technique1
{
    pass Pass0 { PixelShader = compile ps_3_0 Main(); }
}
