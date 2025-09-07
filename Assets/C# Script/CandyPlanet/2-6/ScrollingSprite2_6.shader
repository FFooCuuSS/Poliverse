Shader "Custom/ScrollingSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ScrollX ("Scroll X", Float) = 0.0
        _ScrollY ("Scroll Y", Float) = 0.0
    }
    SubShader
    {
        Tags {"RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScrollX;
            float _ScrollY;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

    
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);

    
                uv += float2(_ScrollX, _ScrollY);

   
                uv = frac(uv);

                o.uv = uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
