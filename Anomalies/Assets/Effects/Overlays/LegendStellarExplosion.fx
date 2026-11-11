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
// Failure: expansion, blast transition, erosion, dilution. Success: unused, full sprite opacity, unused, outer fade.
float4 uRemnantState;
float3 uInteriorState; // Outward drift, erosion threshold, remaining density.
float2 uOuterGeometry; // Outer front radius and sprite padding, in units of the original blast radius.
float uExpansionRatio;
float uYellowPalette;
float2 uTextureSize;

// One mirrored sector contains the edge from a point to its neighboring valley.
float RoundedStarDistance(float2 sector, float size)
{
    const float2 Tip = float2(0, 0.96);
    const float2 Valley = float2(0.28214, 0.38833); // Radius 0.48 at 36 degrees.
    const float2 Edge = Valley - Tip;
    float2 offset = sector - Tip * size;
    float alongEdge = clamp(dot(offset, Edge) / dot(Edge, Edge), 0, size);
    float side = dot(offset, float2(-Edge.y, Edge.x));
    return length(offset - Edge * alongEdge) * sign(side) - size * 0.04;
}

// A golden coronation: expanding filigree framed by sixteen star jewels.
// Equal-world-distance local UVs keep the rings circular at every viewport ratio.
float3 DrawCurtainCall(float2 position)
{
    float radius = length(position);
    float progress = uLifetimeRatio;
    float front = uExpansionRatio;
    float ringDistance = radius - uOuterGeometry.x;
    float halo = exp(-ringDistance * ringDistance / 0.0028);
    float rim = exp(-ringDistance * ringDistance / 0.000055);
    float innerDistance = radius - front * 0.79;
    float innerHalo = exp(-innerDistance * innerDistance / 0.0005);
    float echoDistance = radius - front * 0.58;
    float echo = exp(-echoDistance * echoDistance / 0.0012);
    float mist = tex2D(uNoiseSampler, position * 1.8 + float2(0, -uGlobalTime * 0.06)).r;
    float veil = exp(-radius * radius / 0.24) * (0.3 + mist * 0.3);
    float3 gold = float3(1, 0.64, 0.055);
    float3 pearl = float3(1, 0.87, 0.43);
    float3 amber = float3(1, 0.35, 0.018);
    float3 outer = gold * halo * 0.60 + pearl * rim * 0.7;
    float3 interior = gold * (innerHalo * 0.3 + echo * 0.16 + veil * 0.28);
    // Sixteen evenly spaced jewels unfurl with the ring, then soften into the night.
    float2 direction = position / max(radius, 0.001);
    float2 harmonic = direction;
    // Complex squaring yields cos(16a), sin(16a) without a polar seam or atan2.
    harmonic = float2(harmonic.x * harmonic.x - harmonic.y * harmonic.y, 2 * harmonic.x * harmonic.y);
    harmonic = float2(harmonic.x * harmonic.x - harmonic.y * harmonic.y, 2 * harmonic.x * harmonic.y);
    harmonic = float2(harmonic.x * harmonic.x - harmonic.y * harmonic.y, 2 * harmonic.x * harmonic.y);
    harmonic = float2(harmonic.x * harmonic.x - harmonic.y * harmonic.y, 2 * harmonic.x * harmonic.y);

    // Fold the sprite into a 36-degree half-sector with a point facing up.
    const float SectorAngle = 1.25663706;
    float angle = atan2(position.x, -position.y + 0.000001);
    float foldedAngle = abs(frac(angle / SectorAngle + 0.5) - 0.5) * SectorAngle;
    float2 sector = radius * float2(sin(foldedAngle), cos(foldedAngle));
    float starSize = min(front, 0.78) * 0.69 + uInteriorState.x * 0.025;
    float starDistance = RoundedStarDistance(sector, starSize);
    float pixelWidth = 2 / max(uTextureSize.x, 1);
    float threadWidth = max(pixelWidth, lerp(0.003, 0.009, uInteriorState.x));
    float thread = 1 - smoothstep(threadWidth * 0.35, threadWidth, abs(starDistance));
    float insetDistance = abs(starDistance + starSize * 0.042);
    float inset = 1 - smoothstep(threadWidth * 0.30, threadWidth, insetDistance);
    float starGlow = exp(-starDistance * starDistance / lerp(0.00065, 0.0026, uInteriorState.x));
    interior += (pearl * thread * 0.95 + gold * inset * 0.42 + amber * starGlow * 0.36) * uExpansionRatio;

    // Break the gilded lines into drifting arcs. The thinner glow gives way first.
    float2 dustFlow = position - direction * uInteriorState.x * 0.05;
    float dust = tex2D(uNoiseSampler, dustFlow * 3.8 + 0.37).r;
    float goldDensity = dust * 0.65 + mist * 0.2 + thread * 0.15;
    float survivingGold = smoothstep(-0.13, 0.11, goldDensity - uInteriorState.y);
    interior *= survivingGold * uInteriorState.z;

    // The solid central flash opens along the same silhouette.
    float coreDistance = RoundedStarDistance(sector, 0.08 + front * (0.18 + progress * 0.18));
    float coreLight = 1 - smoothstep(-pixelWidth, pixelWidth, coreDistance);
    float coreGlow = exp(-max(coreDistance, 0) * max(coreDistance, 0) / 0.00035);
    float opening = 1 - progress;
    outer += (pearl * coreLight * 1.1 + amber * coreGlow * 0.35) * opening * opening;

    float radialOffset = radius - uOuterGeometry.x * 0.86;
    float tangentOffset = harmonic.y * max(radius, 0.02) / 16;
    float jewelMask = smoothstep(0.5, 0.95, harmonic.x);
    float twinkle = 0.75 + 0.25 * sin(direction.x * 13 + direction.y * 9 + uGlobalTime * 3);
    float bloom = exp(-(radialOffset * radialOffset + tangentOffset * tangentOffset) / 0.0014);
    float horizontal = exp(-radialOffset * radialOffset / 0.0022 - tangentOffset * tangentOffset / 0.000012);
    float vertical = exp(-radialOffset * radialOffset / 0.000012 - tangentOffset * tangentOffset / 0.0022);
    outer += pearl * (bloom * 0.35 + (horizontal + vertical) * 0.6) * twinkle * jewelMask;
    float innerEdge = 1 - smoothstep(1.04, 1.14, radius);
    return outer * uRemnantState.w + interior * innerEdge;
}

