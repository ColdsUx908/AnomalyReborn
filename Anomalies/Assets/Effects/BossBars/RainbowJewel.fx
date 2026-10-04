sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float2 uScreenResolution;
float2 uPosition;
float uTime;

float3 GetColor(float2 screenCoords)
{
    // 定义七种颜色：赤橙黄绿青蓝紫
    float3 colorRed = float3(0.9, 0.0, 0.0); // 赤
    float3 colorOrange = float3(0.8, 0.5, 0.1); // 橙
    float3 colorYellow = float3(0.8, 0.8, 0.0); // 黄
    float3 colorGreen = float3(0.0, 0.9, 0.0); // 绿
    float3 colorCyan = float3(0.0, 0.8, 1.0); // 青
    float3 colorBlue = float3(0.0, 0.0, 0.9); // 蓝
    float3 colorPurple = float3(0.5, 0.1, 0.8); // 紫

    // 纹理扰动
    float txt1 = tex2D(uImage1, screenCoords * float2(7.2, 13.8) + float2(uTime * -0.25, uTime * -0.1)).r;
    float txt2 = tex2D(uImage1, screenCoords * float2(5.2, 10.8) + float2(uTime * -0.25, uTime * -0.1)).r;
    float txt = txt1 * 0.7 + pow(max(txt2, 0), 0.4) * 0.6;

    // 条带参数
    float length = 0.25; // 条带长度控制
    float speed = -0.25; // 移动速度
    float stripes = 7.0; // 条带数
    float t = frac(screenCoords.x * length * stripes + uTime * speed) * stripes; // [0, 7)

    // 根据 t 所在区间进行颜色插值
    float3 base;
    if (t < 1.0)
        base = lerp(colorRed, colorOrange, t); // 赤→橙
    else if (t < 2.0)
        base = lerp(colorOrange, colorYellow, t - 1.0); // 橙→黄
    else if (t < 3.0)
        base = lerp(colorYellow, colorGreen, t - 2.0); // 黄→绿
    else if (t < 4.0)
        base = lerp(colorGreen, colorCyan, t - 3.0); // 绿→青
    else if (t < 5.0)
        base = lerp(colorCyan, colorBlue, t - 4.0); // 青→蓝
    else if (t < 6.0)
        base = lerp(colorBlue, colorPurple, t - 5.0); // 蓝→紫
    else
        base = lerp(colorPurple, colorRed, t - 6.0); // 紫→赤（循环）

    base *= 0.6 + txt * 0.7;
    base += txt1 * 0.3;
    base *= 1.7 - txt2 * 0.85 - txt1 * 0.5;
    base = clamp(base * 1.1, 0.25, 1.2);
    return base;
}

float4 Main(float4 screenPos : VPOS, float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    float2 screenCoords = (screenPos.xy - uPosition) / uScreenResolution;
    float alpha = tex2D(uImage0, coords).a;
    float3 gradientColor = GetColor(screenCoords);
    return float4(gradientColor, 1.0) * alpha * sampleColor;
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}