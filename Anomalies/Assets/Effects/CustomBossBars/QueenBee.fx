sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float2 uScreenResolution;
float uScreenRatio; //Width / Height
float2 uPosition;
float uTime;

float3 HexCoordRatioAndID(float2 uv)
{
    const float sqrt3 = 1.7320508;
    
    // 如果 uScreenRatio = width / height，使用 uv.x *= uScreenRatio 可以拉伸 X 轴使其保持正六边形
    uv.x *= uScreenRatio;
    
    // 尖顶六边形（竖直边）的仿射变换
    float2x2 M = float2x2(sqrt3, -1.0, 0.0, 2.0);
    float2 q = mul(M, uv);
    
    float2 r = floor(q);
    float2 f = frac(q) - 0.5;
    
    const float w00 = 1.0 / 3.0;
    const float w01 = 1.0 / 6.0;
    const float w11 = 1.0 / 3.0;
    
    // 计算 5 个候选邻居：原先的 3 个 + 缺失的 2 个
    float2 d0 = f;
    float2 d1 = f - float2(1.0, 0.0);
    float2 d2 = f - float2(0.0, 1.0);
    float2 d3 = f - float2(-1.0, 0.0); // 新增：左邻居
    float2 d4 = f - float2(0.0, -1.0); // 新增：下邻居

    float dist0 = d0.x * d0.x * w00 + 2.0 * d0.x * d0.y * w01 + d0.y * d0.y * w11;
    float dist1 = d1.x * d1.x * w00 + 2.0 * d1.x * d1.y * w01 + d1.y * d1.y * w11;
    float dist2 = d2.x * d2.x * w00 + 2.0 * d2.x * d2.y * w01 + d2.y * d2.y * w11;
    float dist3 = d3.x * d3.x * w00 + 2.0 * d3.x * d3.y * w01 + d3.y * d3.y * w11;
    float dist4 = d4.x * d4.x * w00 + 2.0 * d4.x * d4.y * w01 + d4.y * d4.y * w11;
    
    float2 bestD = d0;
    float2 bestC = r;
    float minDist = dist0;

    // 使用循环遍历更新，避免 if/else 嵌套出 bug
    float2 offsets[4] = { float2(1.0, 0.0), float2(0.0, 1.0), float2(-1.0, 0.0), float2(0.0, -1.0) };
    float dists[4] = { dist1, dist2, dist3, dist4 };
    float2 ds[4] = { d1, d2, d3, d4 };

    [unroll]
    for (int i = 0; i < 4; i++)
    {
        if (dists[i] < minDist)
        {
            minDist = dists[i];
            bestD = ds[i];
            bestC = r + offsets[i];
        }
    }
    
    float lenSq = bestD.x * bestD.x + bestD.y * bestD.y;
    if (lenSq < 1e-12)
        return float3(0.0, bestC.x, bestC.y);
    
    // 邻居方向
    float2 neighbors[6];
    neighbors[0] = float2(1.0, 0.0);
    neighbors[1] = float2(-1.0, 0.0);
    neighbors[2] = float2(0.0, 1.0);
    neighbors[3] = float2(0.0, -1.0);
    neighbors[4] = float2(1.0, -1.0);
    neighbors[5] = float2(-1.0, 1.0);
    
    float minT = 1e10;
    for (int j = 0; j < 6; ++j)
    {
        float2 n = neighbors[j];
        float nWn = n.x * n.x * w00 + 2.0 * n.x * n.y * w01 + n.y * n.y * w11;
        float dWn = (bestD.x * w00 + bestD.y * w01) * n.x +
                    (bestD.x * w01 + bestD.y * w11) * n.y;
        if (dWn > 0.0)
        {
            float t = nWn / (2.0 * dWn);
            if (t > 0.0 && t < minT)
                minT = t;
        }
    }
    
    float ratio = 1.0 / minT;
    ratio = clamp(ratio, 0.0, 1.0);
    
    return float3(ratio, bestC.x, bestC.y);
}

float3 GetColor(float2 screenCoords)
{
    float3 colorBase = float3(0.9, 0.9, 0);
    float3 colorTop = float3(0.9, 0.6, 0);
    
    float2 coords = screenCoords + float2(sin(screenCoords.x * 30 + uTime * 2.4), 0.0);
    float txt = tex2D(uImage1, screenCoords * float2(1.5, 3) + float2(uTime * -0.07, uTime * -0.12)).r;
    float txtVal = pow(txt, 0.9);
    float3 gradientColor = lerp(colorBase, colorTop, coords.x) * min(1.15, 0.8 + txtVal * 0.6);
    
    // ----- 六边形动态叠加（平移 + 呼吸缩放） -----
    float baseScale = 0.035; // 基础密度
    // 平移效果：整体网格缓慢漂移
    float2 offset = float2(uTime * 0.025, uTime * 0.015);
    float2 coords2 = screenCoords + offset;
    
    float3 hexLocal = HexCoordRatioAndID(coords2 / baseScale);
    float ratio = hexLocal.x; // OA/OB 比值，范围 [0, 1]
    float2 gridID = hexLocal.yz; // 六边形在变换空间中的整数行列索引
    
    // 计算边框强度（边缘发光线条）
    float hexIntensity;
    if (ratio > 0.6)
        hexIntensity = smoothstep(0.6, 1, ratio); // 边框强度，范围 [0, 1]
    else
        hexIntensity = 0;
    
    // 利用 gridID 生成伪随机值，控制覆盖率（约 30%）
    float randomVal = frac(sin(dot(gridID, float2(12.9898, 78.233))) * 43758.5453);
    float coverageMask = step(randomVal, 0.30);
    
    // 边框颜色也随时间微变
    float3 hexColor = float3(1, 0.5, 0) * (0.93 + 0.15 * sin(uTime * 0.1));
    
    // 叠加边框（乘以 mask 控制显隐）
    gradientColor = lerp(gradientColor, hexColor, hexIntensity * coverageMask);
    return gradientColor;
}

float4 PixelShaderFunction(float4 screenPos : VPOS, float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
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
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