float4 DrawExplosion(float4 sampleColor, float2 position)
{
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
float3 DrawRemnant(float2 position)
{
    float radius = length(position);
    float2 direction = position / max(radius, 0.001);
    float expansion = uRemnantState.x;
    float outerDrift = uOuterGeometry.x - uExpansionRatio;
    float lobe = tex2D(uNoiseSampler, direction * 0.56 + 0.31).r;
    // Different ejection speeds spread the shell into lobes and leave a lower-density interior.
    float shellRadius = lerp(0.79, 0.94, expansion) + (lobe - 0.5) * lerp(0.06, 0.24, expansion);
    float width = lerp(0.09, 0.24, expansion);
    float shellDistance = (radius - outerDrift - shellRadius) / width;
    float shell = exp(-shellDistance * shellDistance);
    float2 flow = (position - direction * expansion * (0.18 + lobe * 0.18)) / lerp(1.0, 1.45, expansion);
    float clouds = tex2D(uNoiseSampler, flow * 1.8 + 0.41).r;
    float strands = tex2D(uNoiseSampler, flow * 4.6 + clouds * 0.4).r;
    float density = clouds * 0.7 + strands * 0.3;
    // Thin gas clears first while denser knots linger.
    float survivingGas = smoothstep(-0.14, 0.14, density - uRemnantState.z);
    float outerEdge = 1 - smoothstep(0.90, 1, radius - outerDrift);
    float glow = smoothstep(0.34, 0.74, density);
    float3 blueGas = lerp(float3(0.18, 0.20, 0.62), float3(0.45, 0.83, 1), glow);
    float3 goldGas = lerp(float3(0.52, 0.16, 0.38), float3(1, 0.65, 0.27), glow);
    float3 gasColor = lerp(blueGas, goldGas, uYellowPalette);
    // Slower inner ejecta expand into a diffuse, cooling cloud after the fast shell has thinned out.
    float drift = uInteriorState.x;
    float innerRadius = lerp(0.26, 0.56, drift) + (lobe - 0.5) * 0.16;
    float innerDistance = (radius - innerRadius) / lerp(0.16, 0.30, drift);
    float2 innerFlow = (position - direction * drift * (0.20 + 0.15 * lobe)) / lerp(1.0, 1.4, drift);
    float innerCloud = tex2D(uNoiseSampler, innerFlow * 2.0 + 0.27).r;
    float innerDensity = innerCloud * 0.78 + strands * 0.22;
    float innerGas = smoothstep(-0.19, 0.18, innerDensity - uInteriorState.y);
    float3 blueInterior = lerp(float3(0.32, 0.68, 1), float3(0.40, 0.25, 0.72), drift);
    float3 goldInterior = lerp(float3(0.82, 0.33, 0.24), float3(0.46, 0.21, 0.63), drift);
    float3 innerColor = lerp(blueInterior, goldInterior, uYellowPalette);
    float3 interior = innerColor * exp(-innerDistance * innerDistance) * innerGas * innerDensity * uInteriorState.z;
    float innerEdge = 1 - smoothstep(0.90, 1, radius);
    return gasColor * shell * survivingGas * (0.25 + density) * uRemnantState.w * outerEdge + interior * innerEdge;
}

float3 DrawGoldenImpact(float2 position, float radius)
{
    float front = uOuterGeometry.x;
    float rimDistance = radius - front;
    float rim = exp(-rimDistance * rimDistance / 0.00010);
    float halo = exp(-rimDistance * rimDistance / 0.0026);
    float echoDistance = radius - front * 0.88;
    float echo = exp(-echoDistance * echoDistance / 0.00022);
    float3 gold = float3(1, 0.62, 0.045);
    float3 hotGold = float3(1, 0.87, 0.29);
    float3 rings = (hotGold * rim * 1.25 + gold * (halo * 0.62 + echo * 0.65))
        * uExpansionRatio * (1 - uRemnantState.x) * uRemnantState.w;

    // Advect the star field with the inner cloud so its gold points travel with the ejecta.
    float drift = uInteriorState.x;
    float2 flow = position / (1 + drift * 0.65) * 14;
    float2 cell = floor(flow);
    float2 seed = frac(sin(float2(dot(cell, float2(41.17, 59.81)), dot(cell, float2(73.03, 29.57)))) * 4375.63);
    float2 star = frac(flow) - lerp(0.24, 0.76, seed);
    float2 square = star * star;
    float cross = exp(-dot(square, float2(900, 62))) + exp(-dot(square, float2(62, 900)));
    float starCore = exp(-dot(square, float2(410, 410)));
    float twinkle = 0.70 + 0.30 * sin(uGlobalTime * (3 + seed.x * 5) + seed.y * 6.28);
    float cloudDistance = (radius - lerp(0.27, 0.67, drift)) / 0.25;
    float cloud = exp(-cloudDistance * cloudDistance) * (1 - smoothstep(0.82, 1.08, radius));
    float surviving = smoothstep(-0.12, 0.16, seed.y - uInteriorState.y);
    float3 stars = (gold * cross * 0.65 + hotGold * starCore) * cloud * twinkle * surviving
        * smoothstep(0.32, 0.65, seed.x) * uInteriorState.z * uRemnantState.y;
    return rings + stars;
}

float4 Main(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, uniform float mode) : COLOR0
{
    float2 position = (coords - 0.5) * 2;
    if (mode > 1.5)
    {
        position *= uOuterGeometry.y;
        float radius = length(position);
        float edgeBand = smoothstep(uExpansionRatio * 0.65, max(uOuterGeometry.x, 0.001), radius);
        float2 blastPosition = position - position / max(radius, 0.001) * ((uOuterGeometry.x - uExpansionRatio) * edgeBand);
        float3 blast = DrawExplosion(sampleColor, blastPosition).rgb;
        float3 remnant = DrawRemnant(position);
        float3 color = lerp(blast, remnant, uRemnantState.y);
        if (mode > 2.5)
            color += DrawGoldenImpact(position, radius);
        return float4(color, sampleColor.a);
    }
    if (mode > 0.5)
        return float4(DrawCurtainCall(position * uOuterGeometry.y), sampleColor.a);
    else
        return DrawExplosion(sampleColor, position);
}
technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main(0.0);
    }
    pass CurtainCall
    {
        PixelShader = compile ps_3_0 Main(1.0);
    }
    pass SupernovaRemnant
    {
        PixelShader = compile ps_3_0 Main(2.0);
    }
    pass GoldenImpact
    {
        PixelShader = compile ps_3_0 Main(3.0);
    }
}
