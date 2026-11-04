sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity : register(C0);
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float4 uShaderSpecificData;

float4 Main(float4 position : SV_POSITION, float2 coords : TEXCOORD0) : COLOR0
{
    float distanceFromTargetPosition = distance(coords, 0.5);
    
    // Calculate the swirl coordinates.
    float2 centeredCoords = coords - 0.5;
    float swirlRotation = length(centeredCoords) * 41.2 - uTime * 6;
    float swirlSine = sin(swirlRotation);
    float swirlCosine = cos(swirlRotation);
    float2x2 swirlRotationMatrix = float2x2(swirlCosine, -swirlSine, swirlSine, swirlCosine);
    float2 swirlCoordinates = mul(centeredCoords, swirlRotationMatrix) + 0.5;
    
    // Calculate fade, swirl arm colors, and draw the portal to the screen.
    float swirlColorFade = saturate(distanceFromTargetPosition * 3) / (uOpacity + 0.0001);
    // The caller supplies opacity in [0, 1]; make the nonnegative pow domain explicit.
    float3 swirlBaseColor = lerp(uColor, uSecondaryColor, pow(max(swirlColorFade, 0.0), 0.33));
    // Generated Voronoi2: restore the original sample mean and contrast.
    float swirlNoiseColor = saturate(tex2D(uImage0, swirlCoordinates).r * 0.827525745 + 0.070533117) * (1 - swirlColorFade);
    float4 endColor = lerp(float4(swirlBaseColor, 0.1), 0, swirlColorFade);
    return lerp(0, endColor * (1 + (1 - swirlColorFade) * 2), saturate(swirlNoiseColor));
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
