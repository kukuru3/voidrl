Shader "Scanner/Sci-Fi Overlay"
{
	HLSLINCLUDE

	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	float4 maintex(float2 uv) {
        return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    }

	float _Scanline;
	float _HexRadius;
	uniform float _HexAnimProgress;
	uniform float _HexAnimSeed;

	uniform sampler2D _RenderMetadata;



	#include "Packages/com.k3.core/ShaderLib/utils.hlsl"
	#include "Packages/com.k3.core/ShaderLib/colors.hlsl"
	#include "Packages/com.k3.core/ShaderLib/hex.hlsl"
	#define screen _ScreenParams.xy

	float4 metadata(float2 uv) {
		uv.x = 0.5 + (uv.x - 0.5) * screen.x / screen.y;
		return tex2D(_RenderMetadata, uv);
	}
	
	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float2 uv = i.texcoord;
		float2 hex = hexround(  pixel_to_hex(uv * screen, _HexRadius) );

		float4 color = (0);
				
		float randid = random2(hex + _HexAnimSeed);
		
		float2 centerOfHex = hex_to_pixel(hex, _HexRadius) / screen;
		
		float hexBoost = metadata(centerOfHex).b;

		float hexProgress = -1.0 + randid + _Time.y * 1.5 ;

		if (hexBoost > 0.001) {
			//return float4(hexBoost,0,0,1);
			hexProgress = -1.0 + randid + (1.0 - hexBoost) * 1;
		}

		if (_HexRadius > 0.01 && hexProgress < 0) {
			float3 centerHexColor = maintex(centerOfHex).rgb;
			color.rgb = centerHexColor;
		} else {
			color = maintex(uv);
		}

		float mult = step((uv * screen).y % 2, 1.0) * (1.0 + pow(_Scanline, 0.33)); // dunno why the 0.2 works, it just does. 
		color.rgb *= lerp(1.0, mult, _Scanline);
		
		return color;
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM
				#pragma vertex VertDefault
				#pragma fragment Frag
			ENDHLSL
		}
	}
}
