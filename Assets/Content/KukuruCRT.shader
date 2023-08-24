Shader "Scanner/K3 CRT1 Variant 1"
{
    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);


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

    float4 Frag(VaryingsDefault i) : SV_Target
    {
        float2 uv = i.texcoord;

        float2 screen = _ScreenParams.xy;

        const bool useCurve = true;

        float time = _Time.y;

        float3 col;

        float2 offsetRed   = float2(11, 1);
        float2 offsetGreen = float2(0,-2);
        float2 offsetBlue  = float2(-2,0);
        
        // "x" is a series of horizontal noise lines that disappear and appear.
        float x = 0.0;
    
        // if (useNoiseLines) 
	    //     x = sin(0.3*iTime+uv.y*21.0)*sin(0.7*iTime+uv.y*29.0)*sin(0.3+0.33*iTime+uv.y*31.0)*0.0017; 

        if (useCurve) uv = curve(uv);

        // col.r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offsetRed / screen).r;
        // col.g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offsetGreen / screen).g;
        // col.b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offsetBlue / screen).b;


        // float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
        //return float4(col, 1.0);
        

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
