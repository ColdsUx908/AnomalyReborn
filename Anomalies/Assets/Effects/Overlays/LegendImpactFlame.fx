sampler uMask : register(s0);
float2 uScreenPosition;
float4x4 uInverseViewMatrix;
float2 uResolution;
float uFlowTime;
float2 uCenter;
float4 uShape;
float2 uAxis;
float uStrength;
float3 uStarColor;
static const float uEdgeDistanceRange = 64.0;

float Hash(float2 cell)
{
    float3 value = frac(cell.xyx * float3(0.1031, 0.1030, 0.0973));
    value += dot(value, value.yzx + 33.33);
    return frac((value.x + value.y) * value.z);
}

float Noise(float2 position)
{
    float2 cell = floor(position);
    float2 fraction = frac(position);
    float2 blend = fraction * fraction * (3.0 - 2.0 * fraction);
    float upper = lerp(Hash(cell), Hash(cell + float2(1.0, 0.0)), blend.x);
    float lower = lerp(Hash(cell + float2(0.0, 1.0)), Hash(cell + 1.0), blend.x);
    return lerp(upper, lower, blend.y);
}

float SampleExterior(float2 uv, float2 radius)
{
    float2 diagonal = radius * 0.70710678;
    float minProximity = tex2D(uMask, uv + float2(radius.x, 0)).b;
    minProximity = min(minProximity, tex2D(uMask, uv - float2(radius.x, 0)).b);
    minProximity = min(minProximity, tex2D(uMask, uv + float2(0, radius.y)).b);
    minProximity = min(minProximity, tex2D(uMask, uv - float2(0, radius.y)).b);
    minProximity = min(minProximity, tex2D(uMask, uv + diagonal).b);
    minProximity = min(minProximity, tex2D(uMask, uv - diagonal).b);
    minProximity = min(minProximity, tex2D(uMask, uv + float2(diagonal.x, -diagonal.y)).b);
    minProximity = min(minProximity, tex2D(uMask, uv + float2(-diagonal.x, diagonal.y)).b);
    return (1.0 - minProximity) * uEdgeDistanceRange;
}

float4 Main(float2 uv : TEXCOORD0, uniform float writeMask) : COLOR0
{
    float2 world = mul(float4(uv * uResolution, 0, 1), uInverseViewMatrix).xy + uScreenPosition;
    if (writeMask > 0.5)
    {
        float2 offset = world - uCenter;
        float radius = length(offset);
        // Intersect a radial interval with a strip. A half-disc uses a one-sided strip.
        float projection = dot(offset, uAxis);
        float radial = max(radius - uShape.x, uShape.y - radius);
        float strip = max(projection - uShape.z, uShape.w - projection);
        float distance = max(radial, strip);
        float aa = max(0.001, fwidth(distance));
        float coverage = saturate(0.5 - distance / aa);
        // Max blending retains the nearest shape at each pixel, including across overlaps.
        // Encode world distance so the rim follows camera zoom instead of viewport dimensions.
        float proximity = 1.0 - saturate(distance / uEdgeDistanceRange);
        return float4(coverage, coverage * uStrength, proximity, 0.0);
    }

    float strength = tex2D(uMask, uv).g;
    if (strength <= 0.0)
        return 0.0;

    // Two layers of convection shear as they rise. The smaller layer traces hot granular lanes.
    float2 flow = world / float2(104.0, 128.0) + float2(0.14, 0.62) * uFlowTime;
    float2 warp = float2(Noise(flow * 0.54 + 4.2), Noise(flow * 0.57 - 7.8));
    float2 plasma = flow + (warp - 0.5) * float2(2.3, 1.65);
    float broad = Noise(plasma * 0.76);
    float medium = Noise(plasma * 2.15 + float2(-0.21, 0.17) * uFlowTime + 8.3);
    float fine = Noise(plasma * 5.9 + float2(0.28, 0.39) * uFlowTime + 16.7);
    float heat = smoothstep(0.23, 0.81, broad * 0.51 + medium * 0.35 + fine * 0.14);
    float lane = 1.0 - saturate(abs(medium - 0.53) * 8.5);
    float filaments = lane * lane * smoothstep(0.27, 0.66, broad);
    float grain = Noise(plasma * 14.8 + float2(-0.31, 0.58) * uFlowTime + 23.6);
    float granules = smoothstep(0.52, 0.84, fine) * smoothstep(0.42, 0.86, grain);
    float hotSpots = heat * 0.74 + filaments * 0.22 + granules * 0.18;
    float whiteHeat = smoothstep(0.34, 0.74, hotSpots);

    // Sample the continuous exterior distance so the orange rim fades without discrete bands.
    float tongues = smoothstep(0.16, 0.84, broad * 0.62 + medium * 0.38);
    float reach = lerp(38.0, 54.0, tongues);
    // Convert the fixed world-space reach to UV offsets through the inverse camera transform.
    float2 worldPerPixel = max(float2(0.001, 0.001), float2(length(ddx(world)), length(ddy(world))));
    float exterior = SampleExterior(uv, reach / worldPerPixel / uResolution);
    float rim = smoothstep(0.08, 0.95, exterior / reach);
    float hotRim = smoothstep(0.24, 0.85, heat * 0.65 + fine * 0.35);

    float3 gold = lerp(uStarColor, float3(1.0, 0.96, 0.28), heat);
    float3 interior = lerp(gold, float3(1.0, 0.995, 0.97), whiteHeat);
    float3 orange = lerp(float3(1.0, 0.36, 0.025), float3(1.0, 0.69, 0.08), hotRim);
    float3 color = lerp(interior, orange, rim);

    // The green channel holds the strongest envelope here.
    float alpha = strength * lerp(0.46 + heat * 0.22 + whiteHeat * 0.16, 0.94, rim);
    // Keep the rim saturated even over a bright background.
    float goldEmission = lerp(0.22 + heat * 0.06, 0.145, whiteHeat);
    float emission = strength * lerp(goldEmission, 0.10, rim);
    return float4(color * (alpha + emission), alpha);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main(0.0);
    }
    pass Coverage
    {
        PixelShader = compile ps_3_0 Main(1.0);
    }
}
