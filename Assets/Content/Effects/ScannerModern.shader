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

	float4 maintex(float2 uv) {
        return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    }

    float remap(float value, float from, float to, float targetFrom, float targetTo) {
        float t = (to - value) / (to - from);
        return lerp(targetFrom, targetTo, t);
    }

	float remapClamped(float value, float from, float to, float targetFrom, float targetTo) {
        float t = (to - value) / (to - from);
        return lerp(targetFrom, targetTo, saturate(t));
    }

	float4 _ColorA;
	float4 _ColorB;
	float4 _Remap;

	float _Aberration;

	float4 _CorrectionRamp;

	float _Scanline;

	uniform float _HexAnimProgress;
	uniform float _HexAnimSeed;

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

	float2 hexround(float2 pixel) {
		
		float q = round(pixel.x);
		float r = round(pixel.y);
		float initS = - pixel.x - pixel.y;

		float s = round(initS);

		float qdiff = abs(q - pixel.x);
		float rdiff = abs(r - pixel.y);
		float sdiff = abs(s - initS);

		if (qdiff > rdiff && qdiff > sdiff) {
			q = -r-s;
		} else if (rdiff > sdiff) {
			r = -q-s;
		}

		return float2(q, r);
	}

	// function flat_hex_to_pixel(hex):
	// 	var x = size * (     3./2 * hex.q                    )
	// 	var y = size * (sqrt(3)/2 * hex.q  +  sqrt(3) * hex.r)
	// 	return Point(x, y)

	// function pixel_to_flat_hex(point):
    // var q = ( 2./3 * point.x                        ) / size
    // var r = (-1./3 * point.x  +  sqrt(3)/3 * point.y) / size
    // return axial_round(Hex(q, r))

	float2 pixel_to_hex(float2 pt, float size) {
		return float2( 
			( 2.0/3 * pt.x                     ) / size,
			(-1.0/3 * pt.x  +  sqrt(3)/3 * pt.y) / size
		);
	}

	float2 hex_to_pixel(float2 hex, float size) {
		float x = size * (     3.0 / 2 * hex.x                    );
		float y = size * (sqrt(3)  / 2 * hex.x  +  sqrt(3) * hex.y);
		return float2(x, y);
	}

	float random2(float2 uv)
	{
		return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
	}

	float4 Frag(VaryingsDefault i) : SV_Target
	{

		float hexSize = 8.0;

		float2 uv = i.texcoord;
		float2 screen = _ScreenParams.xy;

		uv = pixel_to_hex(uv * screen, hexSize);
		uv = hex_to_pixel(uv, hexSize) / screen;

		float2 hex = hexround(  pixel_to_hex(uv * screen, hexSize) );

		float4 color = (0); 

		if (_Aberration > 0) {
			float2 offsetR = float2(-1,0);
			float2 offsetG = float2(1,0);
			float2 offsetB = (0); //float2(-2,0);
			
			float r = correct(maintex(uv + offsetR / screen * _Aberration)).r;
			float g = correct(maintex(uv + offsetG / screen * _Aberration)).g;
			float b = correct(maintex(uv + offsetB / screen * _Aberration)).b;

			color.rgb = float3(r,g,b);
		} else {
			float3 original = maintex(uv).rgb;
			color.rgb = correct(original);
		}

		// color.gb *= finalMul;

		float mult = step((uv * screen).y % 2, 1.0) * (1.0 + pow(_Scanline, 0.33)); // dunno why the 0.2 works, it just does. 
		color.rgb *= lerp(1.0, mult, _Scanline);
		float randid = -1.0 + random2(hex + _HexAnimSeed) + _Time.y * 1.5 ;

		if (randid  < 0) {
			float2 centerOfHex = hex_to_pixel(hex, hexSize) / screen;			
			float3 centerHexColor = correct(maintex(centerOfHex).rgb);
			// color.rgb = lerp(color.rgb, centerHexColor, -randid);
			color.rgb = centerHexColor;
		} 
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
