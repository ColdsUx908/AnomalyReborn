// Surface edge adapted from LegendEnergySphere; no interior stars or separate core.
// Outer glow origin: FargowiltasCrossmod (MIT), see Licenses/FargowiltasCrossmod-MIT.txt.
texture uNebulaNoiseTexture;
sampler uNebulaSampler : register(s1) = sampler_state
{
    Texture = <uNebulaNoiseTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
float3 uOrbColor;
float uOpacity;
float uGlobalTime;

float SampleNebulaNoise(float2 uv)
{
    const float NoiseScale = 1.117127757;
    const float NoiseOffset = -0.040383510;
    return saturate(tex2D(uNebulaSampler, uv).r * NoiseScale + NoiseOffset);
}

float4 Main(float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    // Same square local UV and surface radius as the large sphere, at 12 world pixels.
    float2 offset = coords - 0.5;
    float radius = length(offset);
    float angle = atan2(offset.y, offset.x);
    float2 polar = float2(radius, angle / 6.2831853 + 0.5);
    float noiseA = SampleNebulaNoise(polar * float2(5, 3) + uGlobalTime * float2(-0.22, 0.035));
    float noiseB = SampleNebulaNoise(polar * float2(9, 5) + uGlobalTime * float2(-0.14, -0.025));
    float edgeNoise = SampleNebulaNoise(polar * float2(3, 4) + uGlobalTime * float2(-0.45, 0));
    float distance = radius + (edgeNoise - 0.5) * 0.012;
    // Screen derivatives soften the moving rim at this much smaller size and during absorption.
    float aa = max(fwidth(distance), 0.004);
    float outerGlow = 0.05 / max(abs(distance - 0.25), aa)
        * (1 - smoothstep(0.25, 0.4, distance));
    float bodyOpacity = 1 - smoothstep(0.25 - aa, 0.25 + aa, distance);
    float3 bodyColor = lerp(uOrbColor, float3(1, 1, 1), 0.22)
        * (0.65 + noiseA * 0.2 + noiseB * 0.15);
    float3 color = bodyColor * bodyOpacity
        + uOrbColor * outerGlow * lerp(0.22, 1, smoothstep(0.17, 0.245, radius));
    return float4(color * uOpacity * sampleColor.rgb * sampleColor.a,
        bodyOpacity * uOpacity * sampleColor.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
