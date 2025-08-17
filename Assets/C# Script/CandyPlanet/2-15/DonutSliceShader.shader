Shader "Unlit/DonutSliceShader"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Rotation ("Rotation", Float) = 0
        _SliceCount ("Slice Count", Float) = 12
        _EatenFlags ("Eaten Flags", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Rotation;
            float _SliceCount;
            float4 _EatenFlags;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5; // 중심 기준
                float angle = atan2(uv.y, uv.x); // 라디안
                angle = degrees(angle);
                if (angle < 0) angle += 360;

                // 회전 보정
                float fixedAngle = fmod(angle - _Rotation + 360, 360);

                // 현재 조각 인덱스
                float sliceAngle = 360.0 / _SliceCount;
                int sliceIndex = int(fixedAngle / sliceAngle);

                // 4비트 마스크로 먹힌 조각 확인
                int mask = 1 << sliceIndex;
                int flags = int(_EatenFlags.x); // 최대 32조각까지 가능

                if ((flags & mask) != 0)
                {
                    // 먹힌 조각이면 알파 제거
                    return float4(0,0,0,0);
                }

                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
