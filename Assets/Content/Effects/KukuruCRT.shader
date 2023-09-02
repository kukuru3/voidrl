Shader "Scanner/CRT"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
    
    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

    sampler2D _LastCameraDepthTexture ;
    
    float _ChromAbbDistance;
    float _DotMatrixRepeat;
    float _Greenify;
    float _ColorCurve;

    half4 _CentralBleed;

    half3 _ShadowBaseline;

    float4 _ScanlineProps; // repeat, speed, dark, light

    float _FlickerIntensity;

    float _FinalMix;

    float _Tweak;

    float _Distortion;
    
    float2 curve(float2 uv)
    {
        float2  c = uv - 0.5;
        float dt = (dot(c, c) - 0.2) * _Distortion; 
        return uv + c * ((1.0 + dt) * dt);
        // return uv;
    }

    float4 maintex(float2 uv) {
        return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    }

    float remap(float value, float from, float to, float targetFrom, float targetTo) {
        float t = (to - value) / (to - from);
        return lerp(targetFrom, targetTo, t);
    }

    uniform float _AddedSkyboxColor;

    float4 Frag(VaryingsDefault i) : SV_Target
    {
        float2 uv = i.texcoord;

        float2 screen = _ScreenParams.xy;

        const bool useCurve = true;
        const bool useBleeding = false;

        const bool useNoiseLines = true;
        const bool useVignette = false;
        const bool useScanlines = true;
        const float useDotMatrix = true;

        float3 colorBalance = float3(1.0 - _Greenify, 1.0 + _Greenify, 1.0 - _Greenify);

        float time = _Time.y;

        float3 col = (0);

        float2 offsetR = float2(-1,0);
        float2 offsetG = float2(1,0);
        float2 offsetB = (0); //float2(-2,0);

        // // "x" is a series of horizontal noise lines that disappear and appear.
        float noisex = 0.0;
    
        if (useNoiseLines) 
	        noisex = 
                sin( 0.3 * time + uv.y * 21.0)
                *sin( 0.7 * time + uv.y * 29.0)
                *sin(0.3+0.33*time+uv.y*31.0); //*0.0017; 

        noisex = saturate((noisex - 0.5) * 2.0);
        // return float4(noisex, noisex, noisex, 1.0);

        if (useCurve) uv = curve(uv);

        // todo: "x-factor" was used here somewhere

        float2 noisemul = float2(0.0017, 0.0);

        float3 originalColor = maintex(uv + noisemul * noisex);

        col.r = maintex(uv + offsetR / screen * _ChromAbbDistance + noisemul * noisex).x;
        col.g = maintex(uv + offsetG / screen * _ChromAbbDistance + noisemul * noisex).y;
        col.b = maintex(uv + offsetB / screen * _ChromAbbDistance + noisemul * noisex).z;

        if (useBleeding) {
            col.r += 0.08 * maintex(0.75 * float2(0.025, -0.027) + uv).r;
            col.g += 0.05 * maintex(0.75 * float2(-0.022, -0.02) + uv).g;
            col.b += 0.08 * maintex(0.75 * float2(-0.02, -0.018) + uv).b;
        }
        
        half lum = dot(col, float3(0.299f, 0.587f, 0.114f));

        float adder = 1.0; 

        col += _ShadowBaseline * adder;

        col -= _AddedSkyboxColor;

        float multiplier = length(i.texcoord - 0.5);
        multiplier = 2 * (0.5 - multiplier + _CentralBleed.z);

        multiplier = saturate(multiplier);
        multiplier = pow(multiplier, _CentralBleed.y);
        col += _ShadowBaseline * (multiplier * _CentralBleed.x);

        // col *= (1.0 + multiplier * 0.1);

        col = saturate(col*(1.0 - _ColorCurve) + _ColorCurve*col*col);

        

        // vignette:
        if (useVignette) {
            float vig = 16.0*uv.x*uv.y*(1.0-uv.x)*(1.0-uv.y);
            col *= pow(vig,0.4);
        }

        if (useScanlines) {

            float scanlineRepeat = _ScanlineProps.x;
            float scanlineSpeed = _ScanlineProps.y;
            float scanlineDark = _ScanlineProps.z;
            float scanlineLight = _ScanlineProps.w;

            // scans will always be in the range of 0 - 0.7
            float scanlineFactor = 0.5 + 0.5 * sin(scanlineSpeed * time + uv.y * screen.y * 2 * PI / scanlineRepeat );

            float y = i.texcoord.y * screen.y + _Tweak;
            scanlineFactor = step(scanlineRepeat / 2, y % scanlineRepeat);

            scanlineFactor = lerp(scanlineDark, scanlineLight, scanlineFactor);

            // col *= scanlineFactor;

            float luma = dot(col, float3(0.299f, 0.587f, 0.114f));
            float brightness = smoothstep(0.7, 1.4, luma);
            float multiplier = scanlineFactor;
            // float multiplier = lerp(scanlineFactor, 1 - 0.5 * scanlineFactor, brightness);
            col *= multiplier;
        }

        //col *= 1.4;

        col *= colorBalance;

        // add a very slight flicker
        col *= 1.0 + _FlickerIntensity*sin(110.0*time) ;

        if (uv.x < 0.0 || uv.x > 1.0) col *= 0.0;
	    if (uv.y < 0.0 || uv.y > 1.0) col *= 0.0;

        if (useDotMatrix) {

            float m = i.texcoord.x * screen.x;

            m = sin(m * 2 * PI / _DotMatrixRepeat);

            // m = saturate(  (fmod(m, 12.0)-6.0) / 6.0 * 0.4 );
            col.rgb *= 1 + 0.12 * m;
            // col.rgb = m;

            // "horizontal" component of the scanlining aka dotmatrixing
            // col*=1.0-0.65*(clamp((fmod(uv.x * screen.x, 2.0)-1.0)*2.0,0.0,1.0));
        }


        // float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
        
        col = lerp(originalColor, col, _FinalMix);
        return float4(col, 1.0);
        

        return float4(0,1,0,1);
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
