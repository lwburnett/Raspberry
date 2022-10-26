﻿#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D InsideTexture;
Texture2D OutsideTexture;

float2 InsideUBounds;
float2 InsideVBounds;
float2 OutsideUBounds;
float2 OutsideVBounds;

float2 SpritePositionTopLeft;
float2 SpriteDimensions;
float2 ScreenDimensions;
float2 PlayerPosition;
float ProximityRadius;

sampler2D InsideTextureSampler = sampler_state
{
	Texture = <InsideTexture>;
};

sampler2D OutsideTextureSampler = sampler_state
{
	Texture = <OutsideTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

// This avoids an overflow that happens because floats are capped at 2^14
float GetDistSafe(float2 Point1, float2 Point2)
{
	float2 adjustedPoint1 = Point1 / ScreenDimensions.y;
	float2 adjustedPoint2 = Point2 / ScreenDimensions.y;

	float2 diffs = adjustedPoint2 - adjustedPoint1;

	float interTerm = dot(diffs, diffs);
	float adjustedDist = sqrt(interTerm);

	return adjustedDist * ScreenDimensions.y;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	const float lerpUValue = (input.TextureCoordinates.x - InsideUBounds.x) / (InsideUBounds.y - InsideUBounds.x);
	const float lerpVValue = (input.TextureCoordinates.y - InsideVBounds.x) / (InsideVBounds.y - InsideVBounds.x);

	const float outsideU = lerp(OutsideUBounds.x, OutsideUBounds.y, lerpUValue);
	const float outsideV = lerp(OutsideVBounds.x, OutsideVBounds.y, lerpVValue);
	
	float4 insideColor = tex2D(InsideTextureSampler, input.TextureCoordinates);
	float4 outsideColor = tex2D(OutsideTextureSampler, float2(outsideU, outsideV));

	float4 result = outsideColor;

	const float2 thisPixelPos = float2(SpritePositionTopLeft.x + (lerpUValue * SpriteDimensions.x), SpritePositionTopLeft.y + lerpVValue * SpriteDimensions.y);
	const float dist = GetDistSafe(thisPixelPos, PlayerPosition);

	if (dist <= ProximityRadius)
	{
		result = insideColor;
	}

	return result;
}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};