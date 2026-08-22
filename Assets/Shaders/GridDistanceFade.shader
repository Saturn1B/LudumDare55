Shader "Custom/InfiniteGrid"
{
    Properties
    {
        _CellSize("Cell Size", Float) = 1.0
        _ThinLineColor("Thin Line Color", Color) = (0.6, 0.6, 0.6, 0.3)
        _BoldLineColor("Bold Line Color", Color) = (0.3, 0.3, 0.3, 0.5)
        _LineWidth("Line Width", Float) = 0.02
        _BoldLineWidth("Bold Line Width", Float) = 0.04
        _BoldLineEvery("Bold Line Every", Int) = 5
        _BoldLineOffset("Bold Line Offset", Float) = 2.0
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            float _CellSize;
            fixed4 _ThinLineColor;
            fixed4 _BoldLineColor;
            float _LineWidth;
            float _BoldLineWidth;
            int _BoldLineEvery;
            float _BoldLineOffset;
            float _FadeStart;
            float _FadeEnd;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            // Fonction pour calculer l'intensité de la ligne
            float gridLine(float coord, float lineWidth)
            {
                float grid = abs(frac(coord) - 0.5) / fwidth(coord);
                return 1.0 - min(grid / lineWidth, 1.0);
            }

            // Fonction pour déterminer si c'est une ligne en gras
            bool isBoldLine(float coord)
            {
                // Appliquer l'offset avant de calculer la position de la ligne en gras
                float offsetCoord = coord + (_BoldLineOffset / _CellSize);
                return fmod(abs(floor(offsetCoord)), _BoldLineEvery) < 0.5;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Utiliser les coordonnées du monde pour que la grille soit fixe dans l'espace
                float2 gridCoords = i.worldPos.xz / _CellSize;

                // Calculer les lignes X et Z
                float lineX = gridLine(gridCoords.x, _LineWidth);
                float lineZ = gridLine(gridCoords.y, _LineWidth);

                // Calculer les lignes en gras
                float boldLineX = 0.0;
                float boldLineZ = 0.0;

                if (isBoldLine(gridCoords.x))
                {
                    boldLineX = gridLine(gridCoords.x, _BoldLineWidth);
                }

                if (isBoldLine(gridCoords.y))
                {
                    boldLineZ = gridLine(gridCoords.y, _BoldLineWidth);
                }

                // Combiner les lignes
                float totalLine = max(lineX, lineZ);
                float totalBoldLine = max(boldLineX, boldLineZ);

                // Choisir la couleur appropriée
                fixed4 finalColor = lerp(_ThinLineColor, _BoldLineColor, totalBoldLine);
                finalColor.a *= max(totalLine, totalBoldLine);

                // Calculer le fade basé sur la distance
                float3 camPos = _WorldSpaceCameraPos;
                float dist = distance(i.worldPos, camPos);
                float fade = saturate((_FadeEnd - dist) / (_FadeEnd - _FadeStart));
                finalColor.a *= fade;

                // Éviter les lignes trop fines qui causent du flickering
                if (finalColor.a < 0.01)
                    discard;

                return finalColor;
            }
            ENDCG
        }
    }
}