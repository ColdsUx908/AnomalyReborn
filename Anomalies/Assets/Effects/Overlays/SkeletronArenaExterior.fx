float2 uScreenPosition;
float4x4 uInverseViewMatrix;
float2 uCenter;
float uRadius;
float uRotation;
float uFlowTime;
float uOpacity;

float Hash(float2 position)
{
    float3 mixed = frac(position.xyx * 0.1031);
    mixed += dot(mixed, mixed.yzx + 33.33);
    return frac((mixed.x + mixed.y) * mixed.z);
}

float Grain(float2 position)
{
    float2 cell = floor(position);
    float2 blend = frac(position);
    blend = blend * blend * (3.0 - 2.0 * blend);
    return lerp(lerp(Hash(cell), Hash(cell + float2(1, 0)), blend.x),
        lerp(Hash(cell + float2(0, 1)), Hash(cell + 1.0), blend.x), blend.y);
}

float Bone(float2 position)
{
    float shaft = length(float2(max(abs(position.x) - 24.0, 0.0), position.y)) - 4.2;
    float2 joint = float2(abs(position.x) - 27.0, abs(position.y) - 4.5);
    return min(shaft, length(joint) - 6.2);
}

float EmittedBone(float2 local, float slot, float generation, float age)
{
    float2 seed = float2(slot, generation);
    // Empty births, jittered perimeter positions and independent lifetimes remove the lattice.
    float chance = Hash(seed + 17.3);
    float lifetime = 25.0 + Hash(seed + 42.1) * 6.0;
    if (chance > 0.48 || age < 0.0 || age > lifetime)
        return 0.0;

    float birthAngle = (slot + 0.08 + Hash(seed + 3.7) * 0.84) * 0.1308996939;
    float normalAngle = uRotation + (floor((birthAngle - uRotation) / 0.3926990817) + 0.5) * 0.3926990817;
    float2 normal = float2(cos(normalAngle), sin(normalAngle));
    float2 tangent = float2(-normal.y, normal.x);
    float2 direction = float2(cos(birthAngle), sin(birthAngle));
    float2 birthPosition = direction * (uRadius * 0.9807852804 / dot(direction, normal));
    float speed = 10.0 + Hash(seed + 6.2) * 4.0;
    float drift = (Hash(seed + 8.4) - 0.5) * 1.6;
    float2 position = local - birthPosition - age * (normal * speed + tangent * drift);
    float scale = 0.55 + Hash(seed + 11.9) * 0.55;
    float2 axis = normalize(float2(Hash(seed + 14.8) - 0.5, Hash(seed + 9.1) - 0.5) + 0.001);
    position = float2(dot(position, axis), dot(position, float2(-axis.y, axis.x))) / scale;
    float boneDistance = Bone(position);
    // Mix single bones and loose crosses instead of repeating one recognizable stamp.
    if (Hash(seed + 19.2) > 0.55)
        boneDistance = min(boneDistance, Bone(float2(position.y * 0.94, -position.x + 3.0)));
    float fade = smoothstep(0.0, 2.5, age) * (1.0 - smoothstep(lifetime - 7.0, lifetime, age));
    return (1.0 - smoothstep(-0.8, 1.4, boneDistance)) * fade;
}

float SampleBones(float2 local)
{
    // Match the approved 4x preview, including births and fades, without accelerating the mist.
    const float BoneTimeScale = 4.0;
    float polarAngle = atan2(local.y, local.x);
    float nearestSlot = floor(frac(polarAngle / 6.2831853072) * 48.0);
    float bones = 0.0;
    // Only adjacent angular sectors can overlap a pixel; no full-screen particle scan.
    [loop] for (int i = -1; i <= 1; i++)
    {
        float slot = nearestSlot + i;
        slot -= floor(slot / 48.0) * 48.0;
        float clock = uFlowTime * BoneTimeScale + Hash(float2(slot, 71.0)) * 32.0;
        float generation = floor(clock / 32.0);
        bones = max(bones, EmittedBone(local, slot, generation, clock - generation * 32.0));
    }
    return bones;
}

float4 Main(float4 screenPos : VPOS, float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    float2 world = mul(float4(screenPos.xy, 0, 1), uInverseViewMatrix).xy + uScreenPosition;
    float2 local = world - uCenter;
    // The closest angular side normal is the maximum of all sixteen supporting planes.
    float polarAngle = atan2(local.y, local.x);
    float normalAngle = uRotation + (floor((polarAngle - uRotation) / 0.3926990817) + 0.5) * 0.3926990817;
    float distanceToEdge = dot(local, float2(cos(normalAngle), sin(normalAngle))) - uRadius * 0.9807852804;
    // Explicit rejection before any pattern modulation: the whole interior is RGBA zero.
    if (distanceToEdge <= 0.5 || distanceToEdge >= 460.0)
        return float4(0, 0, 0, 0);

    float2 flow = local + float2(uFlowTime * 2.0, -uFlowTime * 3.0);
    float cloud = Grain(flow / 110.0) * 0.65 + Grain(flow / 43.0) * 0.35;
    float envelope = smoothstep(0.5, 22.0, distanceToEdge)
        * (1.0 - smoothstep(170.0, 460.0, distanceToEdge));
    float bones = SampleBones(local);
    float pores = Grain(flow / 6.0);
    float opacity = envelope * (0.26 + cloud * 0.34 + bones * 0.30) * uOpacity;
    float3 ivory = lerp(float3(0.72, 0.66, 0.54), float3(1.0, 0.975, 0.90), saturate(cloud * 0.7 + bones * 0.55));
    ivory *= 0.92 + pores * 0.08;
    return float4(ivory * opacity, opacity) * sampleColor;
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
