Shader "Custom/SeeThroughHighlight"
{
    Properties
    {
        _Color ("Highlight Color", Color) = (1, 1, 0, 1) // Yellow by default
    }
    SubShader
    {
        // "Queue"="Overlay" tells Unity to draw this very last
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        LOD 100

        // This is the magic line that gives it X-Ray vision through walls
        ZTest Always
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Just draw the solid color
                return _Color;
            }
            ENDCG
        }
    }
}