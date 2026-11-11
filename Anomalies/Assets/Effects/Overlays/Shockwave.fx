float3 uColor;
float uTime;
float uOpacity;
float uRadius;
float uDrawRadius;

float MainRidge(float distance, float width)
{
    return exp2(-abs(distance) / width);
}

float2 RotateHarmonic(float2 direction)
{
    return float2(direction.x * direction.x - direction.y * direction.y, 2.0 * direction.x * direction.y);
}

float4 Main(float2 uv : TEXCOORD0) : COLOR0
{
    float2 position = (uv * 2.0 - 1.0) * uDrawRadius;
    float radius = length(position);
    float2 direction = position / max(radius, 0.001);
    float2 harmonic4 = RotateHarmonic(RotateHarmonic(direction));
    float2 harmonic8 = RotateHarmonic(harmonic4);
    float2 harmonic16 = RotateHarmonic(harmonic8);
    float2 harmonic32 = RotateHarmonic(harmonic16);
    float phase = uTime * 3.0;
    float2 sweep = float2(cos(phase), sin(phase));
    float emergence = saturate(radius / 220.0);
    float ripple = 6.0 * dot(harmonic16, sweep) + 3.0 * harmonic32.y;
    float front = radius - uRadius - ripple * emergence;
    float filaments = pow(saturate(0.5 + 0.5 * dot(harmonic32, sweep.yx)), 4.0);
    float flares = pow(saturate(0.5 + 0.5 * dot(harmonic8, sweep)), 12.0);
    float wake = MainRidge(front + 40.0, 44.0) * (0.15 + filaments * 0.24);
    float echoes = MainRidge(front + 38.0 + 8.0 * harmonic16.y, 5.0) * 0.36
        + MainRidge(front + 82.0 + 13.0 * dot(harmonic8, sweep.yx), 7.0) * 0.17;
    float corona = MainRidge(front, 20.0 + flares * 37.0) * (0.34 + flares * 0.29);
    float core = MainRidge(front, 3.0) * (0.61 + filaments * 0.39);
    float sparks = flares * pow(0.5 + 0.5 * sin(front * 0.19 - uTime * 18.0), 12.0)
        * MainRidge(front - 25.0, 24.0) * 0.55;
    float3 highlight = lerp(uColor, float3(1.0, 0.98, 0.91), 0.77);
    float3 color = uColor * (corona + (echoes + wake) * emergence) + highlight * (core + sparks);
    float edgeFade = 1.0 - smoothstep(uDrawRadius - 40.0, uDrawRadius, radius);
    float centerFade = smoothstep(0.0, 22.0, radius);
    return float4(saturate(color), uOpacity * edgeFade * centerFade);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 Main();
    }
}
