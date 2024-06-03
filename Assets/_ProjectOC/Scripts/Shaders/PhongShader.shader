Shader "Universal Render Pipeline/CRLuo/CRLuo_URP_05_Specular" //URP路径名
{
     //面板属性
    Properties
    {
	    //基础颜色
		_Color("基础颜色", Color) = (0.5,0.5,0.5,1)
		//纹理贴图
        _MainTex ("主贴图", 2D) = "white" {}

	    //法线贴图
		_NormalTex("法线贴图", 2D) = "bump" {}
		//法线强度
       _NormalScale("法线强度", Float) = 1.0

		_SpecularColor("高光颜色", Color) = (1,1,1,1)
		_SpecularPower("高光强度", Range( 1, 10)) = 1
		_SpecularRange("高光范围", Range( 1, 100)) = 40
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
            HLSLPROGRAM  //URP 程序块开始
			#pragma target 4.5
			#pragma vertex vert
            #pragma fragment frag

			//URP函数库
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			#pragma multi_compile  _MAIN_LIGHT_SHADOWS
			#pragma multi_compile  _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile  _SHADOWS_SOFT

			CBUFFER_START(UnityPerMaterial) //变量引入开始

            //获取属性面板颜色
            float4 _Color; 
            float _NormalScale;
            float4	   _SpecularColor;
			float  _SpecularPower;
			float	_SpecularRange;
            CBUFFER_END //变量引入结束
			

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_NormalTex);
            SAMPLER(sampler_NormalTex);




			//定义模型原始数据结构
            struct VertexInput          
            {
				//物体空间顶点坐标
                float4 positionOS : POSITION; 

				//模型UV坐标
                float2 uv : TEXCOORD0;

				//模型法线
				float4 normalOS  : NORMAL;

				//物体空间切线
				float4 tangentOS  : TANGENT;
            };


			//定义顶点程序片段与表i面程序片段的传递数据结构
            struct VertexOutput 
            {
			   //物体裁切空间坐标
                float4 position : SV_POSITION; 
				
				//UV坐标
                float2 uv : TEXCOORD0;
				//世界空间顶点
				float3 positionWS :  TEXCOORD1;
				//世界空间法线
				float3 normalWS : TEXCOORD2;
				//世界空间切线
				float3 tangentWS : TEXCOORD3;
				//世界空间副切线
				float3 bitangentWS : TEXCOORD4;
            };

			
				//顶点程序片段
                VertexOutput vert(VertexInput v)
                {
				   //声明输出变量o
                    VertexOutput o;

					//输入物体空间顶点数据
					VertexPositionInputs positionInputs = GetVertexPositionInputs(v.positionOS.xyz);
					//获取裁切空间顶点
                    o.position = positionInputs.positionCS;
					//获取世界空间顶点
					o.positionWS = positionInputs.positionWS;

					//输入物体空间法线数据
					VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS.xyz,v.tangentOS);

					//获取世界空间法线
					o.normalWS = normalInputs.normalWS;
					//获取世界空间顶点
					o.tangentWS = normalInputs.tangentWS;
					//获取世界空间顶点
					o.bitangentWS = normalInputs.bitangentWS;
					//传递法线变量
                    o.uv = v.uv;
					//输出数据
                    return o;
                }

				//表面程序片段
                float4 frag(VertexOutput i): SV_Target 
                {
				//------法线贴图转世界法线--------
				//载入法线贴图
				float4 normalTXS = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, i.uv);

				//贴图颜色 0~1 转 -1~1并且缩放法线强度
				float3 normalTS = UnpackNormalScale(normalTXS,_NormalScale);

				//贴图法线转换为世界法线
				half3 normalWS = TransformTangentToWorld(normalTS,real3x3(i.tangentWS, i.bitangentWS, i.normalWS));



					   //-----------阴影数据--------------
					   //当前模型接收阴影
				float4 SHADOW_COORDS = TransformWorldToShadowCoord(i.positionWS);
					//放入光照数据
					Light  lightData = GetMainLight(SHADOW_COORDS);

					//阴影数据
					half shadow = lightData.shadowAttenuation;

					//光照渐变
					 float Ramp_light=saturate(dot( lightData.direction, normalWS)*0.5+0.5);

				   //获取纹理 = 纹理载入（纹理变量，纹理重复，UV坐标）
                    float4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

					 //颜色叠加光照
					 _Color.rgb *= Ramp_light *lightData.color.rgb*shadow+_GlossyEnvironmentColor.rgb;

					 
					 //光照着色
					_Color.rgb *=baseTex.rgb;


					 //---------高光--------
					//世界坐标灯光方向
					float3 lightDir = normalize(lightData.direction);

					//摄像机方向
					float3 viewDir = normalize(GetCameraPositionWS().xyz-i.positionWS);

					//使用反向光源方向与法线求高光反射
					float3 lightReflectDir = normalize(reflect(-lightDir,normalWS));
			
					//高光渐变 = 反射光线与视角点乘
				float 	Ramp_Specular = saturate(dot(lightReflectDir,viewDir));
				//高光解算 = 次方（高光渐变，高光范围）*高光颜色*高光强度（透明度）
				float3 SpecularColor = pow(Ramp_Specular,_SpecularRange)*_SpecularPower*_SpecularColor.rgb*_SpecularColor.a;

				//混合高光(阴影遮挡高光)
				_Color.rgb +=SpecularColor*shadow;
					//透明度混合
					_Color.a +=baseTex.a;

					//输出颜色
                    return  _Color ;
                }

                ENDHLSL  //URP 程序块结束
        }
		//当前模型创建阴影计算
	 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}