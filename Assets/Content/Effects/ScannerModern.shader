Shader "Scanner/Cold Space"
{
	HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	/*float _fadeAmount;
	float _quantizationStrength;
	float3 _lightQuantizationParams;*/

	float3 HUEtoRGB(in float H)
	{
		float R = abs(H * 6 - 3) - 1;
		float G = 2 - abs(H * 6 - 2);
		float B = 2 - abs(H * 6 - 4);
		return saturate(float3(R,G,B));
	}

	float Epsilon = 1e-10;
 
	float3 RGBtoHCV(in float3 RGB)
	{
		// Based on work by Sam Hocevar and Emil Persson
		float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
		float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
		float C = Q.x - min(Q.w, Q.y);
		float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
		return float3(H, C, Q.x);
	}

	float3 HSLtoRGB(in float3 HSL)
	{
		float3 RGB = HUEtoRGB(HSL.x);
		float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
		return (RGB - 0.5) * C + HSL.z;
	}

	float3 RGBtoHSL(in float3 RGB)
	{
		float3 HCV = RGBtoHCV(RGB);
		float L = HCV.z - HCV.y * 0.5;
		float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
		return float3(HCV.x, S, L);
	}

	float4 _ColorA;
	float4 _ColorB;
	float4 _Remap;

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float2 screenPos = _ScreenParams.xy;
	
		float2 sampleUV = i.texcoord;
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sampleUV);

		// float lm = dot(float3(0.299, 0.587, 0.114), color.rgb);
		float3 hsl = RGBtoHSL(color);

		float lm = hsl.z;
		
		float hue = lerp(_Remap.x, _Remap.y, lm);
		float sat = saturate(lerp(_Remap.z, _Remap.w, lm));
		
		color.rgb = HSLtoRGB(float3(hue, sat, lm));

		
		

		// color.rgb = lerp(_ColorA, _ColorB, luma1);
		// color.rgb = float3(luma1, luma1, luma1);

		// color.rgb = 1 - color.rgb;
		return color; //  return half4(1, 0, 0, 1);
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
