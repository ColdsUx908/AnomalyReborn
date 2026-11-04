sampler uImage1 : register(s1);
float2 uScreenPosition;
float4x4 uInverseViewMatrix;
float2 uCenter;
float uRadius;
float uFlowTime;
float uOpacity;
float3 uColor;

// Original world-space distance envelope and independently generated Wave2 noise.
float4 Main(float4 screenPos : VPOS, float2 coords : TEXCOORD0, float4 sampleColor : COLOR0) : COLOR0
{
    float2 world = mul(float4(screenPos.xy, 0, 1), uInverseViewMatrix).xy + uScreenPosition;
    float2 uv = (world - uCenter) / 620.0;
    float first = tex2D(uImage1, uv + float2(uFlowTime * 0.023, -uFlowTime * 0.031)).r;
    float second = tex2D(uImage1, uv * 1.37 + float2(-uFlowTime * 0.019, uFlowTime * 0.017) + 0.31).r;
    float third = tex2D(uImage1, uv * 0.79 + float2(uFlowTime * 0.011, uFlowTime * 0.026) + 0.67).r;
    float mesh = first * 0.45 + second * 0.35 + third * 0.20;
    float distanceToEdge = length(world - uCenter) - uRadius;
    // Clip against the exact radius before adding noise: no mist or rim inside the arena.
    float mistDistance = distanceToEdge + (mesh - 0.5) * 18.0;
    float outsideMask = smoothstep(0.0, 2.0, distanceToEdge);
    float envelope = outsideMask
        * (1.0 - smoothstep(0.0, 300.0, mistDistance));
    float folds = pow(saturate(1.0 - mesh), 1.5);
    float density = envelope * (0.12 + folds * 0.72);
    float rim = outsideMask * exp(-max(distanceToEdge, 0.0) / 16.0) * (0.12 + folds * 0.20);
    float3 gold = lerp(uColor * 0.80, float3(1.0, 0.84, 0.32), folds);
    return float4(gold * (density + rim) * uOpacity * sampleColor.rgb, 0.0);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
