Shader "Custom/Shader_Survivor"
{
    Properties
    {           
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Normal ("Normal", 2D) = "black" {}
        _Specular("Specular", 2D) = "black" {}
        _PantsMask("PantsMask", 2D) = "black" {}
        _PantColorDiff("PantColorDiff", Range(0,1)) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Cutoff("Base Alpha cutoff", Range(0, 0.9)) = 0.5
    }
    SubShader
    {
        Tags {"Queue" = "AlphaTest" "IgnoreProjector"="True" "RenderType" = "TransparentCutout" }
        LOD 200
        Cull Back

        ZTest LEqual
        //Offset -200, 0

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf StandardSpecular alphatest:_Cutoff fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Normal;
        sampler2D _Specular;
        sampler2D _PantsMask;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Normal;
            float2 uv_Specular;
        };

        half _Glossiness;
        half _Metallic;
        half _PantColorDiff;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float3 RGB2HSV(float3 c)
        {
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
            float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

            float d = q.x - min(q.w, q.y);
            float e = 1.0e-10;
            return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
        }
        float3 HSV2RGB(float3 c)
        {
            float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
            float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
            return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
        }

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            // Albedo comes from a texture tinted by color
            float4 c = tex2D(_MainTex, IN.uv_MainTex);
            float3 albedo_rgb = c.rgb;

         /*   float mask = tex2D(_PantsMask, IN.uv_MainTex).a;            
            float3 albedo_hsv = RGB2HSV(albedo_rgb);            
            albedo_hsv.r = albedo_hsv.r - mask * _PantColorDiff;
            albedo_rgb = HSV2RGB(albedo_hsv);*/
            
            o.Albedo = albedo_rgb;
            o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));
            fixed4 specular = tex2D(_Specular, IN.uv_Specular);
            o.Specular = (specular.r + specular.g + specular.b) / 3;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
