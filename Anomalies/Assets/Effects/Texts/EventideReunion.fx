sampler uImage0 : register(s0);
float2 uScreenResolution;
float2 uPosition;
float uTime;
float2 uTextSize;
float2 uTextScale;
float uTextRotation;

float Pulse(float value, float sharpness)
{
    return pow(saturate(0.5 + 0.5 * cos(value)), sharpness);
}

float3 GetAspectColor(float2 p, float theme)
{
    float2 uv = p / max(uTextSize, float2(1.0, 1.0));
    float t = uTime;
    // 所有空间图案都使用按字高统一缩放的局部坐标。
    float2 q = p / max(uTextSize.y, 1.0);
    if (theme < 1.5)
    {
        // 太阳：向上翻卷的金橙日焰和明亮日冕。
        float flame = sin(q.x * 15.0 + sin(q.y * 7.0 + t * 3.0) * 1.8 - t * 2.5);
        float heat = saturate(0.45 + flame * 0.3 + (1.0 - uv.y) * 0.25);
        float flare = Pulse(q.x * 10.0 + q.y * 4.0 - t * 4.0, 10.0);
        return lerp(float3(1.0, 0.30, 0.035), float3(1.0, 0.88, 0.25), heat) + flare * float3(0.12, 0.18, 0.22);
    }
    if (theme < 2.5)
    {
        // 轮回：两束蓝紫因果线相互交织，沿无尽环往复流动。
        float loop = sin(q.x * 7.0 - t * 2.0);
        float threadA = exp(-abs(q.y - 0.5 - loop * 0.28) * 16.0);
        float threadB = exp(-abs(q.y - 0.5 + loop * 0.28) * 16.0);
        float flow = Pulse(q.x * 13.0 + t * 3.0, 5.0);
        return lerp(float3(0.40, 0.26, 0.95), float3(0.62, 0.65, 1.0), 0.5 + loop * 0.5)
            + (threadA + threadB) * float3(0.22, 0.30, 0.30) * (0.4 + flow * 0.6);
    }
    if (theme < 3.5)
    {
        // 生命：翠绿叶脉向上生长，嫩芽亮点沿叶脉流动。
        float vein = Pulse(q.x * 12.0 + abs(q.y - 0.5) * 13.0 + t * 1.5, 12.0);
        float sap = Pulse(q.y * 10.0 + t * 2.5 + sin(q.x * 8.0), 5.0);
        return lerp(float3(0.20, 0.78, 0.16), float3(0.76, 1.0, 0.42), saturate((1.0 - uv.y) * 0.6 + sap * 0.3))
            + vein * float3(0.16, 0.14, 0.10);
    }
    if (theme < 4.5)
    {
        // 死亡：冷灰紫魂光与斜掠文字的镰刃，暗部保留可读亮度。
        float mist = sin(q.x * 5.0 + sin(q.y * 8.0 - t * 2.0) + t);
        float scythe = Pulse(q.x * 4.0 + q.y * 8.0 - t * 3.0, 22.0);
        return lerp(float3(0.43, 0.40, 0.65), float3(0.76, 0.75, 0.90), mist * 0.5 + 0.5)
            + scythe * float3(0.23, 0.20, 0.25);
    }
    if (theme < 5.5)
    {
        // 神圣：金白裁决光束与交叉的圣印闪光。
        float pillar = Pulse(q.x * 9.0 - t * 1.5, 18.0);
        float seal = pillar * Pulse(q.y * 12.0, 3.0);
        float radiance = 0.5 + 0.5 * sin(q.x * 3.0 + t * 2.0);
        return lerp(float3(1.0, 0.67, 0.16), float3(1.0, 0.96, 0.72), radiance * 0.65 + pillar * 0.35)
            + seal * float3(0.08, 0.12, 0.22);
    }
    if (theme < 6.5)
    {
        // 恒星：蓝紫星云向周期性星核收束，白蓝核心脉冲。
        float2 cell = float2(frac(q.x * 0.65) - 0.5, (q.y - 0.5) * 0.65);
        float radius = length(cell);
        float collapse = Pulse(radius * 30.0 + t * 4.0, 8.0);
        float core = exp(-radius * 14.0) * (0.7 + 0.3 * sin(t * 3.0));
        float nebula = 0.5 + 0.5 * sin(q.x * 6.0 + q.y * 5.0 - t);
        return lerp(float3(0.36, 0.28, 1.0), float3(0.28, 0.72, 1.0), nebula)
            + collapse * float3(0.16, 0.16, 0.16) + core * float3(0.45, 0.45, 0.25);
    }
    if (theme > 7.5)
    {
        // 混沌：非周期叠加的紫色涡流与游走裂隙，只改变字形内部颜色。
        float2 warp = q + float2(sin(q.y * 7.0 + t * 1.7), cos(q.x * 4.3 - t * 1.3)) * 0.22;
        float turbulence = sin(warp.x * 8.3 + sin(warp.y * 9.1 - t * 2.1))
            + sin(warp.x * 13.7 - warp.y * 6.4 - t * 1.41) * 0.5;
        float rift = pow(saturate(1.0 - abs(turbulence) * 5.0), 3.0);
        float eddy = 0.5 + 0.5 * sin(warp.x * 3.7 + warp.y * 5.3 + t * 0.83);
        float3 purple = lerp(float3(0.48, 0.20, 0.85), float3(0.85, 0.43, 1.0), eddy);
        return lerp(purple, float3(0.90, 0.78, 1.0), rift * 0.75);
    }
    // 六相轮转、终焉日升：宝石色流光汇入金白晨曦，亮带缓缓掠过整行。
    float wave = uv.x * 0.85 - t * 0.09 + sin(uv.y * 3.0 + t) * 0.04;
    float3 spectrum = 0.68 + 0.30 * cos(6.2831853 * (wave + float3(0.0, 0.33, 0.67)));
    float dawn = Pulse(uv.x * 6.2831853 - t * 1.2, 6.0);
    float glint = Pulse(q.x * 5.0 - q.y * 2.0 - t * 2.5, 24.0);
    return lerp(spectrum, float3(1.0, 0.86, 0.46), dawn * 0.7) + glint * float3(0.12, 0.12, 0.12);
}

