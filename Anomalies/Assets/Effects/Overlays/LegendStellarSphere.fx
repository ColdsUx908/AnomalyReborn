// Outer glow adapted from FargowiltasCrossmod (MIT, copyright 2023 Javyz).
// License: Anomalies/Licenses/FargowiltasCrossmod-MIT.txt.
// Square sprite-local UVs keep stars circular at every viewport aspect ratio.
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
float3 uGoldColor;
float3 uStarlightColor;
float uCoreFlash;
float uBluePalette;
float uPhotosphere;

// Generated Perlin2: preserve the original LinearWrap sample mean and contrast.
float SampleNebulaNoise(float2 uv)
{
    const float NoiseScale = 1.117127757;
    const float NoiseOffset = -0.040383510;
    return saturate(tex2D(uNebulaSampler, uv).r * NoiseScale + NoiseOffset);
}

float Hash(float2 cell)
{
    return frac(sin(dot(cell, float2(127.1, 311.7))) * 43758.5453);
}

float Starfield(float2 position, float density, float seed)
{
    float2 grid = position * density + seed;
    float2 cell = floor(grid);
    float random = Hash(cell);
    float2 offset = float2(Hash(cell + 13.7), Hash(cell + 43.1)) * 0.5 + 0.25;
    float2 localPosition = frac(grid) - offset;
    float core = pow(saturate(1 - length(localPosition) * 8), 3);
    float rays = pow(saturate(1 - abs(localPosition.x) * 30), 3) * saturate(1 - abs(localPosition.y) * 7)
        + pow(saturate(1 - abs(localPosition.y) * 30), 3) * saturate(1 - abs(localPosition.x) * 7);
    float twinkle = 0.55 + 0.45 * sin(uGlobalTime * (1.5 + random) + random * 25);
    return (core + rays * 0.85) * twinkle * step(0.52, random);
}

