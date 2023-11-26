Shader "Scanner/Space Blur"
{
	HLSLINCLUDE

	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

	#include "Packages/com.k3.core/ShaderLib/utils.hlsl"
	#include "Packages/com.k3.core/ShaderLib/colors.hlsl"

	float4 maintex(float2 uv) {
        return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    }
	#define screen _ScreenParams.xy

	float _Factor;
	float _DiagonalFactor;

	float3 sampleWithKernel(float2 uv) {
		float a = _Factor;
		float d = _DiagonalFactor * _Factor;

		// there is one square with weight 1, 4 regular ones, 4 diagonal ones

		float s = 1.0 + (a + d) * 4;
		a /= s;
		d /= s;

		float kernel[9] = {
			d, a, d,
			a, 1.0/s, a,
			d, a, d
		};

		float3 sum = (0);
		float2 texelSize = 1.0 / screen.xy;

		for (int y = 0; y <= 2; y++)
		{
			for (int x = 0; x <= 2; x++)
			{
				float kernelValue = kernel[3*y+x];
				sum += maintex(uv + float2(x-1, y-1) * texelSize).rgb * kernelValue;
			}
		}
		return sum;
	}

	float4 Frag(VaryingsDefault i) : SV_Target
	{
		// float depth = tex2D(_CameraDepthTexture, i.uv).r;
		// return float4(r, 0, 0, 1);
		// //linear depth between camera and far clipping plane
		// depth = Linear01Depth(depth);
		float4 color = float4(sampleWithKernel(i.texcoord), 1.0);
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
