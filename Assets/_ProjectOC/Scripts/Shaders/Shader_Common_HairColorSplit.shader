Shader "Universal Render Pipeline/Shader_Common_HairColorSplit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Color1",Color) = (1,0.2,0.2,1)
        _Color2 ("Color2",Color) = (1,0.5,0.2,1)
        _ColorType("ColorType",int) = 0
        _smoothStepThreshold("StepThreshold",Range(0,0.8)) = 0.3
        _smoothStrength("SmoothStrength",Range(0,0.5)) = 0.1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"  }
        LOD 100

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
                //"LightMode" = "UniversalGBuffer"
            }
            HLSLPROGRAM  //URP ����鿪ʼ
			#pragma target 4.5
			#pragma vertex vert
            #pragma fragment frag

			//URP������
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			#pragma multi_compile  _MAIN_LIGHT_SHADOWS
			#pragma multi_compile  _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile  _SHADOWS_SOFT


            CBUFFER_START(UnityPerMaterial) //�������뿪ʼ
			float _smoothStepThreshold,_smoothStrength;
            float4 _BaseColor,_Color2;
            int _ColorType;
            CBUFFER_END //�����������
            
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
                float3 positionWS :  TEXCOORD1;
				float3 normalWS : NORMAL;
            };

            
            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
				VertexPositionInputs positionInputs = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = positionInputs.positionCS;
                o.positionWS = positionInputs.positionWS;
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS.xyz);
				o.normalWS = normalInputs.normalWS;
                
                o.uv = v.uv;
                return o;
            }

            float4 frag (VertexOutput IN) : SV_Target
            {
                Light mainLight = GetMainLight(); 
                float4 mainLightColor = float4(mainLight.color, 1); //��ȡ����Դ��ɫ
                float3 worldLightDir = normalize(mainLight.direction); //����Դ����
                float ndotLRaw = dot(IN.normalWS, worldLightDir);
                float halfLambert = ndotLRaw * 0.5f + 0.5f;
                float4 SHADOW_COORDS = TransformWorldToShadowCoord(IN.positionWS);
                Light  lightData = GetMainLight(SHADOW_COORDS);
                float4 color = float4(1,1,1,1);
                half shadow = lightData.shadowAttenuation;
                // <0.5 ��ɫ    >0.5 ��ɫ
                //0��ɫ 1��ɫ 2��̬���� 3��̬���� 
                if(_ColorType == 0)
                {
                    color = _BaseColor;
                }
                else if(_ColorType == 1)
                {
                    color = step(0.5f,IN.uv.x) ? _Color2:_BaseColor;
                }
                else if(_ColorType == 2)
                {
                    float lerpValue = smoothstep(_smoothStepThreshold,_smoothStepThreshold+_smoothStrength,IN.uv.y);
                    color = lerp(_Color2,_BaseColor,lerpValue);
                }
                else if(_ColorType == 3)
                {
                    float4 _texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(IN.uv));
                    color = step(0.5f,_texColor.x) ? _Color2:_BaseColor;
                }
                
                return color*halfLambert*mainLightColor;
            }
          ENDHLSL  //URP ��������
        }
		//��ǰģ�ʹ�����Ӱ����
	 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}