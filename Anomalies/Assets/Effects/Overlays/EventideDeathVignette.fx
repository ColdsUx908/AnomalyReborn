float4x4 TransformMatrix;
float2 Resolution;
float Opacity;
struct VertexInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};
struct PixelInput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};
PixelInput Transform(VertexInput input)
{
    PixelInput output;
    output.Position = mul(input.Position, TransformMatrix);
    output.UV = input.UV;
    return output;
}
float4 Main(PixelInput input) : COLOR0
{
    // 全部边距按屏幕高度统一尺度；宽屏不会横向拉伸过渡带。
    float2 margin = min(input.UV, 1.0 - input.UV) * Resolution / Resolution.y;
    float horizontal = 1.0 - smoothstep(0.005, 0.24, margin.x);
    float vertical = 1.0 - smoothstep(0.005, 0.24, margin.y);
    float mask = 1.0 - (1.0 - horizontal) * (1.0 - vertical);
    float alpha = mask * Opacity;
    return float4(float3(0.008, 0.001, 0.016) * alpha, alpha);
}
technique Technique1
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 Transform();
        PixelShader = compile ps_3_0 Main();
    }
}