float4 Main(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0, uniform float coreLayer) : COLOR0
{
    float2 offset = coords - 0.5;
    float distanceFromCenter = length(offset);
    float angle = atan2(offset.y, offset.x);
    float radiusRatio = distanceFromCenter * 4;
    // 角度固定，径向流动坐标用于噪声与星场。
    float orbitalAngle = angle;
    float spiralAngle = orbitalAngle;
    float2 polar = float2(distanceFromCenter, spiralAngle / 6.2831853 + 0.5);
    float noiseA = SampleNebulaNoise(polar * float2(5, 3) + uGlobalTime * float2(-0.22, 0.035));
    float noiseB = SampleNebulaNoise(polar * float2(9, 5) + uGlobalTime * float2(-0.14, -0.025));
    // 恢复原球的窄亮边和独立白核；噪声仅轻微扰动轮廓，不再铺成均匀发亮的球面。
    float edgeNoise = SampleNebulaNoise(polar * float2(3, 4) + uGlobalTime * float2(-0.45, 0));
    float distance = distanceFromCenter + (edgeNoise - 0.5) * 0.012;
    float outerGlow = 0.05 / max(abs(distance - 0.25), 0.004)
        * (1 - smoothstep(0.25, 0.4, distance));
    float corePulse = 0.5 + 0.5 * sin(uGlobalTime * 9 + sin(uGlobalTime * 3.7) * 1.2);
    float coreRadius = 0.14 + corePulse * 0.018;
    float coreDistance = radiusRatio / (1 + uCoreFlash * 0.65);
    float coreBody = 1 - smoothstep(coreRadius * 0.7, coreRadius, coreDistance);
    float coreHalo = pow(saturate(1 - coreDistance / 0.42), 3) * (0.7 + corePulse * 0.8);
    float coreRays = pow(abs(cos(angle * 4 + uGlobalTime * 0.3)), 18)
        * pow(saturate(1 - coreDistance / (0.32 + corePulse * 0.08)), 2);
    float innerGlow = (coreBody * (1.8 + corePulse * 1.8) + coreHalo + coreRays) * (1 + uCoreFlash * 1.5);
    if (coreLayer > 0.5)
        return float4(uStarlightColor * innerGlow * sampleColor.rgb * sampleColor.a * (1 - uPhotosphere), 0);
    float interiorMask = 1 - smoothstep(0.22, 0.25, distanceFromCenter);
    float streaks = 0;
    float tips = 0;
    // 二十四个独立槽位高频生成星光能量，随机角度、长度和宽度。
    // 线头沿直线吸入星核，尾部沿径向延伸。

    for (int i = 0; i < 24; i++)
    {
        float clock = uGlobalTime * 1.25 + i / 24.0;
        float generation = floor(clock);
        float age = frac(clock);
        float seed = Hash(float2(i, generation));
        float birthAngle = seed * 6.2831853;
        float headRadius = 1 - age * (1 - coreRadius);
        float tailLength = lerp(0.08, 0.19, Hash(float2(i + 19, generation)));
        float along = radiusRatio - headRadius;
        float angularOffset = frac((angle - birthAngle)
            / 6.2831853 + 0.5) - 0.5;
        float width = lerp(0.009, 0.018, Hash(float2(i + 37, generation)));
        float tailTaper = 1 - smoothstep(0.025, tailLength, along);
        float crossSection = pow(saturate(1 - abs(angularOffset) / max(width * tailTaper, 0.001)), 2);
        float segment = smoothstep(-0.035, 0.025, along)
            * (1 - smoothstep(tailLength * 0.55, tailLength, along));
        float fade = smoothstep(0, 0.07, age) * (1 - smoothstep(0.94, 1, age));
        streaks = max(streaks, crossSection * segment * fade);
        // 线头只沿径向延伸，去掉角向展开的十字光芒，形成纤细光波。
        float waveHead = pow(saturate(1 - abs(along - 0.025) * 14), 2)
            * pow(saturate(1 - abs(angularOffset) / (width * 0.45)), 2);
        tips += waveHead * fade;
    }
    streaks *= interiorMask * (1 - coreBody);
    tips *= interiorMask * (1 - coreBody);
    // 参考 EventideDeathSweep 的星云底色、亮星提取与闪烁分层。
    // 用局部等比例平面生成双层星野，不受极坐标压扁，也不依赖其他内容模块的贴图。
    float2 skyPosition = offset * 4;
    float skyMask = smoothstep(0.045, 0.085, distanceFromCenter)
        * (1 - smoothstep(0.17, 0.215, distanceFromCenter));
    float distantStars = Starfield(skyPosition + uGlobalTime * float2(0.008, -0.005), 22, 31.7);
    float brightStars = Starfield(skyPosition + uGlobalTime * float2(-0.004, 0.006), 9, 7.3);
    float stars = (distantStars * 1.25 + brightStars * 2.2) * skyMask;
    float cloud = SampleNebulaNoise(skyPosition * 0.65 + uGlobalTime * float2(0.008, -0.005));
    float cloudLight = pow(saturate(cloud * 1.8), 3);
    float3 gold = uGoldColor;
    float3 starlight = uStarlightColor;
    float bodyOpacity = 1 - smoothstep(0.24, 0.252, distanceFromCenter);
    // 明亮球体上用暖金短线保留颜色对比；线头叠加星芒，外圈保持原有窄亮边。
    float3 bodyColor = lerp(float3(0.78, 0.72, 0.4), float3(0.34, 0.48, 0.78), uBluePalette) * (0.85 + noiseA * 0.1 + noiseB * 0.05);
    bodyColor += lerp(lerp(float3(0.07, 0.055, 0.015), float3(0.015, 0.035, 0.09), uBluePalette), lerp(float3(0.18, 0.14, 0.055), float3(0.055, 0.11, 0.23), uBluePalette), cloudLight) * skyMask;
    float3 streakColor = lerp(lerp(float3(1, 0.9, 0.52), float3(0.45, 0.65, 1), uBluePalette), lerp(float3(1, 0.97, 0.76), float3(0.83, 0.91, 1), uBluePalette), saturate(radiusRatio));
    float3 color = lerp(bodyColor, streakColor, streaks * 0.7) * bodyOpacity
        + gold * outerGlow * lerp(0.22, 1, smoothstep(0.17, 0.245, distanceFromCenter)) * (1 - streaks * 0.3)
        + starlight * (stars * 2 + tips * 2.5);
    // AlphaBlend 使用预乘颜色：底层遮挡背景，额外亮度继续产生金白辉光。
    return float4(color * sampleColor.rgb * sampleColor.a, bodyOpacity * sampleColor.a);
}
technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main(0.0);
    }
    pass Core
    {
        PixelShader = compile ps_3_0 Main(1.0);
    }
}
