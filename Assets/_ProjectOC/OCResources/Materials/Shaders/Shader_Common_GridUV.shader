Shader "Universal Render Pipeline/GridUVScale"
{
     //�������
    Properties
    {
        _UVScale("UVScale",float) = 1
	    //������ɫ
		_MainColor("MainColor", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
    }
        SubShader
        {
        	Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"  }
			//����뼶��
            LOD 100 
            //Cull [_CullMode]
		 Pass
        {
        	Tags
            {
                "LightMode" = "UniversalForward"
                //"LightMode" = "UniversalGBuffer"
            }
            HLSLPROGRAM  //URP ����鿪ʼ
			#pragma target 4.5
			//�������Ƭ�� vert
			#pragma vertex vert
			//�������Ƭ�� frag
            #pragma fragment frag
            

			//URP������
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			CBUFFER_START(UnityPerMaterial) //�������뿪ʼ
			float _UVScale;
            float4 _MainColor; 
            CBUFFER_END //�����������

			//��ȡ�������,//������ͼ������
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);



			//����ģ��ԭʼ���ݽṹ
            struct VertexInput          
            {
                float4 positionOS : POSITION; 
                float4 normalOS  : NORMAL;
            };


			//���嶥�����Ƭ�����i�����Ƭ�εĴ������ݽṹ
            struct VertexOutput 
            {
                float4 position : SV_POSITION; 
				float3 positionWS :  TEXCOORD1;
				float3 normalWS : NORMAL;
            };

			float3 triplanar(float3 worldPos, float scale, float3 blendAxes) {
				float3 scaledWorldPos = worldPos / scale;
				float3 xProjection = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(scaledWorldPos.y, scaledWorldPos.z)) * blendAxes.x).xyz;
				float3 yProjection = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(scaledWorldPos.x, scaledWorldPos.z)) * blendAxes.y).xyz;
				float3 zProjection = (SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(scaledWorldPos.x, scaledWorldPos.y)) * blendAxes.z).xyz;
				return xProjection + yProjection + zProjection;
			}

            VertexOutput vert(VertexInput v)
            {

                VertexOutput o;

                //��������ռ䶥������
				VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.position = positionInputs.positionCS;
				o.positionWS = positionInputs.positionWS;
				VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS.xyz);
				o.normalWS = normalInputs.normalWS;
                return o;
            }

			//�������Ƭ��

            float4 frag(VertexOutput IN): SV_Target 
            {	                
                //����Դ
                Light mainLight = GetMainLight(); 
                float4 mainLightColor = float4(mainLight.color, 1); //��ȡ����Դ��ɫ
                float3 worldLightDir = normalize(mainLight.direction); //����Դ����
                float3 blendAxes = abs(IN.normalWS);
				blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
                float3 textureColor = triplanar(IN.positionWS,_UVScale,blendAxes);

                //������
                float ndotLRaw = dot(IN.normalWS, worldLightDir);
                float halfLambert = ndotLRaw * 0.5f + 0.5f;

                float3 diffuse = textureColor * mainLightColor.xyz* halfLambert * _MainColor.xyz;
                float3 result = diffuse;
                
                return  float4(result,1.0f);
            }

            ENDHLSL  //URP ��������            
        }
UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
