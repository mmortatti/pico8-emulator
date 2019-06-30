#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#pragma enable_opengl_debug_symbols

texture SpriteTexture;
texture picoPalette;
texture bitwiseAnd;

sampler s0;
sampler s1;
sampler s2;

sampler SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

sampler picoPaletteSampler = sampler_state
{
    Texture = <picoPalette>;
};

sampler bitwiseAndSampler = sampler_state
{
	Texture = <bitwiseAnd>;
};

float AND(float x, float y) {
	return tex2D(s1, float2(x, y)).a;
}

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    int colorValue = tex2D(s0,input.TextureCoordinates).a * 255;
    int x = input.TextureCoordinates.x * 128;

    if (x % 2 == 0) {
        colorValue = AND(colorValue, (int)0x0f);
    }
    else {
        colorValue = colorValue / 16;
    }

	return float4((float)colorValue / 255, (float)colorValue / 255, (float)colorValue / 255, 1);

	/*return tex2D(picoPaletteSampler, tex2D(SpriteTextureSampler,input.TextureCoordinates));*/
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};