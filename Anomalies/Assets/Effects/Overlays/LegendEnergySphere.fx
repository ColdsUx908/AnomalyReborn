// Outer glow adapted from FargowiltasCrossmod (MIT, copyright 2023 Javyz).
// License: Anomalies/Licenses/FargowiltasCrossmod-MIT.txt.
// Square sprite-local UVs keep stars circular at every viewport aspect ratio.
texture nebulaNoiseTexture;
sampler NebulaSampler : register(s1) = sampler_state
{
    Texture = <nebulaNoiseTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
float globalTime;

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
    float core = pow(saturate(1 - length(localPosition) * 12), 3);
    float rays = pow(saturate(1 - abs(localPosition.x) * 65), 3) * saturate(1 - abs(localPosition.y) * 7)
        + pow(saturate(1 - abs(localPosition.y) * 65), 3) * saturate(1 - abs(localPosition.x) * 7);
    float twinkle = 0.55 + 0.45 * sin(globalTime * (1.5 + random) + random * 25);
    return (core + rays * 0.45) * twinkle * step(0.52, random);
}

float4 Main(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 offset = coords - 0.5;
    float distanceFromCenter = length(offset);
    float angle = atan2(offset.y, offset.x);
    float radiusRatio = distanceFromCenter * 4;
    // 共用旋转坐标驱动赤金底纹、亮带和星芒；对数半径将光带卷成银河旋臂。
    float orbitalAngle = angle - globalTime * 0.42;
    float spiralAngle = orbitalAngle + log(max(radiusRatio, 0.12)) * 1.8;
    float2 polar = float2(distanceFromCenter, spiralAngle / 6.2831853 + 0.5);
    float noiseA = tex2D(NebulaSampler, polar * float2(5, 3) + globalTime * float2(-0.22, 0.035)).r;
    float noiseB = tex2D(NebulaSampler, polar * float2(9, 5) + globalTime * float2(-0.14, -0.025)).r;
    // 恢复原球的窄亮边和独立白核；噪声仅轻微扰动轮廓，不再铺成均匀发亮的球面。
    float edgeNoise = tex2D(NebulaSampler, polar * float2(3, 4) + globalTime * float2(-0.45, 0)).r;
    float distance = distanceFromCenter + (edgeNoise - 0.5) * 0.012;
    float outerGlow = 0.05 / max(abs(distance - 0.25), 0.004)
        * (1 - smoothstep(0.25, 0.4, distance));
    float innerGlow = 0.36 / max(distanceFromCenter, 0.004)
        * (1 - smoothstep(0, 0.08, distanceFromCenter));
    // 参考羲和太阳的径向衰减和角向明暗分层；只让流动光带越过白色阈值。
    float radialFlow = spiralAngle;
    float bands = sin(radialFlow * 17 + noiseA * 1.6)
        + cos(radialFlow * 29 + noiseB * 1.2) * 0.65;
    float streams = pow(saturate(bands * 0.38 + 0.65), 2) * 3.2;
    float interiorMask = 1 - smoothstep(0.22, 0.25, distanceFromCenter);
    streams *= smoothstep(0.025, 0.075, distanceFromCenter) * interiorMask;
    float rotation = -globalTime * 0.42 + log(max(radiusRatio, 0.12)) * 1.8;
    float2 flow = float2(offset.x * cos(rotation) - offset.y * sin(rotation),
        offset.x * sin(rotation) + offset.y * cos(rotation)) * 4;
    float stars = (Starfield(flow, 6, 0) + Starfield(flow * 1.17, 9, 17.3) * 0.6) * interiorMask;
    float3 gold = float3(1, 0.82, 0.06);
    float3 starlight = float3(1, 0.98, 0.8);
    // 内部采用不透明的赤金/橙色底层；仅外沿辉光保持加法叠加，光带间隙不再透出场景。
    float bodyOpacity = 1 - smoothstep(0.24, 0.252, distanceFromCenter);
    float armWarmth = sin(spiralAngle * 17 + noiseA * 1.6) * 0.5 + 0.5;
    float warmth = saturate(armWarmth * 0.75 + noiseA * 0.15 + noiseB * 0.1);
    float3 bodyColor = lerp(float3(0.95, 0.22, 0.015), float3(1, 0.48, 0.025), warmth);
    float3 innerGold = lerp(gold, float3(1, 0.35, 0.02), bodyOpacity * 0.85);
    float3 color = bodyColor * bodyOpacity + innerGold * (outerGlow + interiorMask * 0.12)
        + starlight * (innerGlow + streams + stars * 0.8);
    // AlphaBlend 使用预乘颜色：底层遮挡背景，额外亮度继续产生金白辉光。
    return float4(color * sampleColor.rgb * sampleColor.a, bodyOpacity * sampleColor.a);
}
technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
