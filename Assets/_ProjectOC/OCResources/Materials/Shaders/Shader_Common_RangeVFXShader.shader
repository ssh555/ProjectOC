Shader "Unlit/RangeVFXShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VFXColor("VFXColor",Color) = (1,1,1,1)
        _FlowSpeed("FlowSpeed",float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        
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
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            fixed4 _VFXColor;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _FlowSpeed;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                v.uv.x += _Time.x * _FlowSpeed;
                o.uv = TRANSFORM_TEX(v.uv , _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 res = tex2D(_MainTex, i.uv) * _VFXColor;
                clip (res.a - 0.5f);
                return res;
            }
            ENDCG
        }
    }
}
