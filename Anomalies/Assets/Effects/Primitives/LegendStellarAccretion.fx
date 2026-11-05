float4x4 uTransformMatrix;
float uGlobalTime;
float uStreamLength;
float uDetachedEnd;
float uSphereRadius;
texture uNebulaNoiseTexture;
sampler uNebulaSampler : register(s1) = sampler_state
{
    Texture = <uNebulaNoiseTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
struct VertexInput { float4 Position : POSITION0; float4 Color : COLOR0; float2 UV : TEXCOORD0; };
struct PixelInput { float4 Position : POSITION0; float4 Color : COLOR0; float2 UV : TEXCOORD0; float3 SpherePosition : TEXCOORD1; };
PixelInput Transform(VertexInput input)
{
    PixelInput output;
    output.Position = mul(float4(input.Position.xy, 0, 1), uTransformMatrix);
    output.Color = input.Color;
    output.UV = input.UV;
    output.SpherePosition = input.Position.xyz;
    return output;
}
float4 Main(PixelInput input) : COLOR0
{
    // Compare against the visible hemisphere in projected world pixels, not the equatorial plane.
    float radial = length(input.SpherePosition.xy);
    float surfaceDepth = -sqrt(max(0.0001, uSphereRadius * uSphereRadius - radial * radial));
    float contact = smoothstep(0, 16, surfaceDepth - input.SpherePosition.z);
    // Exterior gas must remain fully visible right up to the limb. Fade only inside the disk.
    float outside = smoothstep(0, 1, (radial - uSphereRadius + 12) / 12.0);
    float visibility = lerp(contact, 1, outside);
    float along = input.UV.x;
    float across = input.UV.y * 2 - 1;
    // Texture motion follows arc length from donor to accretor; density never falls to isolated dots.
    float largeNoise = tex2D(uNebulaSampler, float2(along * 0.7 - uGlobalTime * 0.65, across * 0.35 + uGlobalTime * 0.03)).r;
    float fineNoise = tex2D(uNebulaSampler, float2(along * 2.2 - uGlobalTime * 1.15, across * 0.8 - uGlobalTime * 0.08)).r;
    float displaced = across + (largeNoise - 0.5) * 0.09;
    float envelope = exp(-displaced * displaced * 6);
    float halo = exp(-across * across * 2.8) * 0.22;
    float core = exp(-displaced * displaced * 28) * (0.17 + fineNoise * 0.15);
    float veins = pow(saturate(1 - abs(fineNoise - 0.55) * 7), 3) * envelope * 0.06;
    float bounds = 1 - smoothstep(0.68, 1, abs(across));
    float startFade = smoothstep(0, 0.32, along);
    // An attached stream ends beneath the surface: sphere occlusion owns its termination.
    // Arc-length fading here would extinguish the gas before it reaches the star.
    float endFade = 1 - uDetachedEnd * (1 - smoothstep(0, 0.16, uStreamLength - along));
    float mouth = smoothstep(0, 0.45, along + 0.25 - abs(across) * 0.65);
    float density = envelope * (0.56 + largeNoise * 0.24 + fineNoise * 0.1) + halo;
    // Undo the camera projection to heat gas by distance to the actual receiving surface.
    // This keeps the terminal palette stable as the stream length and orbit change.
    float cameraScale = max(0.01, (1400 + input.SpherePosition.z) / 1400);
    float distanceToStar = length(float3(input.SpherePosition.xy * cameraScale, input.SpherePosition.z));
    float surfaceHeat = 1 - smoothstep(0, 1, (distanceToStar - uSphereRadius - 24) / 176.0);
    float3 gasColor = lerp(input.Color.rgb, float3(1, 0.84, 0.48), surfaceHeat);
    float3 warmCore = lerp(gasColor, float3(1, 0.97, 0.86), 0.7);
    float3 emission = (gasColor * density * 0.55 + warmCore * (core + veins) * 0.6) * (1 + surfaceHeat * 0.65);
    // Premultiplied emissive RGB, alpha 0: AlphaBlend adds glow without dimming the scene.
    return float4(emission * visibility * bounds * mouth * startFade * endFade * input.Color.a, 0);
}
technique Technique1
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 Transform();
        PixelShader = compile ps_3_0 Main();
    }
}
