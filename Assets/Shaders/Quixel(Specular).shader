Shader "Custom/Quixel(Specular)"
{
    Properties
    {           
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Normal ("Normal", 2D) = "black" {}
        _Specular("Specular", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5 
        _Alpha("Alpha", Range(0,1)) = 1
    }
    SubShader
    {
        //Tags{"Queue" = "Transparent"  "IgnoreProjection" = "True" "RenderType" = "Transparent" } // Í¸Ã÷µÄShader
        Tags{"Queue" = "Geometry"  "IgnoreProjection" = "True"  "RenderType" = "Opaque" }
        
        LOD 200    
        //ZWrite Off
        ZTest LEqual
        
        //Blend SrcAlpha OneMinusSrcAlpha
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf StandardSpecular fullforwardshadows //alpha

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Normal;
        sampler2D _Specular;
        sampler2D _MetallicTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Alpha;
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
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_MainTex));
            fixed4 specular = tex2D(_Specular, IN.uv_MainTex);
            o.Specular = (specular.r + specular.g + specular.b) / 3;
            o.Smoothness = _Glossiness;
            o.Alpha = _Alpha;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
