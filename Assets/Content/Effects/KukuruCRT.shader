Shader "Scanner/CRT"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

    float _ChromAbbDistance;

    float _ScanlineRepeat;

    float _DotMatrixRepeat;

    float _Greenify;
    
    float2 curve(float2 uv)
    {
        uv = (uv - 0.5) * 2.0;
        uv *= 1.1;	
        
        uv.x *= 1.0 + pow((abs(uv.y) / 5.0), 2.0);
        uv.y *= 1.0 + pow((abs(uv.x) / 4.0), 2.0);

        uv  = (uv / 2.0) + 0.5;
        uv =  uv *0.92 + 0.04;
        return uv;
    }
    float4 maintex(float2 uv) {
        return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
    }

    float4 Frag(VaryingsDefault i) : SV_Target
    {
        float2 uv = i.texcoord;

        float2 screen = _ScreenParams.xy;

        const bool useCurve = false;
        const bool useNoiseLines = true;
        const bool useBleeding = false;
        const bool useVignette = true;

        const bool useScanlines = true;
        const float useDotMatrix = true;

        // const float greenify = 0.15;

        float3 colorBalance = float3(1.0 - _Greenify, 1.0 + _Greenify, 1.0 - _Greenify);

        const float colorCurve = 0.4;

        float time = _Time.y;

        float3 col = (0);

        float2 offsetR = float2(-1,0);
        float2 offsetG = float2(1,0);
        float2 offsetB = (0); //float2(-2,0);

        

        // // "x" is a series of horizontal noise lines that disappear and appear.
        float x = 0.0;
    
        if (useNoiseLines) 
	        x = sin(0.3*time+uv.y*21.0)*sin(0.7*time+uv.y*29.0)*sin(0.3+0.33*time+uv.y*31.0)*0.0017; 

        if (useCurve) uv = curve(uv);

        // todo: "x-factor" was used here somewhere
        col.r = maintex(uv + offsetR / screen * _ChromAbbDistance).x;
        col.g = maintex(uv + offsetG / screen * _ChromAbbDistance).y;
        col.b = maintex(uv + offsetB / screen * _ChromAbbDistance).z;

        if (useBleeding) {
            col.r += 0.08 * maintex(0.75 * float2(0.025, -0.027) + uv).r;
            col.g += 0.05 * maintex(0.75 * float2(-0.022, -0.02) + uv).g;
            col.b += 0.08 * maintex(0.75 * float2(-0.02, -0.018) + uv).b;
        }

        // subtle color curve:
        col = clamp(col*(1.0 - colorCurve) + colorCurve*col*col ,0.0 ,1.0);

        // vignette:
        if (useVignette) {
            float vig = (0.0 + 1.0*16.0*uv.x*uv.y*(1.0-uv.x)*(1.0-uv.y));
            col *= pow(vig,0.3);
        }

        if (useScanlines) {
            col *= 2.8; // to compensate for the sinewave, probably
            float scans = clamp( 0.35+0.35*sin(2*time + uv.y*screen.y * 2 * PI / _ScanlineRepeat ), 0.0, 1.0);	

            // scans will always be in the range of 0 - 0.7

            float s = pow(scans,1.7); // and this will remap them even further to the range of 0-0.54
            col = col * (0.4 + 0.7*s); // color is multiplied by 0.4 where there are no scanlines, and by 0.78 where the scanlines are full 
        }

        col *= colorBalance;

        // add a very slight flicker
        col *= 1.0+0.02*sin(110.0*time);

        if (uv.x < 0.0 || uv.x > 1.0) col *= 0.0;
	    if (uv.y < 0.0 || uv.y > 1.0) col *= 0.0;

        if (useDotMatrix) {

            float m = i.texcoord.x * screen.x;

            m = saturate(sin(m * 2 * PI / _DotMatrixRepeat));

            // m = saturate(  (fmod(m, 12.0)-6.0) / 6.0 * 0.4 );
            col.rgb *= 1.3 - 0.65 * m;
            // col.rgb = m;

            // "horizontal" component of the scanlining aka dotmatrixing
            // col*=1.0-0.65*(clamp((fmod(uv.x * screen.x, 2.0)-1.0)*2.0,0.0,1.0));
        }


        // float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
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
