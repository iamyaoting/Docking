Shader "Custom/SpecularTwoSidedCutout"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Normal ("Normal", 2D) = "black" {}
        _Specular("Specular", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Cutoff("Base Alpha cutoff", Range(0, 0.9)) = 0.5
    }
    SubShader
    {
        Tags {"Queue" = "AlphaTest" "IgnoreProjector"="True" "RenderType" = "TransparentCutout" }
        LOD 200
        Cull off

        ZTest Greater
        //ZWrite Off//重要，关闭ZWrite后，后渲染的会覆盖此Pass
        CGPROGRAM
        #pragma surface surf StandardSpecular alphatest:_Cutoff fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Normal;
            float2 uv_Specular;
        };       
        UNITY_INSTANCING_BUFFER_START(Props)       
        UNITY_INSTANCING_BUFFER_END(Props)
        void surf(Input IN, inout SurfaceOutputStandardSpecular o)
        {              
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);            
            o.Alpha = c.a;
        }
        ENDCG

        ZTest LEqual       
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf StandardSpecular alphatest:_Cutoff fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Normal;
        sampler2D _Specular;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Normal;
            float2 uv_Specular;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) *_Color;
            o.Albedo = c.rgb;
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
