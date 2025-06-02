Shader "Unlit/GridDistanceFade"
{
    Properties
    {
        _Color("Base Color", Color) = (1,1,1,1)
        _FadeStart("Fade Start", Float) = 10
        _FadeEnd("Fade End", Float) = 50
    }

        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 color : COLOR;
            };

            fixed4 _Color;
            float _FadeStart;
            float _FadeEnd;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 camPos = _WorldSpaceCameraPos;
                float dist = distance(i.worldPos, camPos);

                float fade = saturate((_FadeEnd - dist) / (_FadeEnd - _FadeStart));
                i.color.a *= fade;

                return i.color;
            }
            ENDCG
        }
    }
}
