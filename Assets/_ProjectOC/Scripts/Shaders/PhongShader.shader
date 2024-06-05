Shader "Universal Render Pipeline/CRLuo/CRLuo_URP_05_Specular" //URP·����
{
     //�������
    Properties
    {
	    //������ɫ
		_Color("������ɫ", Color) = (0.5,0.5,0.5,1)
		//������ͼ
        _MainTex ("����ͼ", 2D) = "white" {}

	    //������ͼ
		_NormalTex("������ͼ", 2D) = "bump" {}
		//����ǿ��
       _NormalScale("����ǿ��", Float) = 1.0

		_SpecularColor("�߹���ɫ", Color) = (1,1,1,1)
		_SpecularPower("�߹�ǿ��", Range( 1, 10)) = 1
		_SpecularRange("�߹ⷶΧ", Range( 1, 100)) = 40
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

            //��ȡ���������ɫ
            float4 _Color; 
            float _NormalScale;
            float4	   _SpecularColor;
			float  _SpecularPower;
			float	_SpecularRange;
            CBUFFER_END //�����������
			

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_NormalTex);
            SAMPLER(sampler_NormalTex);




			//����ģ��ԭʼ���ݽṹ
            struct VertexInput          
            {
				//����ռ䶥������
                float4 positionOS : POSITION; 

				//ģ��UV����
                float2 uv : TEXCOORD0;

				//ģ�ͷ���
				float4 normalOS  : NORMAL;

				//����ռ�����
				float4 tangentOS  : TANGENT;
            };


			//���嶥�����Ƭ�����i�����Ƭ�εĴ������ݽṹ
            struct VertexOutput 
            {
			   //������пռ�����
                float4 position : SV_POSITION; 
				
				//UV����
                float2 uv : TEXCOORD0;
				//����ռ䶥��
				float3 positionWS :  TEXCOORD1;
				//����ռ䷨��
				float3 normalWS : TEXCOORD2;
				//����ռ�����
				float3 tangentWS : TEXCOORD3;
				//����ռ丱����
				float3 bitangentWS : TEXCOORD4;
            };

			
				//�������Ƭ��
                VertexOutput vert(VertexInput v)
                {
				   //�����������o
                    VertexOutput o;

					//��������ռ䶥������
					VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
					//��ȡ���пռ䶥��
                    o.position = positionInputs.positionCS;
					//��ȡ����ռ䶥��
					o.positionWS = positionInputs.positionWS;

					//��������ռ䷨������
					VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS.xyz,v.tangentOS);

					//��ȡ����ռ䷨��
					o.normalWS = normalInputs.normalWS;
					//��ȡ����ռ䶥��
					o.tangentWS = normalInputs.tangentWS;
					//��ȡ����ռ䶥��
					o.bitangentWS = normalInputs.bitangentWS;
					//���ݷ��߱���
                    o.uv = v.uv;
					//�������
                    return o;
                }

				//�������Ƭ��
                float4 frag(VertexOutput i): SV_Target 
                {
				//------������ͼת���編��--------
				//���뷨����ͼ
				float4 normalTXS = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, i.uv);

				//��ͼ��ɫ 0~1 ת -1~1�������ŷ���ǿ��
				float3 normalTS = UnpackNormalScale(normalTXS,_NormalScale);

				//��ͼ����ת��Ϊ���編��
				half3 normalWS = TransformTangentToWorld(normalTS,real3x3(i.tangentWS, i.bitangentWS, i.normalWS));



					   //-----------��Ӱ����--------------
					   //��ǰģ�ͽ�����Ӱ
				float4 SHADOW_COORDS = TransformWorldToShadowCoord(i.positionWS);
					//�����������
					Light  lightData = GetMainLight(SHADOW_COORDS);

					//��Ӱ����
					half shadow = lightData.shadowAttenuation;

					//���ս���
					 float Ramp_light=saturate(dot( lightData.direction, normalWS)*0.5+0.5);

				   //��ȡ���� = �������루��������������ظ���UV���꣩
                    float4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

					 //��ɫ���ӹ���
					 _Color.rgb *= Ramp_light *lightData.color.rgb*shadow+_GlossyEnvironmentColor.rgb;

					 
					 //������ɫ
					_Color.rgb *=baseTex.rgb;


					 //---------�߹�--------
					//��������ƹⷽ��
					float3 lightDir = normalize(lightData.direction);

					//���������
					float3 viewDir = normalize(GetCameraPositionWS().xyz-i.positionWS);

					//ʹ�÷����Դ�����뷨����߹ⷴ��
					float3 lightReflectDir = normalize(reflect(-lightDir,normalWS));
			
					//�߹⽥�� = ����������ӽǵ��
				float 	Ramp_Specular = saturate(dot(lightReflectDir,viewDir));
				//�߹���� = �η����߹⽥�䣬�߹ⷶΧ��*�߹���ɫ*�߹�ǿ�ȣ�͸���ȣ�
				float3 SpecularColor = pow(Ramp_Specular,_SpecularRange)*_SpecularPower*_SpecularColor.rgb*_SpecularColor.a;

				//��ϸ߹�(��Ӱ�ڵ��߹�)
				_Color.rgb +=SpecularColor*shadow;
					//͸���Ȼ��
					_Color.a +=baseTex.a;

					//�����ɫ
                    return  _Color ;
                }

                ENDHLSL  //URP ��������
        }
		//��ǰģ�ʹ�����Ӱ����
	 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}