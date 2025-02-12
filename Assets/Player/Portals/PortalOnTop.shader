Shader "Unlit/Portal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Opaque" }
        ZWrite Off
        ZTest Always
        Blend Off
        Cull Off
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };


            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;
                // Clipping plane position from vertex
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Convert to screen position;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Divide the x and y value by w, due to matrix multiplication
                float2 uv = i.screenPos.xy / i.screenPos.w;
                // sample and return
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }

    Fallback "Standard"
}
