Shader "Sprites/OutlineDual"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _Thickness ("Outline Thickness", Range(0.0, 10.0)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _Thickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                float alpha = c.a;

                // Sample alpha in multiple directions to detect edges
                float2 offset = _MainTex_TexelSize.xy * _Thickness;
                float a1 = tex2D(_MainTex, i.uv + float2(offset.x, 0)).a;
                float a2 = tex2D(_MainTex, i.uv + float2(-offset.x, 0)).a;
                float a3 = tex2D(_MainTex, i.uv + float2(0, offset.y)).a;
                float a4 = tex2D(_MainTex, i.uv + float2(0, -offset.y)).a;

                float edge = step(0.1, (a1 + a2 + a3 + a4) * 0.25) - step(0.9, alpha);

                // Mix inside + outside outline
                float outlineMask = saturate(edge + (alpha * 0.5));

                fixed4 outline = _OutlineColor;
                outline.a *= outlineMask;

                // Blend base + outline
                fixed4 finalColor = lerp(outline, c, alpha);
                return finalColor;
            }
            ENDCG
        }
    }
}
