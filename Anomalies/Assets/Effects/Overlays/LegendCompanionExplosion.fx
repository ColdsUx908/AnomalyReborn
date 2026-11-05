// Inspired by CrossMod Eternity Ares GaussNukeBoom's filled blast and bright rim.
// Reference: Fargo-Team/FargosSoulsDLC, GaussNukeExplosionShader (MIT, Javyz 2023).
// License: Anomalies/Licenses/FargowiltasCrossmod-MIT.txt.
// Sprite-local square UVs represent equal world distances on both axes.
texture uNoiseTexture;
sampler uNoiseSampler : register(s1) = sampler_state
{
    Texture = <uNoiseTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
float uGlobalTime;
float uLifetimeRatio;
float uExpansionRatio;
float uYellowPalette;
float2 uTextureSize;

float4 Main(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 position = (coords - 0.5) * 2;
    float radius = length(position);
    float progress = uLifetimeRatio;
    float front = uExpansionRatio;
    float2 direction = position / max(radius, 0.001);
    // Cartesian domain warping keeps ejecta continuous across the polar seam.
    float2 flow = position * 2.4 - direction * uGlobalTime * 0.65;
    float broad = tex2D(uNoiseSampler, flow * 0.37 + 0.17).r;
    float fine = tex2D(uNoiseSampler, flow * 1.7 + broad * 0.23).r;
    float filaments = tex2D(uNoiseSampler, direction * 1.8 + radius * 0.18 - uGlobalTime * 0.12).r;
    float turbulence = broad * 0.65 + fine * 0.35;
    float behind = front - radius;
    float shellWidth = 0.022 + front * 0.025;
    float shellCoordinate = (behind - shellWidth * 0.7) / shellWidth;
    float shell = exp(-shellCoordinate * shellCoordinate);
    float cellular = smoothstep(0.22, 0.78, turbulence);
    float ejecta = exp(-max(behind, 0) * 5) * smoothstep(0.36, 0.7, filaments)
        * (0.2 + cellular) * smoothstep(0, 0.04, behind);
    float cavity = smoothstep(front * 0.12, front * 0.65 + 0.001, radius);
    float core = exp(-radius * radius / (0.012 + progress * 0.035)) * pow(max(1 - progress, 0), 6);
    // A stable white-hot leading edge remains legible independently of turbulent wisps.
    float leadingCoordinate = (behind - 0.008) / 0.009;
    float leadingEdge = exp(-leadingCoordinate * leadingCoordinate);
    float shock = shell * (0.7 + cellular * 1.7);
    float normalizedRadius = radius / max(front, 0.001);
    float pressureBand = smoothstep(0.65, 0.9, normalizedRadius);
    float filledBlast = (0.22 + cellular * 0.48) * (1 - pressureBand * 0.82);
    float launchFlash = pow(max(1 - progress, 0), 6);
    float3 warm = lerp(float3(1, 0.16, 0.025), float3(1, 0.72, 0.28), cellular);
    float3 cold = lerp(float3(0.08, 0.28, 1), float3(0.65, 0.9, 1), turbulence);
    cold = lerp(cold, lerp(float3(1, 0.52, 0.015), float3(1.6, 1.35, 0.3), turbulence), uYellowPalette);
    float3 color = cold * shock * 1.6 + warm * ejecta * cavity * 1.35
        + lerp(warm, cold, normalizedRadius * 0.5) * filledBlast
        + float3(0.8, 0.92, 1) * leadingEdge * 2.5
        + float3(0.82, 0.93, 1) * (core * 5 + launchFlash * 0.65);
    // All visible emission stays behind the damaging front and inside the maximum radius.
    float pixelWidth = 2 / max(uTextureSize.x, 1);
    float mask = (1 - smoothstep(max(front - pixelWidth, 0), max(front, 0.001), radius))
        * (1 - step(1, radius));
    return float4(color * mask, sampleColor.a);
}
technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
