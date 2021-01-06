Shader "Custom/DisplacementOpaque"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Normal ("Normal", 2D) = "black" {}
        _Metallic("Metallic", 2D) = "black" {}
        _DispTex("Displacment", 2D) = "gray" {}
        _Glossiness ("Smoothness", 2D) = "black"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:disp tessellate:tessFixed

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _DispTex;
        
        struct appdata {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;     
            float2 texcoord : TEXCOORD0;
            float2 texcoord1 : TEXCOORD1;
            float2 texcoord2 : TEXCOORD2;
        };           

        float4 tessFixed()
        {
            return 128;
        }

        void disp(inout appdata v)
        {
            float d = tex2Dlod(_DispTex, float4(v.texcoord.xy, 0, 0)).r * 0.04;
       
            v.vertex.xyz += v.normal * d;
        }


        sampler2D _MainTex;
        sampler2D _Normal;
        sampler2D _Metallic;
        sampler2D _Glossiness;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Normal;
            float2 uv_Metallic;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) *_Color;
            o.Albedo = c.rgb;
            o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_Normal));
            o.Metallic = tex2D(_Metallic, IN.uv_Metallic).r;            
            o.Smoothness = 0;// 1.0 - tex2D(_Glossiness, IN.uv_Metallic).r;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
