
Shader "Custom/Color Selection" {
  Properties {
      _MainTex ("Albedo", 2D) = "white" {}
      _Color ("Color", Color) = (1,1,1,1)
      _Met ("Metallic", 2D) = "white" {}
      _NormalMap ("Normal Map", 2D) = "bump" {}
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Standard fullforwardshadows
      struct Input {
        float2 uv_MainTex;
        float2 uv_NormalMap;
        float2 uv_Met;
      };
      sampler2D _MainTex;
      sampler2D _NormalMap;
      sampler2D _Met;
      float4 _Color;
      void surf (Input IN, inout SurfaceOutputStandard o) {
      	float4 Alb = tex2D (_MainTex, IN.uv_MainTex);
      	o.Albedo = Alb.rgb*Alb.a+_Color*(1-Alb.a);
        o.Metallic = tex2D (_Met, IN.uv_Met).r;
        o.Smoothness = tex2D (_Met, IN.uv_Met).a;
        o.Normal = UnpackNormal (tex2D (_NormalMap, IN.uv_NormalMap));
      }
      ENDCG
    } 
    Fallback "VertexLit"
    }