float3 GetColor(float2 screenCoords)
{
    float3 colorBase = float3(1, 0.6, 0);
    float3 colorTop = float3(1, 0.4, 0);
    
    float coords = sin(screenCoords.x * 30 + uTime * 2.5) * 0.5 + 0.5;
    float3 gradientColor = lerp(colorBase, colorTop, coords);
    
    return gradientColor;
}

float4 Main(float4 screenPos : VPOS, float2 coords : TEXCOORD0, float4 sampleColor : COLOR0, uniform float theme) : COLOR0
{
    float alpha = tex2D(uImage0, coords).a;
    if (theme > 0.5)
    {
        float2 p = screenPos.xy - uPosition;
        float c = cos(uTextRotation);
        float s = sin(uTextRotation);
        p = float2(p.x * c + p.y * s, -p.x * s + p.y * c) / max(uTextScale, float2(0.001, 0.001));
        return float4(saturate(GetAspectColor(p, theme)), 1.0) * alpha * sampleColor;
    }
    float2 screenCoords = (screenPos.xy - uPosition) / uScreenResolution;
    // Preserve the 16:9 pattern scale while removing viewport aspect distortion.
    screenCoords.x *= (uScreenResolution.x / uScreenResolution.y) / (16.0 / 9.0);
    float3 gradientColor = GetColor(screenCoords);
    return float4(gradientColor, 1.0) * alpha * sampleColor;
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main(0.0);
    }
    pass Sun
    {
        PixelShader = compile ps_3_0 Main(1.0);
    }
    pass Samsara
    {
        PixelShader = compile ps_3_0 Main(2.0);
    }
    pass Life
    {
        PixelShader = compile ps_3_0 Main(3.0);
    }
    pass Death
    {
        PixelShader = compile ps_3_0 Main(4.0);
    }
    pass Sacredness
    {
        PixelShader = compile ps_3_0 Main(5.0);
    }
    pass Star
    {
        PixelShader = compile ps_3_0 Main(6.0);
    }
    pass FinalSun
    {
        PixelShader = compile ps_3_0 Main(7.0);
    }
    pass Chaos
    {
        PixelShader = compile ps_3_0 Main(8.0);
    }
}
