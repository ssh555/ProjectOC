Shader "Universal Render Pipeline/Shader_Common_HairColorSplit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColor ("Color1",Color) = (1,0.2,0.2,1)
        _Color2 ("Color2",Color) = (1,0.5,0.2,1)
        _ColorType("ColorType",int) = 0
        _smoothStepThreshold("StepThreshold",Range(0,0.8)) = 0.3
        _smoothStrength("SmoothStrength",Range(0,0.5)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalRenderPipeline"}
        LOD 100

        Pass
        {
            HLSLPROGRAM  //URP 程序块开始
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			CBUFFER_START(UnityPerMaterial) //变量引入开始
			float _smoothStepThreshold,_smoothStrength;
            float4 _MainColor,_Color2;
            int _ColorType;
            CBUFFER_END //变量引入结束
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            
            struct VertexInput
            {
                float4 vertex : POSITION;
                float4 normalOS  : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 normalWS : NORMAL;
            };

            
            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
				VertexPositionInputs positionInputs = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = positionInputs.positionCS;
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS.xyz);
				o.normalWS = normalInputs.normalWS;
                
                o.uv = v.uv;
                return o;
            }

            float4 frag (VertexOutput IN) : SV_Target
            {
                Light mainLight = GetMainLight(); 
                float4 mainLightColor = float4(mainLight.color, 1); //获取主光源颜色
                float3 worldLightDir = normalize(mainLight.direction); //主光源方向
                float ndotLRaw = dot(IN.normalWS, worldLightDir);
                float halfLambert = ndotLRaw * 0.5f + 0.5f;
                
                float4 color = float4(1,1,1,1);
                // <0.5 主色    >0.5 副色
                //0纯色 1分色 2动态渐变 3静态渐变 
                if(_ColorType == 0)
                {
                    color = _MainColor;
                }
                else if(_ColorType == 1)
                {
                    color = step(0.5f,IN.uv.x) ? _Color2:_MainColor;
                }
                else if(_ColorType == 2)
                {
                    float lerpValue = smoothstep(_smoothStepThreshold,_smoothStepThreshold+_smoothStrength,IN.uv.y);
                    color = lerp(_Color2,_MainColor,lerpValue);
                }
                else if(_ColorType == 3)
                {
                    float4 _texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(IN.uv));
                    color = step(0.5f,_texColor.x) ? _Color2:_MainColor;
                }
                
                return color*halfLambert*mainLightColor;
            }
            ENDHLSL
        }
    }
}
