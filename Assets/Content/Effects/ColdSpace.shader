Shader "Scanner/Cold Space"
{
	HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	float4 maintex(float2 uv) {
        return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    }
	
	float4 _ColorA;
	float4 _ColorB;
	float4 _Remap;

	float _Aberration;

	float4 _CorrectionRamp;

	float _Scanline;

	float _HexRadius;
	uniform float _HexAnimProgress;
	uniform float _HexAnimSeed;

	#include "Packages/com.k3.core/ShaderLib/utils.hlsl"
	#include "Packages/com.k3.core/ShaderLib/colors.hlsl"
	#include "Packages/com.k3.core/ShaderLib/hex.hlsl"

	float3 correct(float3 color) {
		float3 hsl = RGBtoHSL(color);
		float lm = hsl.z;
		float newHue = lerp(_Remap.x, _Remap.y, lm);
		float newSat = saturate(lerp(_Remap.z, _Remap.w, lm));
		float3 corrected = HSLtoRGB(float3(newHue, newSat, lm));

		float correctionAmount = remapClamped(hsl.g, _CorrectionRamp.x, _CorrectionRamp.y, _CorrectionRamp.w, _CorrectionRamp.z);

		correctionAmount = smoothstep(_CorrectionRamp.w, _CorrectionRamp.z, hsl.g);
		correctionAmount = lerp(_CorrectionRamp.x, _CorrectionRamp.y, correctionAmount);

		return lerp(color, corrected, saturate(correctionAmount));
	}

	#define screen _ScreenParams.xy

	float3 SampleColor(float2 uv) {
		if (_Aberration > 0) {
			float2 offsetR = float2(-1,0);
			float2 offsetG = float2(1,0);
			float2 offsetB = (0); //float2(-2,0);
			
			float r = correct(maintex(uv + offsetR / screen * _Aberration)).r;
			float g = correct(maintex(uv + offsetG / screen * _Aberration)).g;
			float b = correct(maintex(uv + offsetB / screen * _Aberration)).b;

			return float3(r,g,b);
		} else {
			float3 original = maintex(uv).rgb;
			return correct(original);
		}
	}

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float2 uv = i.texcoord;
		float2 hex = hexround(  pixel_to_hex(uv * screen, _HexRadius) );
		float4 color = (0);
		float randid = -1.0 + random2(hex + _HexAnimSeed) + _Time.y * 1.5 ;
		color.rgb = SampleColor(uv);

		if (_HexRadius > 0.01 && randid  < 0) {
			float2 centerOfHex = hex_to_pixel(hex, _HexRadius) / screen;			
			float3 centerHexColor = correct(maintex(centerOfHex).rgb);
			// color.rgb = lerp(color.rgb, centerHexColor, -randid);
			color.rgb = centerHexColor;
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
