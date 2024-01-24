#ifndef HOLOGRAM_SFX_PROCESSOR
#define HOLOGRAM_SFX_PROCESSOR
#endif

struct HologramSFXInput
{
    half4 color;
    float2 uv;
};

half _HologramSFXSpeed;
half _HologramSFXDensity;
half _HologramSFXBrightness;
half4 _HologramSFXColor;

half2 _HologramJitterThreshold; // x for Vertex, y for UV
half2 _HologramJitterSpeedRadio; // x for Vertex, y for UV
half4 _HologramJitterRangeVertex; // x,y for Vertex, z,w for UV
half2 _HologramJitterOffset; // x for Vertex, y for UV

half2 HologramSFXJitterOffsetVertex(half2 vertex, half2 direction)
{
    half optTime = abs(sin(_Time.w * _HologramJitterSpeedRadio.x));
    half timeToJitter = step(_HologramJitterThreshold.x, optTime);
    half jitterPos = vertex.y + sin(_SinTime.y * UNITY_PI) * _HologramJitterRangeVertex.y;
    half jitterPosRange = step(_HologramJitterRangeVertex.x, jitterPos) * step(jitterPos, _HologramJitterRangeVertex.y);
    half offset = jitterPosRange * _HologramJitterOffset.x * timeToJitter * _SinTime.y;
    
    return offset * direction;
}

float2 HologramSFXJitterOffseUV(float2 uv, float2 texelSize, float2 direction)
{
    float optTime = abs(sin(_Time.w * _HologramJitterSpeedRadio.y));
    float timeToJitter = step(_HologramJitterThreshold.y, optTime);
    float jitterPos = uv.y + sin(_SinTime.y * UNITY_PI) * _HologramJitterRangeVertex.w;
    float jitterPosRange = step(_HologramJitterRangeVertex.z, jitterPos) * step(jitterPos, _HologramJitterRangeVertex.w);
    float2 offset = jitterPosRange * _HologramJitterOffset.y * texelSize * timeToJitter * _SinTime.y;
    
    return offset * direction;
}

half4 HologramSFX(HologramSFXInput input)
{
    float mask = frac((input.uv.y - _Time.y * _HologramSFXSpeed) * _HologramSFXDensity);
    mask = (sign(mask - 0.5) * (mask - 0.5) * 2 + 1) / 2;

    half4 color = input.color * _HologramSFXColor;

    return lerp(color, half4(color.xyz * (mask + 1) * _HologramSFXBrightness, color.a), mask);
}