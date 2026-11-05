// Emissive stellar photosphere and corona. Square local UVs are world-isotropic.
texture uNebulaNoiseTexture;
sampler uNebulaSampler : register(s1) = sampler_state
{
    Texture = <uNebulaNoiseTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
float uGlobalTime;
float uSpin;
float uBluePalette;
float uOrangeProgress;
float uCoreFlash;
float uDissipation;
float uCoronaOpacity;

float Noise3(float3 position)
{
    float a = tex2D(uNebulaSampler, position.xy + position.z * float2(0.17, -0.11)).r;
    float b = tex2D(uNebulaSampler, position.yz + position.x * float2(-0.13, 0.19) + 0.37).r;
    float c = tex2D(uNebulaSampler, position.zx + position.y * float2(0.21, 0.09) + 0.71).r;
    return saturate(((a * 0.5 + b * 0.3 + c * 0.2) - 0.48) * 2.5 + 0.5);
}

float4 Main(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 disk = (coords - 0.5) * 4;
    float radius = length(disk);
    float facing = sqrt(max(0.0001, 1 - dot(disk, disk)));
    float3 normal = float3(disk, facing);
    float3 tilted = float3(normal.x, normal.y * 0.94 + normal.z * 0.342,
        normal.z * 0.94 - normal.y * 0.342);
    float sine = sin(uSpin);
    float cosine = cos(uSpin);
    float3 surface = float3(tilted.x * cosine + tilted.z * sine, tilted.y,
        tilted.z * cosine - tilted.x * sine);
    float time = uGlobalTime * 0.025;
    // Low frequency domain warp forms rolling convection, rather than sand-like speckles.
    float3 warp = float3(Noise3(surface * 0.32 + time),
        Noise3(surface * 0.32 + 4.7 - time), Noise3(surface * 0.32 + 9.2 + time));
    float3 position = surface * 0.72 + (warp - 0.5) * 0.28;
    float broad = Noise3(position + time * 0.2);
    float medium = Noise3(position * 2.1 - time * 0.35 + 2.4);
    float fine = Noise3(position * 4.7 + 7.3 + time * 0.55);
    float plasma = saturate(broad * 0.57 + medium * 0.3 + fine * 0.13);
    // Lift the gold photosphere out of its broad dark troughs; retain the blue star distribution.
    plasma = lerp(saturate(plasma * 1.08 + 0.075), plasma, uBluePalette);
    float filamentShape = saturate(1 - abs(medium - 0.54) * 9);
    float filaments = filamentShape * filamentShape * filamentShape * smoothstep(0.37, 0.68, broad);
    float hotRegions = smoothstep(0.44, 0.76, plasma);
    float3 ember = lerp(float3(0.52, 0.28, 0.035), float3(1.10, 1.10, 1.10), uBluePalette);
    float3 orange = lerp(float3(1.15, 0.77, 0.16), float3(0.08, 0.65, 1.35), uBluePalette);
    float3 yellow = lerp(float3(1.58, 1.22, 0.40), float3(0.68, 1.35, 1.8), uBluePalette);
    float3 whiteHot = lerp(float3(1.95, 1.62, 0.85), float3(1.65, 1.95, 2.2), uBluePalette);
    ember = lerp(ember, float3(0.64, 0.09, 0.008), uOrangeProgress);
    orange = lerp(orange, float3(1.35, 0.36, 0.025), uOrangeProgress);
    yellow = lerp(yellow, float3(1.75, 0.88, 0.09), uOrangeProgress);
    whiteHot = lerp(whiteHot, float3(2.0, 1.55, 0.55), uOrangeProgress);
    float3 color = lerp(ember, orange, smoothstep(0.2, 0.48, plasma));
    color = lerp(color, yellow, hotRegions);
    color += whiteHot * filaments * 0.48;
    // Coherent hotter convection patches, distinct from the fine filament highlights.
    float whiteRegions = smoothstep(0.50, 0.72, broad) * smoothstep(0.38, 0.66, medium);
    color = lerp(color, lerp(float3(1.85, 1.77, 1.52), float3(1.95, 1.20, 0.32), uOrangeProgress), whiteRegions * (1 - uBluePalette) * 0.85);
    // Keep self-emission at the limb; a hot chromosphere provides the bright reference silhouette.
    color *= 0.63 + 0.37 * facing;
    float chromosphere = exp(-abs(radius - 0.993) * 115);
    color += whiteHot * chromosphere * 1.35;
    float mask = (1 - smoothstep(0.995, 1.008, radius)) * saturate((plasma - uDissipation) * 10);
    float angle = atan2(disk.y, disk.x);
    float2 coronaUV = float2(angle / 6.2831853, radius * 0.7);
    float coronaNoise = tex2D(uNebulaSampler, coronaUV * float2(3, 1) + float2(time * 0.13, -time)).r;
    float tendrilShape = saturate(tex2D(uNebulaSampler, coronaUV * float2(7, 0.7)
        + float2(-time * 0.07, -time * 0.6)).r * 1.5 - 0.28);
    float tendrils = tendrilShape * tendrilShape * tendrilShape;
    float height = 0.025 + tendrils * 0.17;
    float outside = max(radius - 1, 0);
    float flames = exp(-outside / height * 3.2) * (0.4 + tendrils * 2.3);
    float halo = exp(-outside * 5.2) * 0.24 + exp(-outside * 11) * 0.25;
    float corona = (flames + halo) * smoothstep(0.975, 1.015, radius)
        * (1 - smoothstep(1.65, 1.98, radius));
    float3 coronaColor = lerp(orange, whiteHot, exp(-outside * 40));
    // Sparse magnetic arches, with independent heights and lifetimes, rise above the limb.
    float loopAngle = angle / 6.2831853 * 11 + uSpin * 0.12;
    float sector = floor(loopAngle + 0.5);
    sector -= floor(sector / 11) * 11; // Same prominence on both sides of the atan2 seam.
    float localAngle = frac(loopAngle + 0.5) - 0.5;
    float seed = frac(sin(sector * 127.1 + 19.7) * 43758.5453);
    float width = 0.12 + seed * 0.08;
    float archShape = sqrt(max(0.0001, 1 - localAngle * localAngle / (width * width)));
    float archRadius = 1.012 + archShape * (0.07 + seed * 0.1)
        + sin(localAngle * 70 + seed * 15 + time) * archShape * 0.006;
    float arch = exp(-abs(radius - archRadius) * 140)
        * (1 - smoothstep(width * 0.85, width, abs(localAngle)))
        * smoothstep(0.05, 0.4, sin(uGlobalTime * 0.45 + sector * 2.4))
        * smoothstep(1, 1.025, radius);
    float3 emission = color * mask + (coronaColor * corona + orange * arch * (0.35 + coronaNoise * 0.5)) * uCoronaOpacity;
    emission *= 1 + uCoreFlash * 0.45;
    return float4(emission * sampleColor.rgb * sampleColor.a, mask * sampleColor.a);
}
technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
