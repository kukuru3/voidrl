Shader "Scanner/Postprocessing/Scannerize"
{
	HLSLINCLUDE

#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	/*float _fadeAmount;
	float _quantizationStrength;
	float3 _lightQuantizationParams;*/

	// #include "hsvIncludes.hlsl"

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		float2 screenPos = _ScreenParams.xy;
	
		float2 sampleUV = i.texcoord;
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sampleUV);
		//color = lerp(color, float4(0,0,0,1), _fadeAmount);

		//float3 hsv = RGBtoHSV(color.rgb);

		//float E = _lightQuantizationParams.x; // exponent
		//float B = _lightQuantizationParams.y; // num brackets in the [0..1] range

		//float b = pow(abs(hsv.z + _lightQuantizationParams.z), 1.0 / E) * B; // calculate bracket
		//b = floor(b); // floor it cause we are quantizing
		//hsv.z = pow(abs(b / B), E); // set new luminance

		//// if (hsv.z < 0.1) hsv.z = 0; else hsv.z = 2;

		//color.rgb = lerp(color.rgb, HSVtoRGB(hsv), _quantizationStrength);

		color.rgb = 1 - color.rgb;
		return color; //  return half4(1, 0, 0, 1);

		// return color;
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
