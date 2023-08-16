Shader "Void/Space Sharp"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Cutoff("Cutoff", Range(-1.0, 1.0)) = 0.5
        // _Normals("Normals", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Custom fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Normals;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        fixed4 _Color;
        half _Cutoff;

        // struct SurfaceOutputCustom
        // {
        //     fixed3 Albedo;      // base (diffuse or specular) color
        //     float3 Normal;      // tangent space normal, if written
        //     half3 Emission;
        //     half Metallic;      // 0=non-metal, 1=metal
        //     // Smoothness is the user facing name, it should be perceptual smoothness but user should not have to deal with it.
        //     // Everywhere in the code you meet smoothness it is perceptual smoothness
        //     half Smoothness;    // 0=rough, 1=smooth
        //     half Occlusion;     // occlusion (default 1)
        //     fixed Alpha;        // alpha for transparencies
        // };

        half4 LightingCustom (SurfaceOutput s, half3 lightDir, half atten) {
            half NdotL = dot (s.Normal, lightDir);

            float3 cameraForward = mul((float3x3)unity_CameraToWorld, float3(0,0,1));

            half NdotC = -dot(s.Normal, cameraForward);
            half camera_ramp = smoothstep(0.1, 0.4, NdotC);

            // half diff = NdotL * 0.5 + 0.5;
            // half3 ramp = tex2D (_Ramp, float2(diff)).rgb;
            half4 c;
            half lighting_ramp = smoothstep(_Cutoff, _Cutoff + 0.01,NdotL);
    
            half shadow_ramp = smoothstep(0.5, 0.51, atten);

            c.rgb = s.Albedo * _LightColor0.rgb * camera_ramp * shadow_ramp * lighting_ramp; //s.Albedo * _LightColor0.rgb * atten;
            c.a = s.Alpha;
            
            
            return c;
        }

        // half4 LightingCustom (SurfaceOutput s, half3 viewDir, inout UnityGI gi) {
        //     // float3 lightDir = gi.light.dir;
        //     // half NdotL = dot (s.Normal, lightDir);
        //     // half diff = NdotL * 0.5 + 0.5;
        //     // // half3 ramp = tex2D (_Ramp, float2(diff)).rgb;
        //     // half4 c;
        //     // // c.rgb = s.Albedo * _LightColor0.rgb * ramp * atten;
        //     // c.rgb = s.Albedo * atten * _LightColor0.rgb; //s.Albedo * _LightColor0.rgb * atten;
        //     // c.a = s.Alpha;
        //     return (0,0,0,0.6);
        //     //return c;
        // }

        // void LightingCustom_GI (SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
           
        // }




        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // o.Normal = UnpackNormal(tex2D(_Normals, IN.uv_MainTex));
            // o.Metallic = 0;
            // o.Smoothness = 0;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
