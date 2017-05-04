sampler s0;
texture t0;
sampler LightSampler = sampler_state { Texture = <t0>; };

float4 PixelShaderLight(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 coords: TEXCOORD0) : SV_TARGET0
{
	//float4 color = tex2D(s0, coords);
	//float4 lightColor = tex2D(lightSampler, coords);
	//return color * lightColor;
	float4 color = tex2D(s0, coords);
	float4 lightColor = tex2D(LightSampler, coords);
	//float4 lightColor = LightMask.Sample(LightSampler, coords.xy);
	return color * lightColor;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_4_0_level_9_3 PixelShaderLight();
	}
}