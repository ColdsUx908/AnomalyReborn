sampler uCloudsTexture : register(s0);
sampler uDistortionTexture : register(s1);
sampler uErosionTexture : register(s2);

float uTime;
float uOverallOpacity;
float uDistortionStrength;
float uMainNoiseTextureScale;
float uDistortionTextureScale;
float uErosionTextureScale;
float uErosionMin;
float uGradientPrecision;

float2 uPixelationFactor;
float2 uWorldOffset;
float uAspectRatioCorrection;

float3 uDarkerPixelColor;
float3 uBrighterPixelColor;

// Generated Cloud/Harsh: restore the original sample mean and contrast.
float4 RestoreCloudSample(float4 color)
{
    color.rgb = saturate(color.rgb * 0.828835652 + 0.015395342);
    return color;
}
float RestoreHarshSample(float value)
{
    return saturate(value * 0.999211742 + 0.116102146);
}

float4 Main(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Pixelation.
    coords = round(coords * uPixelationFactor) / uPixelationFactor;
    
    coords.x *= uAspectRatioCorrection;
    // Stretch the coords out to better mimic a blowing wind effect.
    coords *= float2(0.95, 1.05);
    
    // Move with the player in the world.
    coords += uWorldOffset;
    
    // Calculate the distortion that'll be applied to our base noise coordinates.
    float2 distortedCoords = coords + float2(uTime * -0.03, uTime * -0.01) * uDistortionTextureScale;
    float distortion = RestoreCloudSample(tex2D(uDistortionTexture, distortedCoords)).r * uDistortionStrength;
    
    // Calculate the base noise colors.
    float2 adjustedCoords = (coords + distortion) * uMainNoiseTextureScale;
    float4 scrollingNoise1 = RestoreCloudSample(tex2D(uCloudsTexture, adjustedCoords + float2(uTime * -0.065, 0)));
    float4 scrollingNoise2 = RestoreCloudSample(tex2D(uCloudsTexture, adjustedCoords + float2(uTime * -0.015, 0)));
    
    // Multiply the red values of the noise maps together to get the horizontal brightness of both noise maps.
    float combinedBrightness = scrollingNoise1.r * scrollingNoise2.r;
    
    // Lerp between the two colors based on the brightness of a pixel.
    float3 colorFromBrightness = lerp(uDarkerPixelColor, uBrighterPixelColor, combinedBrightness);
    
    // Erode the final output to break up the entire effect slightly.
    float erosionColor = RestoreHarshSample(tex2D(uErosionTexture, adjustedCoords + float2(uTime * -0.07, uTime * -0.02) * uErosionTextureScale).r);
    float erosionMax = uErosionMin + 1;
    float erosion = smoothstep(uErosionMin, erosionMax, erosionColor.r);
    
    float4 finalColor = (float4(colorFromBrightness, 1) * (scrollingNoise1 + scrollingNoise2));
    finalColor *= erosion;
    
    // Apply posterization.
    finalColor = round(finalColor * uGradientPrecision) / uGradientPrecision;
    
    return finalColor * uOverallOpacity;

}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
