Shader "Scanner/Cold Space"
{
	HLSLINCLUDE

	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	uniform sampler2D _RenderMetadata;


	#include "Packages/com.k3.core/ShaderLib/utils.hlsl"
	#include "Packages/com.k3.core/ShaderLib/colors.hlsl"
	#include "Packages/com.k3.core/ShaderLib/hex.hlsl"

	float4 maintex(float2 uv) {
        return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    }
	#define screen _ScreenParams.xy
	float4 metadata(float2 uv) {
		uv.x = 0.5 + (uv.x - 0.5) * screen.x / screen.y;
		return tex2D(_RenderMetadata, uv);
	}
	
	float4 _Remap;
	float4 _CorrectionRamp;

	float _Aberration;


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



	float3 SampleColor(float2 uv) {
		float abb = _Aberration * (1.0 - metadata(uv).r);
		if (abb > 0) {
			float2 offsetR = float2(-1,0);
			float2 offsetG = float2(1,0);
			float2 offsetB = (0); //float2(-2,0);
			
			float r = correct(maintex(uv + offsetR / screen * abb)).r;
			float g = correct(maintex(uv + offsetG / screen * abb)).g;
			float b = correct(maintex(uv + offsetB / screen * abb)).b;

			return float3(r,g,b);
		} else {
			float3 original = maintex(uv).rgb;
			return correct(original);
		}
	}

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float2 uv = i.texcoord;
		float4 color = (0);
		color.rgb = SampleColor(uv);
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
