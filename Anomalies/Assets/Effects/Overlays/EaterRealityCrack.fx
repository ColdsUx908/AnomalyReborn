sampler uBaseTexture : register(s0);

float uOverallOpacity;
float uOpacityCutoffValue;
float uFadeoutPower;
float uMinBrightnessValue;
float uGradientPrecision;

float3 uBrighterPixelColor;
float3 uDarkerPixelColor;

float InverseLerp(float from, float to, float value)
{
    return (value - from) / (to - from);
}

float4 Main(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Convert the regular coordinates to polar coordinates to get a circle shape.
    float2 centeredCoords = coords - 0.5;
    float polarAngle = atan2(centeredCoords.y, centeredCoords.x) / 6.283;
    float distanceToCenter = length(centeredCoords);
    
    float2 polarCoords = float2(polarAngle, distanceToCenter);
    float4 color = tex2D(uBaseTexture, coords);
        
    // Get the horizontal brightness of the sampled texture and apply coloration.
    float brightness = (color.r + color.g + color.b) / 3;
    float brightnessRatio = InverseLerp(uMinBrightnessValue, 1, brightness);
    float3 colorFromBrightness = lerp(uDarkerPixelColor, uBrighterPixelColor, brightnessRatio);
    
    float4 coloredSample = color * float4(colorFromBrightness, 1);  
    
    // Fade out from the edges of the texture to the middle depending on the cutoff value.
    // "fadeoutPower" values which vary from 1 will affect how strong the edges fade while also increasing the brightness of 
    // the non-faded parts of the image to the middle. Use this wisely.
    if (-polarCoords.y < uOpacityCutoffValue)
        // Preserve the existing compiled pow magnitude for negative radial ratios.
        coloredSample.rgba *= pow(abs(-polarCoords.y / (polarCoords.y - uOpacityCutoffValue)), -uFadeoutPower);

    // Apply optional posterization.
    /*if (gradientPrecision > 0)
        finalColor = round(finalColor * gradientPrecision) / gradientPrecision;
    */
    return coloredSample * sampleColor * uOverallOpacity;
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}

