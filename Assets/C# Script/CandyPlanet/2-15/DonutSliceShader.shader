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
                float2 uv = i.uv - 0.5; // �߽� ����
                float angle = atan2(uv.y, uv.x); // ����
                angle = degrees(angle);
                if (angle < 0) angle += 360;

                // ȸ�� ����
                float fixedAngle = fmod(angle - _Rotation + 360, 360);

                // ���� ���� �ε���
                float sliceAngle = 360.0 / _SliceCount;
                int sliceIndex = (int)(fixedAngle / sliceAngle);

                // ��Ʈ����ũ �� float ��� üũ
                float mask = pow(2.0, sliceIndex);   // 2^sliceIndex
                float flags = _EatenFlags.x;         // float�� ���� ��Ʈ��

                // ���� �����̸� ���� ����
                if (fmod(flags / mask, 2.0) >= 1.0)
                {
                    return float4(0,0,0,0);
                }

                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
