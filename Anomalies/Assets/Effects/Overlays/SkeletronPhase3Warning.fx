float4x4 uTransformMatrix;
float2 uScreenPosition;
float4x4 uInverseViewMatrix;
float2 uCenter;
float uArenaRadius;
float uRadius;
float uGapAngle;
float uGapHalfAngle;
float uGapPeriod;
float uAge;
float uDirection;
float uAngularSpeed;
float uPreparationTime;
float uRadialSpeed;
float uOpacity;

struct VertexOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

VertexOutput Transform(float4 position : POSITION0, float4 color : COLOR0, float2 uv : TEXCOORD0)
{
    VertexOutput output;
    output.Position = mul(position, uTransformMatrix);
    output.Color = color;
    output.UV = uv;
    return output;
}

float Hash(float2 p)
{
    float3 mixed = frac(p.xyx * 0.1031);
    mixed += dot(mixed, mixed.yzx + 33.33);
    return frac((mixed.x + mixed.y) * mixed.z);
}

float Grain(float2 p)
{
    float2 cell = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(Hash(cell), Hash(cell + float2(1, 0)), f.x),
        lerp(Hash(cell + float2(0, 1)), Hash(cell + 1.0), f.x), f.y);
}

float Fracture(float distanceToLine, float grain)
{
    float core = 1.0 - smoothstep(1.0, 4.0, abs(distanceToLine));
    float splinters = (1.0 - smoothstep(4.0, 17.0, abs(distanceToLine))) * smoothstep(0.38, 0.77, grain);
    return core * (0.4 + grain * 0.6) + splinters * 0.4;
}

float4 Main(float4 screenPos : VPOS, float2 coords : TEXCOORD0, float4 sampleColor : COLOR0, uniform float mode) : COLOR0
{
    // World pixels: pattern scale and flow speed are independent of viewport aspect and zoom.
    float2 local = mul(float4(screenPos.xy, 0, 1), uInverseViewMatrix).xy + uScreenPosition - uCenter;
    float radius = length(local);
    if (radius > uArenaRadius + 110.0 || uOpacity <= 0.0)
        return float4(0, 0, 0, 0);
    float grain = Grain(local / 19.0 + float2(uAge * 0.025, -uAge * 0.05));
    // Rain geometry freezes during its final fade; vertical flow and opacity can continue.
    float shapeAge = mode < 0.5 ? min(uAge, uPreparationTime) : uAge;
    float jagged = (Grain(local / 47.0 + shapeAge * 0.013) - 0.5) * 9.0;
    float mask = 0.0;
    if (mode < 0.5)
    {
        float lane = abs(frac((-abs(local.x) - uGapAngle) / 640.0 + 0.5) * 640.0 - 320.0);
        float danger = smoothstep(144.0, 153.0, lane);
        // Ragged channel shoulders plus independently broken, falling strands in dangerous columns.
        float shoulderDistance = abs(lane - 155.0 + jagged * 0.6);
        float shoulderCore = 1.0 - smoothstep(3.0, 8.0, shoulderDistance);
        float shoulderGlow = 1.0 - smoothstep(8.0, 28.0, shoulderDistance);
        float shoulder = shoulderCore * (0.85 + grain * 0.35) + shoulderGlow * (0.22 + grain * 0.2);
        float slot = floor(local.x / 75.0);
        float strandX = (slot + 0.2 + Hash(float2(slot, 9.3)) * 0.6) * 75.0;
        float strands = Fracture((local.x - strandX + jagged) * 0.65, grain);
        float falling = smoothstep(0.3, 0.8, Grain(float2(slot * 7.1, (local.y - uAge * 13.0) / 54.0)));
        float flow = Grain(float2(local.x / 85.0, (local.y - uAge * 10.0) / 130.0));
        float veil = (0.08 + flow * 0.2) * (0.65 + falling * 0.35);
        mask = danger * (shoulder + strands * falling * 0.8 + veil);
    }
    else
    {
        float angle = atan2(local.y, local.x);
        float gapDistance = abs(frac((angle - uGapAngle) / uGapPeriod + 0.5) * uGapPeriod - uGapPeriod * 0.5);
        // Keep all ragged detail on the dangerous side; do not paint across safe gaps.
        float outsideGap = smoothstep(uGapHalfAngle, uGapHalfAngle + 0.025, gapDistance);
        float normalAngle = (floor(angle / 0.3926990817) + 0.5) * 0.3926990817;
        float polygonScale = 0.9807852804 / cos(angle - normalAngle);
        if (mode < 1.5)
        {
            float rail = radius - uRadius * polygonScale;
            float teeth = Grain(float2(angle * uRadius / 16.0 - uDirection * uAge * uAngularSpeed * uRadius / 16.0, uRadius / 40.0));
            mask = outsideGap * Fracture(rail + jagged, grain) * (0.55 + teeth * 0.45);
        }
        else
        {
            float birthAngle = angle - uDirection * uAge * uAngularSpeed;
            float birthNormal = (floor(birthAngle / 0.3926990817) + 0.5) * 0.3926990817;
            float boundary = uArenaRadius * 0.9807852804 / cos(birthAngle - birthNormal) + 80.0;
            float waveRadius = boundary - max(0.0, uAge - uPreparationTime) * uRadialSpeed;
            float front = radius - waveRadius;
            float spiral = sin(angle * 11.0 + radius / 45.0 - uDirection * uAge * 0.12);
            float streams = Fracture(spiral * 23.0 + jagged, grain);
            float envelope = smoothstep(-160.0, -110.0, front) * (1.0 - smoothstep(-15.0, 28.0, front));
            mask = outsideGap * (Fracture(front + jagged, grain) * 0.8 + streams * envelope * 0.45);
        }
    }
    float opacity = saturate(mask * uOpacity * 0.9);
    float3 ivory = lerp(float3(0.76, 0.58, 0.36), float3(1.0, 0.97, 0.86), saturate(mask));
    return float4(ivory * opacity, opacity) * sampleColor;
}

technique Technique1
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 Transform();
        PixelShader = compile ps_3_0 Main(0);
    }
    pass Patrol
    {
        VertexShader = compile vs_3_0 Transform();
        PixelShader = compile ps_3_0 Main(1);
    }
    pass Tide
    {
        VertexShader = compile vs_3_0 Transform();
        PixelShader = compile ps_3_0 Main(2);
    }
}
