float4x4 uTransformMatrix;
float4x4 uInverseViewMatrix;
float2 uScreenPosition;
float2 uCenter;
float uAge;
float uCharge;
float uOpacity;
float uFieldScale;
float uFlow;

struct VertexOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

VertexOutput Transform(float4 position : POSITION0, float4 color : COLOR0, float2 uv : TEXCOORD0)
{
    VertexOutput output;
    output.Position = mul(position, uTransformMatrix);
    output.Color = color;
    output.UV = uv;
    return output;
}

float4 Main(float4 screenPos : VPOS, float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    // World-pixel distances preserve circular geometry and flow speed at every aspect/zoom.
    float2 local = mul(float4(screenPos.xy, 0, 1), uInverseViewMatrix).xy + uScreenPosition - uCenter;
    float radius = length(local);
    float angle = atan2(local.y, local.x);
    // Last 48 ticks compress the entire wave field into the core, rather than
    // cutting off a still-active field when the explosion starts.
    float fieldScale = uFieldScale;
    float collapse = 1.0 - fieldScale;
    float sourceRadius = radius / max(fieldScale, 0.02);
    float extent = max(0.01, 800.0 * fieldScale);
    float outer = 1.0 - smoothstep(extent * 0.55, extent, radius);
    float inner = smoothstep(35.0, 100.0, radius);
    // The increasing phase moves every ridge inward; angular perturbations break hard circles.
    float flow = uFlow;
    float warp = sin(angle * 5.0 + sourceRadius * 0.008 - uAge * 0.018) * 0.20
        + sin(angle * 9.0 - sourceRadius * 0.012 + uAge * 0.025) * 0.09;
    float phase = sourceRadius * 0.085 + flow + warp;
    float ridge = pow(saturate(0.5 + 0.5 * cos(phase)), 22.0);
    float soft = pow(saturate(0.5 + 0.5 * cos(phase - 0.6)), 3.0);
    float strands = pow(saturate(0.5 + 0.5 * sin(angle * 17.0 + sourceRadius * 0.025 + flow * 0.55)), 24.0);
    float circulation = 0.5 + 0.5 * sin(angle * 3.0 - uAge * 0.024 + radius * 0.009);
    float force = (ridge * 0.75 + soft * 0.085 + strands * 0.06) * outer * inner;
    force *= (0.38 + 0.62 * uCharge) * (0.65 + 0.35 * circulation);
    float coreWidth = lerp(lerp(15000.0, 34000.0, uCharge), 3500.0, collapse);
    float core = exp(-radius * radius / coreWidth) * (0.1 + 0.28 * uCharge + collapse * 0.35);
    float corona = exp(-pow((radius - lerp(78.0, 31.0, collapse)) / 28.0, 2.0)) * (0.04 + 0.1 * uCharge);
    float3 bone = lerp(float3(0.39, 0.59, 0.75), float3(1.0, 0.85, 0.56), saturate(1.0 - radius / max(extent, 1.0)));
    float3 light = bone * force + float3(1.0, 0.87, 0.64) * (core + corona);
    return float4(light * uOpacity, 1.0);
}

technique Technique1
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 Transform();
        PixelShader = compile ps_3_0 Main();
    }
}
