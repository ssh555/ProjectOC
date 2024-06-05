Shader "Universal Render Pipeline/GridUVScale"
{
     //面板属性
    Properties
    {
        _UVScale("UVScale",float) = 1
	    //基础颜色
		_MainColor("MainColor", Color) = (1,1,1,1)
        _MainTex ("MainTex", 2D) = "white" {}
    }
        SubShader
        {
        	Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"  }
			//多距离级别
            LOD 100 
            //Cull [_CullMode]
		 Pass
        {
        	Tags
            {
                "LightMode" = "UniversalForward"
                //"LightMode" = "UniversalGBuffer"
            }
            HLSLPROGRAM  //URP 程序块开始
			#pragma target 4.5
			//顶点程序片段 vert
			#pragma vertex vert
			//表面程序片段 frag
            #pragma fragment frag
            

			//URP函数库
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			CBUFFER_START(UnityPerMaterial) //变量引入开始
			float _UVScale;
            float4 _MainColor; 
            CBUFFER_END //变量引入结束

			//获取面板纹理,//创建贴图收容器
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);



			//定义模型原始数据结构
            struct VertexInput          
            {
                float4 positionOS : POSITION; 
                float4 normalOS  : NORMAL;
            };


			//定义顶点程序片段与表i面程序片段的传递数据结构
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

                //输入物体空间顶点数据
				VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.position = positionInputs.positionCS;
				o.positionWS = positionInputs.positionWS;
				VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS.xyz);
				o.normalWS = normalInputs.normalWS;
                return o;
            }

			//表面程序片段

            float4 frag(VertexOutput IN): SV_Target 
            {	                
                //主光源
                Light mainLight = GetMainLight(); 
                float4 mainLightColor = float4(mainLight.color, 1); //获取主光源颜色
                float3 worldLightDir = normalize(mainLight.direction); //主光源方向
                float3 blendAxes = abs(IN.normalWS);
				blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
                float3 textureColor = triplanar(IN.positionWS,_UVScale,blendAxes);

                //方向点积
                float ndotLRaw = dot(IN.normalWS, worldLightDir);
                float halfLambert = ndotLRaw * 0.5f + 0.5f;

                float3 diffuse = textureColor * mainLightColor.xyz* halfLambert * _MainColor.xyz;
                float3 result = diffuse;
                
                return  float4(result,1.0f);
            }

            ENDHLSL  //URP 程序块结束            
        }
UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
