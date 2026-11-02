sampler mainWindTexture : register(s0);
sampler windHighlightsTexture : register(s1);
sampler distortionTexture : register(s2);
sampler erosionTexture : register(s3);

float time;
float overallOpacity;
float distortionStrength;
float mainNoiseTextureScale;
float distortionTextureScale;
float erosionTextureScale;
float erosionMin;
float gradientPrecision;

float2 pixelationFactor;
float2 worldOffset;
float aspectRatioCorrection;

float3 darkerPixelColor;
float3 brighterPixelColor;
float3 highlightsColor;

float RestoreMelt2Sample(float value)
{
    return clamp(value * 0.615290304 + 0.210866402, 0.235294118, 0.768627451);
}
float4 RestoreNeuronSample(float4 color)
{
    color.rgb = clamp(color.rgb * 1.044507524 + 0.111467599, 0.000000000, 0.901960784);
    return color;
}
float4 RestoreSharpSample(float4 color)
{
    color.rgb = clamp(color.rgb * 0.912206973 + 0.020639424, 0.003921569, 0.988235294);
    return color;
}
float RestorePebbleSample(float value)
{
    return clamp(value * 0.577707026 + 0.162410500, 0.015686275, 0.996078431);
}

float4 Main(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Pixelation.
    coords = round(coords * pixelationFactor) / pixelationFactor;
    
    coords.x *= aspectRatioCorrection;
    // Stretch the coords out to better mimic a blowing wind effect.
    coords *= float2(0.25, 1.25);

    // Distort the coords in a sine wave so that they bend up and down.
    coords.y += sin(coords.x * 10 + time) * 0.25;
    
    // Move with the player in the world.
    coords += worldOffset;
    
    // Calculate the distortion that'll be applied to our base noise coordinates.
    float2 distortedCoords = coords + float2(time * -0.18, 0) * distortionTextureScale;
    float distortion = RestoreMelt2Sample(tex2D(distortionTexture, distortedCoords).r) * distortionStrength;
    
    // Calculate the base noise colors.
    float2 adjustedCoords = (coords + distortion) * mainNoiseTextureScale;
    float4 scrollingNoise1 = RestoreNeuronSample(tex2D(mainWindTexture, adjustedCoords + float2(time * -0.092, 0)));
    float4 scrollingNoise2 = RestoreNeuronSample(tex2D(mainWindTexture, adjustedCoords + float2(time * -0.031, 0)));
    
    // Multiply the red values of the noise maps together to get the horizontal brightness of both noise maps.
    float combinedBrightness = scrollingNoise1.r * scrollingNoise2.r;
    
    // Lerp between the two colors based on the brightness of a pixel.
    float3 colorFromBrightness = lerp(darkerPixelColor, brighterPixelColor, combinedBrightness);
    
    // Sample another texture to be used as highlights alongside the two noise textures.
    float4 highlightNoise = RestoreSharpSample(tex2D(windHighlightsTexture, adjustedCoords + float2(time * -0.115, 0)));
    float4 windHighlights = highlightNoise * float4(highlightsColor, 1);
    
    // Erode the final output to break up the entire effect slightly.
    float erosionColor = RestorePebbleSample(tex2D(erosionTexture, adjustedCoords + float2(time * -0.087, 0) * erosionTextureScale).r);
    float erosionMax = erosionMin + 1;
    float erosion = smoothstep(erosionMin, erosionMax, erosionColor.r);
    
    float4 finalColor = (float4(colorFromBrightness, 1) * (scrollingNoise1 + scrollingNoise2)) + (windHighlights * 2.25);
    finalColor *= erosion;
    
    // Apply posterization.
    finalColor = round(finalColor * gradientPrecision) / gradientPrecision;
    
    return finalColor * overallOpacity;

}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
