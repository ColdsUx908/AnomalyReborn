float4x4 TransformMatrix;
float Time;
float Opacity;
float Remaining;
float CompanionPresence;

struct VertexInput { float4 Position : POSITION0; float4 Color : COLOR0; float2 UV : TEXCOORD0; };
struct PixelInput { float4 Position : POSITION0; float4 Color : COLOR0; float2 UV : TEXCOORD0; };
PixelInput Transform(VertexInput input)
{
    PixelInput output;
    output.Position = mul(input.Position, TransformMatrix);
    output.Color = input.Color;
    output.UV = input.UV;
    return output;
}
float Band(float distance, float width)
{
    float normalized = distance / width;
    return exp(-normalized * normalized);
}
float4 Main(PixelInput input) : COLOR0
{
    // 方形世界坐标网格：不依赖屏幕长宽比。中央留空给目标及死亡四角星。
    float breath = 0.96 + 0.015 * sin(Time * 2.8);
    float2 p = (input.UV - 0.5) * 2.0 / breath;
    float r = length(p);
    float angle = atan2(p.y, p.x);
    float rotation = Time * 0.32;
    float toothPhase = (angle - rotation) * 18.0;
    // 平顶、圆角的十八枚轮齿，青蓝外刃与深靛齿体。
    float tooth = smoothstep(-0.15, 0.3, cos(toothPhase));
    float outerRadius = 0.75 + 0.095 * tooth;
    float outside = 1.0 - smoothstep(outerRadius - 0.008, outerRadius + 0.008, r);
    float inside = smoothstep(0.635, 0.65, r);
    float gearBody = outside * inside;
    // 窄亮芯配少量外光，利用导数抗锯齿维持轮齿边界清楚。
    float edgeAA = max(fwidth(r - outerRadius), 0.0015);
    float outerEdge = 1.0 - smoothstep(0.007, 0.007 + edgeAA, abs(r - outerRadius));
    float innerAA = max(fwidth(r), 0.0015);
    float innerEdge = 1.0 - smoothstep(0.006, 0.006 + innerAA, abs(r - 0.645));
    float glint = pow(saturate(0.5 + 0.5 * cos(angle * 3.0 - Time * 0.85)), 6.0);
    // 两条反相闭合正弦曲线在环上交织成连续无穷链；首尾函数值与切线均一致。
    float weave = 0.045 * sin(angle * 6.0 + rotation);
    float threadA = Band(r - (0.565 + weave), 0.013);
    float threadB = Band(r - (0.565 - weave), 0.013);
    float threads = max(threadA, threadB);
    // 从十二点钟顺时针消退，首尾软化；完整五秒时没有可见接缝。
    float clock = frac((angle + 1.570796327) / 6.283185307 + 1.0);
    float progressMask = 1.0 - smoothstep(Remaining - 0.009, Remaining + 0.009, clock);
    float progressRing = Band(r - 0.48, 0.008) * progressMask;
    float centerClear = smoothstep(lerp(0.32, 0.41, CompanionPresence), 0.46, r);
    float glow = (Band(r - outerRadius, 0.018) * 0.10
        + max(Band(r - (0.565 + weave), 0.016), Band(r - (0.565 - weave), 0.016)) * 0.065)
        * lerp(1.0, 0.58, CompanionPresence);
    float energy = outerEdge * 1.15 + innerEdge * 0.75 + threads * 0.82 + progressRing * 0.85;
    float3 indigo = float3(0.10, 0.055, 0.30);
    float3 violet = float3(0.43, 0.25, 0.95);
    float3 blue = float3(0.40, 0.62, 1.0);
    float3 palette = lerp(violet, blue, 0.5 + 0.5 * sin(angle * 2.0 + Time * 0.45));
    // 深色窄描边隔开 NPC 纹理，冷白/青紫亮芯保证暗色和高频背景下均可辨。
    float stroke = max(max(outerEdge, innerEdge), max(threads, progressRing));
    float under = max(Band(r - outerRadius, 0.03), Band(r - 0.645, 0.024));
    under = max(under, max(Band(r - (0.565 + weave), 0.029), Band(r - (0.565 - weave), 0.029)));
    under = max(under, Band(r - 0.48, 0.023) * progressMask);
    float bounds = (1.0 - smoothstep(0.91, 0.99, r)) * centerClear;
    float alpha = saturate(max(under * 0.72, stroke * 0.98) + glow) * bounds * Opacity;
    float3 brightInk = lerp(palette, float3(0.86, 0.98, 1.0), 0.68);
    float3 color = float3(0.012, 0.018, 0.04) + brightInk * stroke + palette * glow;
    return float4(color * alpha, alpha);
}
technique Technique1
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 Transform();
        PixelShader = compile ps_3_0 Main();
    }
}




