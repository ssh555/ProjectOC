#define NUM_TEX_COORD_INTERPOLATORS 1
#define NUM_MATERIAL_TEXCOORDS_VERTEX 1
#define NUM_CUSTOM_VERTEX_INTERPOLATORS 0

struct Input
{
	//float3 Normal;
	float2 uv_MainTex : TEXCOORD0;
	float2 uv2_Material_Texture2D_0 : TEXCOORD1;
	float4 color : COLOR;
	float4 tangent;
	//float4 normal;
	float3 viewDir;
	float4 screenPos;
	float3 worldPos;
	//float3 worldNormal;
	float3 normal2;
};
struct SurfaceOutputStandard
{
	float3 Albedo;		// base (diffuse or specular) color
	float3 Normal;		// tangent space normal, if written
	half3 Emission;
	half Metallic;		// 0=non-metal, 1=metal
	// Smoothness is the user facing name, it should be perceptual smoothness but user should not have to deal with it.
	// Everywhere in the code you meet smoothness it is perceptual smoothness
	half Smoothness;	// 0=rough, 1=smooth
	half Occlusion;		// occlusion (default 1)
	float Alpha;		// alpha for transparencies
};

//#define HDRP 1
#define URP 1
//#define UE5
//#define HAS_CUSTOMIZED_UVS 1
#define MATERIAL_TANGENTSPACENORMAL 1
//struct Material
//{
	//samplers start
SAMPLER( SamplerState_Linear_Repeat );
SAMPLER( SamplerState_Linear_Clamp );
TEXTURE2D(       Material_Texture2D_0 );
SAMPLER( sampler_Material_Texture2D_0 );
TEXTURE2D(       Material_Texture2D_1 );
SAMPLER( sampler_Material_Texture2D_1 );
TEXTURE2D(       Material_Texture2D_2 );
SAMPLER( sampler_Material_Texture2D_2 );
TEXTURE2D(       Material_Texture2D_3 );
SAMPLER( sampler_Material_Texture2D_3 );
TEXTURE2D(       Material_Texture2D_4 );
SAMPLER( sampler_Material_Texture2D_4 );
TEXTURE2D(       Material_Texture2D_5 );
SAMPLER( sampler_Material_Texture2D_5 );
TEXTURE2D(       Material_Texture2D_6 );
SAMPLER( sampler_Material_Texture2D_6 );
TEXTURE2D(       Material_Texture2D_7 );
SAMPLER( sampler_Material_Texture2D_7 );
TEXTURE2D(       Material_Texture2D_8 );
SAMPLER( sampler_Material_Texture2D_8 );
TEXTURE2D(       Material_Texture2D_9 );
SAMPLER( sampler_Material_Texture2D_9 );
TEXTURE2D(       Material_Texture2D_10 );
SAMPLER( sampler_Material_Texture2D_10 );
TEXTURE2D(       Material_Texture2D_11 );
SAMPLER( sampler_Material_Texture2D_11 );
TEXTURE2D(       Material_Texture2D_12 );
SAMPLER( sampler_Material_Texture2D_12 );
TEXTURE2D(       Material_Texture2D_13 );
SAMPLER( sampler_Material_Texture2D_13 );
TEXTURE2D(       Material_Texture2D_14 );
SAMPLER( sampler_Material_Texture2D_14 );
TEXTURE2D(       Material_Texture2D_15 );
SAMPLER( sampler_Material_Texture2D_15 );
TEXTURE2D(       Material_Texture2D_16 );
SAMPLER( sampler_Material_Texture2D_16 );
TEXTURE2D(       Material_Texture2D_17 );
SAMPLER( sampler_Material_Texture2D_17 );

//};

#ifdef UE5
	#define UE_LWC_RENDER_TILE_SIZE			2097152.0
	#define UE_LWC_RENDER_TILE_SIZE_SQRT	1448.15466
	#define UE_LWC_RENDER_TILE_SIZE_RSQRT	0.000690533954
	#define UE_LWC_RENDER_TILE_SIZE_RCP		4.76837158e-07
	#define UE_LWC_RENDER_TILE_SIZE_FMOD_PI		0.673652053
	#define UE_LWC_RENDER_TILE_SIZE_FMOD_2PI	0.673652053
	#define INVARIANT(X) X
	#define PI 					(3.1415926535897932)

	//#include "LargeWorldCoordinates.hlsl"
#endif
struct MaterialStruct
{
	float4 VectorExpressions[3];
	float4 ScalarExpressions[10];
	float VTPackedPageTableUniform[2];
	float VTPackedUniform[1];
};
static SamplerState View_MaterialTextureBilinearWrapedSampler;
static SamplerState View_MaterialTextureBilinearClampedSampler;
struct ViewStruct
{
	float GameTime;
	float RealTime;
	float DeltaTime;
	float PrevFrameGameTime;
	float PrevFrameRealTime;
	float MaterialTextureMipBias;	
	float4 PrimitiveSceneData[ 40 ];
	float4 TemporalAAParams;
	float2 ViewRectMin;
	float4 ViewSizeAndInvSize;
	float MaterialTextureDerivativeMultiply;
	uint StateFrameIndexMod8;
	float FrameNumber;
	float2 FieldOfViewWideAngles;
	float4 RuntimeVirtualTextureMipLevel;
	float PreExposure;
	float4 BufferBilinearUVMinMax;
};
struct ResolvedViewStruct
{
	#ifdef UE5
		FLWCVector3 WorldCameraOrigin;
		FLWCVector3 PrevWorldCameraOrigin;
		FLWCVector3 PreViewTranslation;
		FLWCVector3 WorldViewOrigin;
	#else
		float3 WorldCameraOrigin;
		float3 PrevWorldCameraOrigin;
		float3 PreViewTranslation;
		float3 WorldViewOrigin;
	#endif
	float4 ScreenPositionScaleBias;
	float4x4 TranslatedWorldToView;
	float4x4 TranslatedWorldToCameraView;
	float4x4 TranslatedWorldToClip;
	float4x4 ViewToTranslatedWorld;
	float4x4 PrevViewToTranslatedWorld;
	float4x4 CameraViewToTranslatedWorld;
	float4 BufferBilinearUVMinMax;
	float4 XRPassthroughCameraUVs[ 2 ];
};
struct PrimitiveStruct
{
	float4x4 WorldToLocal;
	float4x4 LocalToWorld;
};

static ViewStruct View;
static ResolvedViewStruct ResolvedView;
static PrimitiveStruct Primitive;
uniform float4 View_BufferSizeAndInvSize;
uniform float4 LocalObjectBoundsMin;
uniform float4 LocalObjectBoundsMax;
static SamplerState Material_Wrap_WorldGroupSettings;
static SamplerState Material_Clamp_WorldGroupSettings;

#include "CGL_UnrealCommon.cginc"

static MaterialStruct Material;
void InitializeExpressions()
{
	Material.VectorExpressions[0] = float4(0.000000,0.000000,0.000000,0.000000);//SelectionColor
	Material.VectorExpressions[1] = float4(0.000000,0.000000,1.000000,0.000000);//Noise Map Channel
	Material.VectorExpressions[2] = float4(0.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.ScalarExpressions[0] = float4(1.000000,5.000000,45.000000,15000.000000);//WeightMapScale Layer_01 Texture Size Near Layer_01 Texture Size Far Distance Fade Length
	Material.ScalarExpressions[1] = float4(600.000000,0.514286,25.000000,70.000000);//Distance Fade Offset Layer_01 Flatten Normal Layer_03 Texture Size Near Layer_03 Texture Size Far
	Material.ScalarExpressions[2] = float4(0.000000,1.120609,15.000000,1.000000);//Layer_03 Flatten Normal Steep Slope Intensity Steep Slope Contrast Slope Noise Mask Contrast
	Material.ScalarExpressions[3] = float4(500.000000,2.000000,-1.000000,1500.000000);//Slope Noise Mask Size (Unknown) (Unknown) Layer_02 Texture Size Near
	Material.ScalarExpressions[4] = float4(1.000000,2.000000,-1.000000,4000.000000);//Triplanar Transition Contrast (Unknown) (Unknown) Layer_02 Texture Size Far
	Material.ScalarExpressions[5] = float4(0.000000,1.267227,20.378632,10.000000);//Layer_02 Flatten Normal Medium Slope Intensity Medium Slope Contrast Layer_04 Texture Size Near
	Material.ScalarExpressions[6] = float4(70.000000,0.000000,0.000000,-1.000000);//Layer_04 Texture Size Far Layer_04 Flatten Normal (Unknown) Noise Fade Amount
	Material.ScalarExpressions[7] = float4(5000.000000,8000.000000,3.000000,1.700000);//Blend Mask Size Near Blend Mask Size Far Color Var Blend Contrast Layer_01 Roughness Intensity
	Material.ScalarExpressions[8] = float4(1.000000,1.000000,1.000000,7.000000);//Layer_03 Roughness Intensity Layer_02 Roughness Intensity Layer_04 Roughness Intensity Grass Auto Contrast
	Material.ScalarExpressions[9] = float4(8.000000,-7.000000,0.000000,0.000000);//(Unknown) (Unknown) (Unknown) (Unknown)
}
MaterialFloat3 CustomExpression9(FMaterialPixelParameters Parameters,MaterialFloat AutoLandscapeWeight,MaterialFloat Layer_03Weight,MaterialFloat Layer_04Weight,MaterialFloat3 AutoLandscape,MaterialFloat3 Layer_03,MaterialFloat3 Layer_04,MaterialFloat Layer_03Height,MaterialFloat Layer_04Height)
{
float  lerpres;
float  Local0;

lerpres = lerp( -1.0, 1.0, Layer_03Weight );
Local0 = ( lerpres + Layer_03Height );
float Layer3WithHeight = clamp(Local0, 0.0001, 1.0);

lerpres = lerp( -1.0, 1.0, Layer_04Weight );
Local0 = ( lerpres + Layer_04Height );
float Layer4WithHeight = clamp(Local0, 0.0001, 1.0);

float  AllWeightsAndHeights = AutoLandscapeWeight.r + Layer3WithHeight + Layer4WithHeight + 0;
float  Divider = ( 1.0 / AllWeightsAndHeights );
float3  Layer0Contribution = Divider.rrr * AutoLandscapeWeight.rrr * AutoLandscape;
float3  Layer3Contribution = Divider.rrr * Layer3WithHeight.rrr * Layer_03;
float3  Layer4Contribution = Divider.rrr * Layer4WithHeight.rrr * Layer_04;
float3  Result = Layer0Contribution + Layer3Contribution + Layer4Contribution + float3(0,0,0);
return Result;
}

MaterialFloat3 CustomExpression8(FMaterialPixelParameters Parameters,MaterialFloat3 x)
{
return length(x);
}

MaterialFloat3 CustomExpression7(FMaterialPixelParameters Parameters,MaterialFloat AutoLandscapeWeight,MaterialFloat Layer_03Weight,MaterialFloat Layer_04Weight,MaterialFloat3 AutoLandscape,MaterialFloat Layer_03,MaterialFloat Layer_04,MaterialFloat Layer_03Height,MaterialFloat Layer_04Height)
{
float  lerpres;
float  Local0;

lerpres = lerp( -1.0, 1.0, Layer_03Weight );
Local0 = ( lerpres + Layer_03Height );
float Layer3WithHeight = clamp(Local0, 0.0001, 1.0);

lerpres = lerp( -1.0, 1.0, Layer_04Weight );
Local0 = ( lerpres + Layer_04Height );
float Layer4WithHeight = clamp(Local0, 0.0001, 1.0);

float  AllWeightsAndHeights = AutoLandscapeWeight.r + Layer3WithHeight + Layer4WithHeight + 0;
float  Divider = ( 1.0 / AllWeightsAndHeights );
float3  Layer0Contribution = Divider.rrr * AutoLandscapeWeight.rrr * AutoLandscape;
float3  Layer3Contribution = Divider.rrr * Layer3WithHeight.rrr * Layer_03;
float3  Layer4Contribution = Divider.rrr * Layer4WithHeight.rrr * Layer_04;
float3  Result = Layer0Contribution + Layer3Contribution + Layer4Contribution + float3(0,0,0);
return Result;
}

MaterialFloat3 CustomExpression6(FMaterialPixelParameters Parameters,MaterialFloat3 x)
{
return length(x);
}

MaterialFloat3 CustomExpression5(FMaterialPixelParameters Parameters,MaterialFloat AutoLandscapeWeight,MaterialFloat Layer_03Weight,MaterialFloat Layer_04Weight,MaterialFloat3 AutoLandscape,MaterialFloat3 Layer_03,MaterialFloat3 Layer_04,MaterialFloat Layer_03Height,MaterialFloat Layer_04Height)
{
float  lerpres;
float  Local0;

lerpres = lerp( -1.0, 1.0, Layer_03Weight );
Local0 = ( lerpres + Layer_03Height );
float Layer3WithHeight = clamp(Local0, 0.0001, 1.0);

lerpres = lerp( -1.0, 1.0, Layer_04Weight );
Local0 = ( lerpres + Layer_04Height );
float Layer4WithHeight = clamp(Local0, 0.0001, 1.0);

float  AllWeightsAndHeights = AutoLandscapeWeight.r + Layer3WithHeight + Layer4WithHeight + 0;
float  Divider = ( 1.0 / AllWeightsAndHeights );
float3  Layer0Contribution = Divider.rrr * AutoLandscapeWeight.rrr * AutoLandscape;
float3  Layer3Contribution = Divider.rrr * Layer3WithHeight.rrr * Layer_03;
float3  Layer4Contribution = Divider.rrr * Layer4WithHeight.rrr * Layer_04;
float3  Result = Layer0Contribution + Layer3Contribution + Layer4Contribution + float3(0,0,0);
return Result;
}

MaterialFloat3 CustomExpression4(FMaterialPixelParameters Parameters,MaterialFloat3 x)
{
return length(x);
}

MaterialFloat3 CustomExpression3(FMaterialPixelParameters Parameters,MaterialFloat AutoLandscapeWeight,MaterialFloat Layer_03Weight,MaterialFloat Layer_04Weight,MaterialFloat3 AutoLandscape,MaterialFloat Layer_03,MaterialFloat Layer_04,MaterialFloat Layer_03Height,MaterialFloat Layer_04Height)
{
float  lerpres;
float  Local0;

lerpres = lerp( -1.0, 1.0, Layer_03Weight );
Local0 = ( lerpres + Layer_03Height );
float Layer3WithHeight = clamp(Local0, 0.0001, 1.0);

lerpres = lerp( -1.0, 1.0, Layer_04Weight );
Local0 = ( lerpres + Layer_04Height );
float Layer4WithHeight = clamp(Local0, 0.0001, 1.0);

float  AllWeightsAndHeights = AutoLandscapeWeight.r + Layer3WithHeight + Layer4WithHeight + 0;
float  Divider = ( 1.0 / AllWeightsAndHeights );
float3  Layer0Contribution = Divider.rrr * AutoLandscapeWeight.rrr * AutoLandscape;
float3  Layer3Contribution = Divider.rrr * Layer3WithHeight.rrr * Layer_03;
float3  Layer4Contribution = Divider.rrr * Layer4WithHeight.rrr * Layer_04;
float3  Result = Layer0Contribution + Layer3Contribution + Layer4Contribution + float3(0,0,0);
return Result;
}

MaterialFloat3 CustomExpression2(FMaterialPixelParameters Parameters,MaterialFloat AutoLandscapeWeight,MaterialFloat Layer_03Weight,MaterialFloat Layer_04Weight,MaterialFloat3 AutoLandscape,MaterialFloat3 Layer_03,MaterialFloat3 Layer_04,MaterialFloat Layer_03Height,MaterialFloat Layer_04Height)
{
float  lerpres;
float  Local0;

lerpres = lerp( -1.0, 1.0, Layer_03Weight );
Local0 = ( lerpres + Layer_03Height );
float Layer3WithHeight = clamp(Local0, 0.0001, 1.0);

lerpres = lerp( -1.0, 1.0, Layer_04Weight );
Local0 = ( lerpres + Layer_04Height );
float Layer4WithHeight = clamp(Local0, 0.0001, 1.0);

float  AllWeightsAndHeights = AutoLandscapeWeight.r + Layer3WithHeight + Layer4WithHeight + 0;
float  Divider = ( 1.0 / AllWeightsAndHeights );
float3  Layer0Contribution = Divider.rrr * AutoLandscapeWeight.rrr * AutoLandscape;
float3  Layer3Contribution = Divider.rrr * Layer3WithHeight.rrr * Layer_03;
float3  Layer4Contribution = Divider.rrr * Layer4WithHeight.rrr * Layer_04;
float3  Result = Layer0Contribution + Layer3Contribution + Layer4Contribution + float3(0,0,0);
return Result;
}

MaterialFloat3 CustomExpression1(FMaterialPixelParameters Parameters,MaterialFloat AutoLandscapeWeight,MaterialFloat Layer_03Weight,MaterialFloat Layer_04Weight,MaterialFloat3 AutoLandscape,MaterialFloat3 Layer_03,MaterialFloat3 Layer_04,MaterialFloat Layer_03Height,MaterialFloat Layer_04Height)
{
float  lerpres;
float  Local0;

lerpres = lerp( -1.0, 1.0, Layer_03Weight );
Local0 = ( lerpres + Layer_03Height );
float Layer3WithHeight = clamp(Local0, 0.0001, 1.0);

lerpres = lerp( -1.0, 1.0, Layer_04Weight );
Local0 = ( lerpres + Layer_04Height );
float Layer4WithHeight = clamp(Local0, 0.0001, 1.0);

float  AllWeightsAndHeights = AutoLandscapeWeight.r + Layer3WithHeight + Layer4WithHeight + 0;
float  Divider = ( 1.0 / AllWeightsAndHeights );
float3  Layer0Contribution = Divider.rrr * AutoLandscapeWeight.rrr * AutoLandscape;
float3  Layer3Contribution = Divider.rrr * Layer3WithHeight.rrr * Layer_03;
float3  Layer4Contribution = Divider.rrr * Layer4WithHeight.rrr * Layer_04;
float3  Result = Layer0Contribution + Layer3Contribution + Layer4Contribution + float3(0,0,0);
return Result;
}

MaterialFloat3 CustomExpression0(FMaterialPixelParameters Parameters,MaterialFloat3 x)
{
return length(x);
}
float3 GetMaterialWorldPositionOffset(FMaterialVertexParameters Parameters)
{
	#if USE_INSTANCING
		// skip if this instance is hidden
		if (Parameters.PerInstanceParams.z < 1.f)
		{
			return float3(0,0,0);
		}
	#endif
	return MaterialFloat3(0.00000000,0.00000000,0.00000000);;
}
void CalcPixelMaterialInputs(in out FMaterialPixelParameters Parameters, in out FPixelMaterialInputs PixelMaterialInputs)
{
	//WorldAligned texturing & others use normals & stuff that think Z is up
	Parameters.TangentToWorld[0] = Parameters.TangentToWorld[0].xzy;
	Parameters.TangentToWorld[1] = Parameters.TangentToWorld[1].xzy;
	Parameters.TangentToWorld[2] = Parameters.TangentToWorld[2].xzy;

	float3 WorldNormalCopy = Parameters.WorldNormal;

	// Initial calculations (required for Normal)
	MaterialFloat2 Local0 = (Parameters.TexCoords[0].xy * Material.ScalarExpressions[0].x);
	MaterialFloat Local1 = MaterialStoreTexCoordScale(Parameters, Local0, 0);
	MaterialFloat4 Local2 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_0, GetMaterialSharedSampler(sampler_Material_Texture2D_0,View_MaterialTextureBilinearClampedSampler),Local0));
	MaterialFloat Local3 = MaterialStoreTexSample(Parameters, Local2, 0);
	MaterialFloat4 Local4 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_1, GetMaterialSharedSampler(sampler_Material_Texture2D_1,View_MaterialTextureBilinearClampedSampler),Local0));
	MaterialFloat Local5 = MaterialStoreTexSample(Parameters, Local4, 0);
	MaterialFloat4 Local6 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_2, GetMaterialSharedSampler(sampler_Material_Texture2D_2,View_MaterialTextureBilinearClampedSampler),Local0));
	MaterialFloat Local7 = MaterialStoreTexSample(Parameters, Local6, 0);
	MaterialFloat2 Local8 = (Parameters.TexCoords[0].xy / Material.ScalarExpressions[0].y);
	MaterialFloat Local9 = MaterialStoreTexCoordScale(Parameters, Local8, 3);
	MaterialFloat4 Local10 = UnpackNormalMap(Texture2DSample(Material_Texture2D_3, GetMaterialSharedSampler(sampler_Material_Texture2D_3,View_MaterialTextureBilinearWrapedSampler),Local8));
	MaterialFloat Local11 = MaterialStoreTexSample(Parameters, Local10, 3);
	MaterialFloat2 Local12 = (Parameters.TexCoords[0].xy / Material.ScalarExpressions[0].z);
	MaterialFloat Local13 = MaterialStoreTexCoordScale(Parameters, Local12, 3);
	MaterialFloat4 Local14 = UnpackNormalMap(Texture2DSample(Material_Texture2D_3, GetMaterialSharedSampler(sampler_Material_Texture2D_3,View_MaterialTextureBilinearWrapedSampler),Local12));
	MaterialFloat Local15 = MaterialStoreTexSample(Parameters, Local14, 3);
	MaterialFloat3 Local16 = ResolvedView.WorldCameraOrigin;
	MaterialFloat3 Local17 = (Local16 - GetWorldPosition(Parameters));
	MaterialFloat3 Local18 = CustomExpression0(Parameters,Local17);
	MaterialFloat Local19 = dot(Local17, Local17);
	MaterialFloat Local20 = sqrt(Local19);
	MaterialFloat3 Local21 = (Local17 / Local20);
	MaterialFloat3 Local22 = mul(MaterialFloat3(0.00000000,0.00000000,-1.00000000), (MaterialFloat3x3)(ResolvedView.ViewToTranslatedWorld));
	MaterialFloat3 Local23 = Local22;
	MaterialFloat Local24 = dot(Local21, Local23);
	MaterialFloat3 Local25 = (Local18 * Local24);
	MaterialFloat3 Local26 = (Local25 - Material.ScalarExpressions[0].w);
	MaterialFloat3 Local27 = (Local26 / Material.ScalarExpressions[1].x);
	MaterialFloat3 Local28 = min(max(Local27,MaterialFloat3(0.00000000,0.00000000,0.00000000)),MaterialFloat3(1.00000000,1.00000000,1.00000000));
	MaterialFloat3 Local29 = lerp(Local10.rgb,Local14.rgb,MaterialFloat(MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r));
	MaterialFloat3 Local30 = lerp(Local29.rgb,MaterialFloat3(0.00000000,0.00000000,1.00000000),MaterialFloat(Material.ScalarExpressions[1].y));
	MaterialFloat2 Local31 = (Parameters.TexCoords[0].xy / Material.ScalarExpressions[1].z);
	MaterialFloat Local32 = MaterialStoreTexCoordScale(Parameters, Local31, 3);
	MaterialFloat4 Local33 = UnpackNormalMap(Texture2DSample(Material_Texture2D_4, GetMaterialSharedSampler(sampler_Material_Texture2D_4,View_MaterialTextureBilinearWrapedSampler),Local31));
	MaterialFloat Local34 = MaterialStoreTexSample(Parameters, Local33, 3);
	MaterialFloat2 Local35 = (Parameters.TexCoords[0].xy / Material.ScalarExpressions[1].w);
	MaterialFloat Local36 = MaterialStoreTexCoordScale(Parameters, Local35, 3);
	MaterialFloat4 Local37 = UnpackNormalMap(Texture2DSample(Material_Texture2D_4, GetMaterialSharedSampler(sampler_Material_Texture2D_4,View_MaterialTextureBilinearWrapedSampler),Local35));
	MaterialFloat Local38 = MaterialStoreTexSample(Parameters, Local37, 3);
	MaterialFloat3 Local39 = lerp(Local33.rgb,Local37.rgb,MaterialFloat(MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r));
	MaterialFloat3 Local40 = lerp(Local39.rgb,MaterialFloat3(0.00000000,0.00000000,1.00000000),MaterialFloat(Material.ScalarExpressions[2].x));
	MaterialFloat Local41 = (Parameters.TangentToWorld[2].b * Material.ScalarExpressions[2].y);
	MaterialFloat Local42 = PositiveClampedPow(Local41,Material.ScalarExpressions[2].z);
	MaterialFloat Local43 = min(max(Local42,0.00000000),1.00000000);
	MaterialFloat Local44 = (1.00000000 - Local43);
	MaterialFloat Local45 = (1.00000000 - Local44);
	MaterialFloat Local46 = (Local45 * 2.00000000);
	MaterialFloat2 Local47 = (GetWorldPosition(Parameters).rg / Material.ScalarExpressions[3].x);
	MaterialFloat Local48 = MaterialStoreTexCoordScale(Parameters, Local47, 13);
	MaterialFloat4 Local49 = ProcessMaterialLinearColorTextureLookup(Texture2DSampleBias(Material_Texture2D_5, sampler_Material_Texture2D_5,Local47,View.MaterialTextureMipBias));
	MaterialFloat Local50 = MaterialStoreTexSample(Parameters, Local49, 13);
	MaterialFloat Local51 = dot(MaterialFloat4(Local49.rgb,0), Material.VectorExpressions[1]);
	MaterialFloat Local52 = (1.00000000 - Local51.r);
	MaterialFloat Local53 = lerp(Material.ScalarExpressions[3].z,Material.ScalarExpressions[3].y,Local52);
	MaterialFloat Local54 = min(max(Local53,0.00000000),1.00000000);
	MaterialFloat Local55 = (1.00000000 - Local54.r);
	MaterialFloat Local56 = (Local46 * Local55);
	MaterialFloat Local57 = (1.00000000 - Local56);
	MaterialFloat Local58 = (Local44 * 2.00000000);
	MaterialFloat Local59 = (Local58 * Local54.r);
	MaterialFloat Local60 = ((Local44.r >= 0.50000000) ? Local57.r : Local59.r);
	MaterialFloat3 Local61 = lerp(Local30.rgb,Local40.rgb,MaterialFloat3(MaterialFloat2(Local60,Local60),Local60).r);
	MaterialFloat3 Local62 = (GetWorldPosition(Parameters) / Material.ScalarExpressions[3].w);
	MaterialFloat Local63 = MaterialStoreTexCoordScale(Parameters, Local62.rb, 3);
	MaterialFloat4 Local64 = UnpackNormalMap(Texture2DSample(Material_Texture2D_6, GetMaterialSharedSampler(sampler_Material_Texture2D_6,View_MaterialTextureBilinearWrapedSampler),Local62.rb));
	MaterialFloat Local65 = MaterialStoreTexSample(Parameters, Local64, 3);
	MaterialFloat Local66 = (Parameters.TangentToWorld[2].r + Local64.rgb.r);
	MaterialFloat Local67 = (Parameters.TangentToWorld[2].g * Local64.rgb.b);
	MaterialFloat Local68 = (Parameters.TangentToWorld[2].b + Local64.rgb.g);
	MaterialFloat Local69 = MaterialStoreTexCoordScale(Parameters, Local62.gb, 3);
	MaterialFloat4 Local70 = UnpackNormalMap(Texture2DSample(Material_Texture2D_6, GetMaterialSharedSampler(sampler_Material_Texture2D_6,View_MaterialTextureBilinearWrapedSampler),Local62.gb));
	MaterialFloat Local71 = MaterialStoreTexSample(Parameters, Local70, 3);
	MaterialFloat Local72 = (Parameters.TangentToWorld[2].r * Local70.rgb.b);
	MaterialFloat Local73 = (Parameters.TangentToWorld[2].g + Local70.rgb.r);
	MaterialFloat Local74 = (Parameters.TangentToWorld[2].b + Local70.rgb.g);
	MaterialFloat Local75 = abs(Parameters.TangentToWorld[2].r);
	MaterialFloat Local76 = lerp(Material.ScalarExpressions[4].z,Material.ScalarExpressions[4].y,Local75);
	MaterialFloat Local77 = min(max(Local76,0.00000000),1.00000000);
	MaterialFloat3 Local78 = lerp(MaterialFloat3(MaterialFloat2(Local66,Local67),Local68),MaterialFloat3(MaterialFloat2(Local72,Local73),Local74),MaterialFloat(Local77.r.r));
	MaterialFloat Local79 = dot(Local78, Local78);
	MaterialFloat Local80 = sqrt(Local79);
	MaterialFloat3 Local81 = (Local78 / Local80);
	MaterialFloat3 Local82 = mul((MaterialFloat3x3)(Parameters.TangentToWorld), Local81);
	MaterialFloat3 Local83 = (GetWorldPosition(Parameters) / Material.ScalarExpressions[4].w);
	MaterialFloat Local84 = MaterialStoreTexCoordScale(Parameters, Local83.rb, 3);
	MaterialFloat4 Local85 = UnpackNormalMap(Texture2DSample(Material_Texture2D_6, GetMaterialSharedSampler(sampler_Material_Texture2D_6,View_MaterialTextureBilinearWrapedSampler),Local83.rb));
	MaterialFloat Local86 = MaterialStoreTexSample(Parameters, Local85, 3);
	MaterialFloat Local87 = (Parameters.TangentToWorld[2].r + Local85.rgb.r);
	MaterialFloat Local88 = (Parameters.TangentToWorld[2].g * Local85.rgb.b);
	MaterialFloat Local89 = (Parameters.TangentToWorld[2].b + Local85.rgb.g);
	MaterialFloat Local90 = MaterialStoreTexCoordScale(Parameters, Local83.gb, 3);
	MaterialFloat4 Local91 = UnpackNormalMap(Texture2DSample(Material_Texture2D_6, GetMaterialSharedSampler(sampler_Material_Texture2D_6,View_MaterialTextureBilinearWrapedSampler),Local83.gb));
	MaterialFloat Local92 = MaterialStoreTexSample(Parameters, Local91, 3);
	MaterialFloat Local93 = (Parameters.TangentToWorld[2].r * Local91.rgb.b);
	MaterialFloat Local94 = (Parameters.TangentToWorld[2].g + Local91.rgb.r);
	MaterialFloat Local95 = (Parameters.TangentToWorld[2].b + Local91.rgb.g);
	MaterialFloat3 Local96 = lerp(MaterialFloat3(MaterialFloat2(Local87,Local88),Local89),MaterialFloat3(MaterialFloat2(Local93,Local94),Local95),MaterialFloat(Local77.r.r));
	MaterialFloat Local97 = dot(Local96, Local96);
	MaterialFloat Local98 = sqrt(Local97);
	MaterialFloat3 Local99 = (Local96 / Local98);
	MaterialFloat3 Local100 = mul((MaterialFloat3x3)(Parameters.TangentToWorld), Local99);
	MaterialFloat3 Local101 = lerp(Local82,Local100,MaterialFloat(MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r));
	MaterialFloat3 Local102 = lerp(Local101.rgb,MaterialFloat3(0.00000000,0.00000000,1.00000000),MaterialFloat(Material.ScalarExpressions[5].x));
	MaterialFloat Local103 = (Parameters.TangentToWorld[2].b * Material.ScalarExpressions[5].y);
	MaterialFloat Local104 = PositiveClampedPow(Local103,Material.ScalarExpressions[5].z);
	MaterialFloat Local105 = min(max(Local104,0.00000000),1.00000000);
	MaterialFloat Local106 = (1.00000000 - Local105);
	MaterialFloat Local107 = (1.00000000 - Local106);
	MaterialFloat Local108 = (Local107 * 2.00000000);
	MaterialFloat Local109 = (Local108 * Local55);
	MaterialFloat Local110 = (1.00000000 - Local109);
	MaterialFloat Local111 = (Local106 * 2.00000000);
	MaterialFloat Local112 = (Local111 * Local54.r);
	MaterialFloat Local113 = ((Local106.r >= 0.50000000) ? Local110.r : Local112.r);
	MaterialFloat3 Local114 = lerp(Local61.rgb,Local102.rgb,MaterialFloat3(MaterialFloat2(Local113,Local113),Local113).r);
	MaterialFloat2 Local115 = (Parameters.TexCoords[0].xy / Material.ScalarExpressions[5].w);
	MaterialFloat Local116 = MaterialStoreTexCoordScale(Parameters, Local115, 3);
	MaterialFloat4 Local117 = UnpackNormalMap(Texture2DSample(Material_Texture2D_7, GetMaterialSharedSampler(sampler_Material_Texture2D_7,View_MaterialTextureBilinearWrapedSampler),Local115));
	MaterialFloat Local118 = MaterialStoreTexSample(Parameters, Local117, 3);
	MaterialFloat2 Local119 = (Parameters.TexCoords[0].xy / Material.ScalarExpressions[6].x);
	MaterialFloat Local120 = MaterialStoreTexCoordScale(Parameters, Local119, 3);
	MaterialFloat4 Local121 = UnpackNormalMap(Texture2DSample(Material_Texture2D_7, GetMaterialSharedSampler(sampler_Material_Texture2D_7,View_MaterialTextureBilinearWrapedSampler),Local119));
	MaterialFloat Local122 = MaterialStoreTexSample(Parameters, Local121, 3);
	MaterialFloat3 Local123 = lerp(Local117.rgb,Local121.rgb,MaterialFloat(MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r));
	MaterialFloat3 Local124 = lerp(Local123.rgb,MaterialFloat3(0.00000000,0.00000000,1.00000000),MaterialFloat(Material.ScalarExpressions[6].y));
	MaterialFloat Local125 = MaterialStoreTexCoordScale(Parameters, Local115, 0);
	MaterialFloat4 Local126 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_8, GetMaterialSharedSampler(sampler_Material_Texture2D_8,View_MaterialTextureBilinearWrapedSampler),Local115));
	MaterialFloat Local127 = MaterialStoreTexSample(Parameters, Local126, 0);
	MaterialFloat Local128 = MaterialStoreTexCoordScale(Parameters, Local119, 0);
	MaterialFloat4 Local129 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_8, GetMaterialSharedSampler(sampler_Material_Texture2D_8,View_MaterialTextureBilinearWrapedSampler),Local119));
	MaterialFloat Local130 = MaterialStoreTexSample(Parameters, Local129, 0);
	MaterialFloat Local131 = lerp(Local126.a,Local129.a,MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r);
	MaterialFloat3 Local132 = CustomExpression1(Parameters,Local2.r,Local4.r,Local6.r,Local114,Local40,Local124,0.00000000,Local131.rrr.r);

	// The Normal is a special case as it might have its own expressions and also be used to calculate other inputs, so perform the assignment here
	PixelMaterialInputs.Normal = Local132.rgb;


	// Note that here MaterialNormal can be in world space or tangent space
	float3 MaterialNormal = GetMaterialNormal(Parameters, PixelMaterialInputs);

#if MATERIAL_TANGENTSPACENORMAL
#if SIMPLE_FORWARD_SHADING
	Parameters.WorldNormal = float3(0, 0, 1);
#endif

#if FEATURE_LEVEL >= FEATURE_LEVEL_SM4
	// Mobile will rely on only the final normalize for performance
	MaterialNormal = normalize(MaterialNormal);
#endif

	// normalizing after the tangent space to world space conversion improves quality with sheared bases (UV layout to WS causes shrearing)
	// use full precision normalize to avoid overflows
	Parameters.WorldNormal = TransformTangentNormalToWorld(Parameters.TangentToWorld, MaterialNormal);

#else //MATERIAL_TANGENTSPACENORMAL

	Parameters.WorldNormal = normalize(MaterialNormal);

#endif //MATERIAL_TANGENTSPACENORMAL

#if MATERIAL_TANGENTSPACENORMAL
	// flip the normal for backfaces being rendered with a two-sided material
	Parameters.WorldNormal *= Parameters.TwoSidedSign;
#endif

	Parameters.ReflectionVector = ReflectionAboutCustomWorldNormal(Parameters, Parameters.WorldNormal, false);

#if !PARTICLE_SPRITE_FACTORY
	Parameters.Particle.MotionBlurFade = 1.0f;
#endif // !PARTICLE_SPRITE_FACTORY

	// Now the rest of the inputs
	MaterialFloat3 Local133 = lerp(MaterialFloat3(0.00000000,0.00000000,0.00000000),Material.VectorExpressions[2].rgb,MaterialFloat(Material.ScalarExpressions[6].z));
	MaterialFloat Local134 = MaterialStoreTexCoordScale(Parameters, Local8, 10);
	MaterialFloat4 Local135 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_9, GetMaterialSharedSampler(sampler_Material_Texture2D_9,View_MaterialTextureBilinearWrapedSampler),Local8));
	MaterialFloat Local136 = MaterialStoreTexSample(Parameters, Local135, 10);
	MaterialFloat Local137 = MaterialStoreTexCoordScale(Parameters, Local12, 10);
	MaterialFloat4 Local138 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_9, GetMaterialSharedSampler(sampler_Material_Texture2D_9,View_MaterialTextureBilinearWrapedSampler),Local12));
	MaterialFloat Local139 = MaterialStoreTexSample(Parameters, Local138, 10);
	MaterialFloat3 Local140 = lerp(Local135.rgb,Local138.rgb,MaterialFloat(MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r));
	MaterialFloat Local141 = MaterialStoreTexCoordScale(Parameters, Local8, 0);
	MaterialFloat4 Local142 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_10, GetMaterialSharedSampler(sampler_Material_Texture2D_10,View_MaterialTextureBilinearWrapedSampler),Local8));
	MaterialFloat Local143 = MaterialStoreTexSample(Parameters, Local142, 0);
	MaterialFloat Local144 = MaterialStoreTexCoordScale(Parameters, Local12, 0);
	MaterialFloat4 Local145 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_10, GetMaterialSharedSampler(sampler_Material_Texture2D_10,View_MaterialTextureBilinearWrapedSampler),Local12));
	MaterialFloat Local146 = MaterialStoreTexSample(Parameters, Local145, 0);
	MaterialFloat3 Local147 = lerp(Local142.rgb,Local145.rgb,MaterialFloat(MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r));
	MaterialFloat3 Local148 = lerp(Local140,Local147,MaterialFloat(Material.ScalarExpressions[6].w));
	MaterialFloat2 Local149 = (GetWorldPosition(Parameters).rg / Material.ScalarExpressions[7].x);
	MaterialFloat2 Local150 = (GetWorldPosition(Parameters).rg / Material.ScalarExpressions[7].y);
	MaterialFloat2 Local151 = lerp(Local149,Local150,MaterialFloat(MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r));
	MaterialFloat Local152 = MaterialStoreTexCoordScale(Parameters, Local151, 7);
	MaterialFloat4 Local153 = ProcessMaterialLinearColorTextureLookup(Texture2DSample(Material_Texture2D_11, GetMaterialSharedSampler(sampler_Material_Texture2D_11,View_MaterialTextureBilinearWrapedSampler),Local151));
	MaterialFloat Local154 = MaterialStoreTexSample(Parameters, Local153, 7);
	MaterialFloat Local155 = dot(MaterialFloat4(Local153.rgb,0), Material.VectorExpressions[1]);
	MaterialFloat Local156 = (Local155.r * Material.ScalarExpressions[7].z);
	MaterialFloat Local157 = (Local156 - Local155.r);
	MaterialFloat Local158 = saturate(Local157);
	MaterialFloat3 Local159 = lerp(Local148,Local147,MaterialFloat(Local158));
	MaterialFloat Local160 = MaterialStoreTexCoordScale(Parameters, Local31, 0);
	MaterialFloat4 Local161 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_12, GetMaterialSharedSampler(sampler_Material_Texture2D_12,View_MaterialTextureBilinearWrapedSampler),Local31));
	MaterialFloat Local162 = MaterialStoreTexSample(Parameters, Local161, 0);
	MaterialFloat Local163 = MaterialStoreTexCoordScale(Parameters, Local35, 0);
	MaterialFloat4 Local164 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_12, GetMaterialSharedSampler(sampler_Material_Texture2D_12,View_MaterialTextureBilinearWrapedSampler),Local35));
	MaterialFloat Local165 = MaterialStoreTexSample(Parameters, Local164, 0);
	MaterialFloat3 Local166 = lerp(Local161.rgb,Local164.rgb,MaterialFloat(MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r));
	MaterialFloat Local167 = (WorldNormalCopy.b * Material.ScalarExpressions[2].y);
	MaterialFloat Local168 = PositiveClampedPow(Local167,Material.ScalarExpressions[2].z);
	MaterialFloat Local169 = min(max(Local168,0.00000000),1.00000000);
	MaterialFloat Local170 = (1.00000000 - Local169);
	MaterialFloat Local171 = (1.00000000 - Local170);
	MaterialFloat Local172 = (Local171 * 2.00000000);
	MaterialFloat Local173 = (Local172 * Local55);
	MaterialFloat Local174 = (1.00000000 - Local173);
	MaterialFloat Local175 = (Local170 * 2.00000000);
	MaterialFloat Local176 = (Local175 * Local54.r);
	MaterialFloat Local177 = ((Local170.r >= 0.50000000) ? Local174.r : Local176.r);
	MaterialFloat3 Local178 = lerp(Local159.rgb.rgb,Local166.rgb.rgb,MaterialFloat3(MaterialFloat2(Local177,Local177),Local177).r);
	MaterialFloat Local179 = MaterialStoreTexCoordScale(Parameters, Local62.rb, 0);
	MaterialFloat4 Local180 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_13, GetMaterialSharedSampler(sampler_Material_Texture2D_13,View_MaterialTextureBilinearWrapedSampler),Local62.rb));
	MaterialFloat Local181 = MaterialStoreTexSample(Parameters, Local180, 0);
	MaterialFloat Local182 = MaterialStoreTexCoordScale(Parameters, Local62.gb, 0);
	MaterialFloat4 Local183 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_13, GetMaterialSharedSampler(sampler_Material_Texture2D_13,View_MaterialTextureBilinearWrapedSampler),Local62.gb));
	MaterialFloat Local184 = MaterialStoreTexSample(Parameters, Local183, 0);
	MaterialFloat4 Local185 = lerp(Local180.rgba,Local183.rgba,MaterialFloat(Local77.r.r));
	MaterialFloat Local186 = MaterialStoreTexCoordScale(Parameters, Local83.rb, 0);
	MaterialFloat4 Local187 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_13, GetMaterialSharedSampler(sampler_Material_Texture2D_13,View_MaterialTextureBilinearWrapedSampler),Local83.rb));
	MaterialFloat Local188 = MaterialStoreTexSample(Parameters, Local187, 0);
	MaterialFloat Local189 = MaterialStoreTexCoordScale(Parameters, Local83.gb, 0);
	MaterialFloat4 Local190 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_13, GetMaterialSharedSampler(sampler_Material_Texture2D_13,View_MaterialTextureBilinearWrapedSampler),Local83.gb));
	MaterialFloat Local191 = MaterialStoreTexSample(Parameters, Local190, 0);
	MaterialFloat4 Local192 = lerp(Local187.rgba,Local190.rgba,MaterialFloat(Local77.r.r));
	MaterialFloat3 Local193 = lerp(Local185.rgba.rgb,Local192.rgba.rgb,MaterialFloat(MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r));
	MaterialFloat Local194 = (WorldNormalCopy.b * Material.ScalarExpressions[5].y);
	MaterialFloat Local195 = PositiveClampedPow(Local194,Material.ScalarExpressions[5].z);
	MaterialFloat Local196 = min(max(Local195,0.00000000),1.00000000);
	MaterialFloat Local197 = (1.00000000 - Local196);
	MaterialFloat Local198 = (1.00000000 - Local197);
	MaterialFloat Local199 = (Local198 * 2.00000000);
	MaterialFloat Local200 = (Local199 * Local55);
	MaterialFloat Local201 = (1.00000000 - Local200);
	MaterialFloat Local202 = (Local197 * 2.00000000);
	MaterialFloat Local203 = (Local202 * Local54.r);
	MaterialFloat Local204 = ((Local197.r >= 0.50000000) ? Local201.r : Local203.r);
	MaterialFloat3 Local205 = lerp(Local178.rgb,Local193.rgb.rgb,MaterialFloat3(MaterialFloat2(Local204,Local204),Local204).r);
	MaterialFloat3 Local206 = lerp(Local126.rgb,Local129.rgb,MaterialFloat(MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r));
	MaterialFloat3 Local207 = CustomExpression2(Parameters,Local2.r,Local4.r,Local6.r,Local205,Local166.rgb,Local206.rgb,0.00000000,Local131.rrr.r);
	MaterialFloat Local208 = MaterialStoreTexCoordScale(Parameters, Local8, 6);
	MaterialFloat4 Local209 = ProcessMaterialLinearColorTextureLookup(Texture2DSample(Material_Texture2D_14, GetMaterialSharedSampler(sampler_Material_Texture2D_14,View_MaterialTextureBilinearWrapedSampler),Local8));
	MaterialFloat Local210 = MaterialStoreTexSample(Parameters, Local209, 6);
	MaterialFloat Local211 = MaterialStoreTexCoordScale(Parameters, Local12, 6);
	MaterialFloat4 Local212 = ProcessMaterialLinearColorTextureLookup(Texture2DSample(Material_Texture2D_14, GetMaterialSharedSampler(sampler_Material_Texture2D_14,View_MaterialTextureBilinearWrapedSampler),Local12));
	MaterialFloat Local213 = MaterialStoreTexSample(Parameters, Local212, 6);
	MaterialFloat Local214 = (Local212.g * 2.00000000);
	MaterialFloat Local215 = lerp(Local209.g,Local214,MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r);
	MaterialFloat Local216 = (Local215.r * Material.ScalarExpressions[7].w);
	MaterialFloat Local217 = MaterialStoreTexCoordScale(Parameters, Local31, 6);
	MaterialFloat4 Local218 = ProcessMaterialLinearColorTextureLookup(Texture2DSample(Material_Texture2D_15, GetMaterialSharedSampler(sampler_Material_Texture2D_15,View_MaterialTextureBilinearWrapedSampler),Local31));
	MaterialFloat Local219 = MaterialStoreTexSample(Parameters, Local218, 6);
	MaterialFloat Local220 = MaterialStoreTexCoordScale(Parameters, Local35, 6);
	MaterialFloat4 Local221 = ProcessMaterialLinearColorTextureLookup(Texture2DSample(Material_Texture2D_15, GetMaterialSharedSampler(sampler_Material_Texture2D_15,View_MaterialTextureBilinearWrapedSampler),Local35));
	MaterialFloat Local222 = MaterialStoreTexSample(Parameters, Local221, 6);
	MaterialFloat Local223 = (Local221.g * 2.00000000);
	MaterialFloat Local224 = lerp(Local218.g,Local223,MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r);
	MaterialFloat Local225 = (Local224.r * Material.ScalarExpressions[8].x);
	MaterialFloat3 Local226 = lerp(Local216.rrr,Local225.rrr,MaterialFloat3(MaterialFloat2(Local177,Local177),Local177).r);
	MaterialFloat Local227 = MaterialStoreTexCoordScale(Parameters, Local62.rb, 6);
	MaterialFloat4 Local228 = ProcessMaterialLinearColorTextureLookup(Texture2DSample(Material_Texture2D_16, GetMaterialSharedSampler(sampler_Material_Texture2D_16,View_MaterialTextureBilinearWrapedSampler),Local62.rb));
	MaterialFloat Local229 = MaterialStoreTexSample(Parameters, Local228, 6);
	MaterialFloat Local230 = MaterialStoreTexCoordScale(Parameters, Local62.gb, 6);
	MaterialFloat4 Local231 = ProcessMaterialLinearColorTextureLookup(Texture2DSample(Material_Texture2D_16, GetMaterialSharedSampler(sampler_Material_Texture2D_16,View_MaterialTextureBilinearWrapedSampler),Local62.gb));
	MaterialFloat Local232 = MaterialStoreTexSample(Parameters, Local231, 6);
	MaterialFloat4 Local233 = lerp(Local228.rgba,Local231.rgba,MaterialFloat(Local77.r.r));
	MaterialFloat Local234 = MaterialStoreTexCoordScale(Parameters, Local83.rb, 6);
	MaterialFloat4 Local235 = ProcessMaterialLinearColorTextureLookup(Texture2DSample(Material_Texture2D_16, GetMaterialSharedSampler(sampler_Material_Texture2D_16,View_MaterialTextureBilinearWrapedSampler),Local83.rb));
	MaterialFloat Local236 = MaterialStoreTexSample(Parameters, Local235, 6);
	MaterialFloat Local237 = MaterialStoreTexCoordScale(Parameters, Local83.gb, 6);
	MaterialFloat4 Local238 = ProcessMaterialLinearColorTextureLookup(Texture2DSample(Material_Texture2D_16, GetMaterialSharedSampler(sampler_Material_Texture2D_16,View_MaterialTextureBilinearWrapedSampler),Local83.gb));
	MaterialFloat Local239 = MaterialStoreTexSample(Parameters, Local238, 6);
	MaterialFloat4 Local240 = lerp(Local235.rgba,Local238.rgba,MaterialFloat(Local77.r.r));
	MaterialFloat Local241 = (Local240.g * 2.00000000);
	MaterialFloat Local242 = lerp(Local233.g,Local241,MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r);
	MaterialFloat Local243 = (Local242.r * Material.ScalarExpressions[8].y);
	MaterialFloat3 Local244 = lerp(Local226.rgb,Local243.rrr,MaterialFloat3(MaterialFloat2(Local204,Local204),Local204).r);
	MaterialFloat Local245 = MaterialStoreTexCoordScale(Parameters, Local115, 6);
	MaterialFloat4 Local246 = ProcessMaterialLinearColorTextureLookup(Texture2DSample(Material_Texture2D_17, GetMaterialSharedSampler(sampler_Material_Texture2D_17,View_MaterialTextureBilinearWrapedSampler),Local115));
	MaterialFloat Local247 = MaterialStoreTexSample(Parameters, Local246, 6);
	MaterialFloat Local248 = MaterialStoreTexCoordScale(Parameters, Local119, 6);
	MaterialFloat4 Local249 = ProcessMaterialLinearColorTextureLookup(Texture2DSample(Material_Texture2D_17, GetMaterialSharedSampler(sampler_Material_Texture2D_17,View_MaterialTextureBilinearWrapedSampler),Local119));
	MaterialFloat Local250 = MaterialStoreTexSample(Parameters, Local249, 6);
	MaterialFloat Local251 = (Local249.g * 2.00000000);
	MaterialFloat Local252 = lerp(Local246.g,Local251,MaterialFloat4(MaterialFloat3(MaterialFloat2(Local28.r,Local28.r),Local28.r),Local28.r).r);
	MaterialFloat Local253 = (Local252.r * Material.ScalarExpressions[8].z);
	MaterialFloat3 Local254 = CustomExpression3(Parameters,Local2.r,Local4.r,Local6.r,Local244,Local225,Local253,0.00000000,Local131.rrr.r);

	PixelMaterialInputs.EmissiveColor = Local133;
	PixelMaterialInputs.Opacity = 1.00000000;
	PixelMaterialInputs.OpacityMask = 1.00000000;
	PixelMaterialInputs.BaseColor = Local207.rgb;
	PixelMaterialInputs.Metallic = 0.00000000;
	PixelMaterialInputs.Specular = 0.50000000;
	PixelMaterialInputs.Roughness = Local254.r;
	PixelMaterialInputs.Anisotropy = 0.00000000;
	PixelMaterialInputs.Tangent = MaterialFloat3(1.00000000,0.00000000,0.00000000);
	PixelMaterialInputs.Subsurface = 0;
	PixelMaterialInputs.AmbientOcclusion = 1.00000000;
	PixelMaterialInputs.Refraction = 0;
	PixelMaterialInputs.PixelDepthOffset = 0.00000000;
	PixelMaterialInputs.ShadingModel = 1;


#if MATERIAL_USES_ANISOTROPY
	Parameters.WorldTangent = CalculateAnisotropyTangent(Parameters, PixelMaterialInputs);
#else
	Parameters.WorldTangent = 0;
#endif
}

#define UnityObjectToWorldDir TransformObjectToWorld

void SetupCommonData( int Parameters_PrimitiveId )
{
	View_MaterialTextureBilinearWrapedSampler = SamplerState_Linear_Repeat;
	View_MaterialTextureBilinearClampedSampler = SamplerState_Linear_Clamp;

	Material_Wrap_WorldGroupSettings = SamplerState_Linear_Repeat;
	Material_Clamp_WorldGroupSettings = SamplerState_Linear_Clamp;

	View.GameTime = View.RealTime = _Time.y;// _Time is (t/20, t, t*2, t*3)
	View.PrevFrameGameTime = View.GameTime - unity_DeltaTime.x;//(dt, 1/dt, smoothDt, 1/smoothDt)
	View.PrevFrameRealTime = View.RealTime;
	View.DeltaTime = unity_DeltaTime.x;
	View.MaterialTextureMipBias = 0.0;
	View.TemporalAAParams = float4( 0, 0, 0, 0 );
	View.ViewRectMin = float2( 0, 0 );
	View.ViewSizeAndInvSize = View_BufferSizeAndInvSize;
	View.MaterialTextureDerivativeMultiply = 1.0f;
	View.StateFrameIndexMod8 = 0;
	View.FrameNumber = (int)_Time.y;
	View.FieldOfViewWideAngles = float2( PI * 0.42f, PI * 0.42f );//75degrees, default unity
	View.RuntimeVirtualTextureMipLevel = float4( 0, 0, 0, 0 );
	View.PreExposure = 0;
	View.BufferBilinearUVMinMax = float4(
		View_BufferSizeAndInvSize.z * ( 0 + 0.5 ),//EffectiveViewRect.Min.X
		View_BufferSizeAndInvSize.w * ( 0 + 0.5 ),//EffectiveViewRect.Min.Y
		View_BufferSizeAndInvSize.z * ( View_BufferSizeAndInvSize.x - 0.5 ),//EffectiveViewRect.Max.X
		View_BufferSizeAndInvSize.w * ( View_BufferSizeAndInvSize.y - 0.5 ) );//EffectiveViewRect.Max.Y

	for( int i2 = 0; i2 < 40; i2++ )
		View.PrimitiveSceneData[ i2 ] = float4( 0, 0, 0, 0 );

	float4x4 LocalToWorld = transpose( UNITY_MATRIX_M );
	float4x4 WorldToLocal = transpose( UNITY_MATRIX_I_M );
	float4x4 ViewMatrix = transpose( UNITY_MATRIX_V );
	float4x4 InverseViewMatrix = transpose( UNITY_MATRIX_I_V );
	float4x4 ViewProjectionMatrix = transpose( UNITY_MATRIX_VP );
	uint PrimitiveBaseOffset = Parameters_PrimitiveId * PRIMITIVE_SCENE_DATA_STRIDE;
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 0 ] = LocalToWorld[ 0 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 1 ] = LocalToWorld[ 1 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 2 ] = LocalToWorld[ 2 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 3 ] = LocalToWorld[ 3 ];//LocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 5 ] = float4( ToUnrealPos( SHADERGRAPH_OBJECT_POSITION ), 100.0 );//ObjectWorldPosition
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 6 ] = WorldToLocal[ 0 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 7 ] = WorldToLocal[ 1 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 8 ] = WorldToLocal[ 2 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 9 ] = WorldToLocal[ 3 ];//WorldToLocal
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 10 ] = LocalToWorld[ 0 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 11 ] = LocalToWorld[ 1 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 12 ] = LocalToWorld[ 2 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 13 ] = LocalToWorld[ 3 ];//PreviousLocalToWorld
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 18 ] = float4( ToUnrealPos( SHADERGRAPH_OBJECT_POSITION ), 0 );//ActorWorldPosition
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 19 ] = LocalObjectBoundsMax - LocalObjectBoundsMin;//ObjectBounds
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 21 ] = mul( LocalToWorld, float3( 1, 0, 0 ) );
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 23 ] = LocalObjectBoundsMin;//LocalObjectBoundsMin 
	View.PrimitiveSceneData[ PrimitiveBaseOffset + 24 ] = LocalObjectBoundsMax;//LocalObjectBoundsMax

#ifdef UE5
	ResolvedView.WorldCameraOrigin = LWCPromote( ToUnrealPos( _WorldSpaceCameraPos.xyz ) );
	ResolvedView.PreViewTranslation = LWCPromote( float3( 0, 0, 0 ) );
	ResolvedView.WorldViewOrigin = LWCPromote( float3( 0, 0, 0 ) );
#else
	ResolvedView.WorldCameraOrigin = ToUnrealPos( _WorldSpaceCameraPos.xyz );
	ResolvedView.PreViewTranslation = float3( 0, 0, 0 );
	ResolvedView.WorldViewOrigin = float3( 0, 0, 0 );
#endif
	ResolvedView.PrevWorldCameraOrigin = ResolvedView.WorldCameraOrigin;
	ResolvedView.ScreenPositionScaleBias = float4( 1, 1, 0, 0 );
	ResolvedView.TranslatedWorldToView		 = ViewMatrix;
	ResolvedView.TranslatedWorldToCameraView = ViewMatrix;
	ResolvedView.TranslatedWorldToClip		 = ViewProjectionMatrix;
	ResolvedView.ViewToTranslatedWorld		 = InverseViewMatrix;
	ResolvedView.PrevViewToTranslatedWorld = ResolvedView.ViewToTranslatedWorld;
	ResolvedView.CameraViewToTranslatedWorld = InverseViewMatrix;
	ResolvedView.BufferBilinearUVMinMax = View.BufferBilinearUVMinMax;
	Primitive.WorldToLocal = WorldToLocal;
	Primitive.LocalToWorld = LocalToWorld;
}
float3 PrepareAndGetWPO( float4 VertexColor, float3 UnrealWorldPos, float3 UnrealNormal, float4 InTangent,
						 float4 UV0, float4 UV1 )
{
	InitializeExpressions();
	FMaterialVertexParameters Parameters = (FMaterialVertexParameters)0;

	float3 InWorldNormal = UnrealNormal;
	float4 tangentWorld = InTangent;
	tangentWorld.xyz = normalize( tangentWorld.xyz );
	//float3x3 tangentToWorld = CreateTangentToWorldPerVertex( InWorldNormal, tangentWorld.xyz, tangentWorld.w );
	Parameters.TangentToWorld = float3x3( normalize( cross( InWorldNormal, tangentWorld.xyz ) * tangentWorld.w ), tangentWorld.xyz, InWorldNormal );

	
	UnrealWorldPos = ToUnrealPos( UnrealWorldPos );
	Parameters.WorldPosition = UnrealWorldPos;
	Parameters.TangentToWorld[ 0 ] = Parameters.TangentToWorld[ 0 ].xzy;
	Parameters.TangentToWorld[ 1 ] = Parameters.TangentToWorld[ 1 ].xzy;
	Parameters.TangentToWorld[ 2 ] = Parameters.TangentToWorld[ 2 ].xzy;//WorldAligned texturing uses normals that think Z is up

	Parameters.VertexColor = VertexColor;

#if NUM_MATERIAL_TEXCOORDS_VERTEX > 0			
	Parameters.TexCoords[ 0 ] = float2( UV0.x, UV0.y );
#endif
#if NUM_MATERIAL_TEXCOORDS_VERTEX > 1
	Parameters.TexCoords[ 1 ] = float2( UV1.x, UV1.y );
#endif
#if NUM_MATERIAL_TEXCOORDS_VERTEX > 2
	for( int i = 2; i < NUM_TEX_COORD_INTERPOLATORS; i++ )
	{
		Parameters.TexCoords[ i ] = float2( UV0.x, UV0.y );
	}
#endif

	Parameters.PrimitiveId = 0;

	SetupCommonData( Parameters.PrimitiveId );

#ifdef UE5
	Parameters.PrevFrameLocalToWorld = MakeLWCMatrix( float3( 0, 0, 0 ), Primitive.LocalToWorld );
#else
	Parameters.PrevFrameLocalToWorld = Primitive.LocalToWorld;
#endif
	
	float3 Offset = float3( 0, 0, 0 );
	Offset = GetMaterialWorldPositionOffset( Parameters );
	//Convert from unreal units to unity
	Offset /= float3( 100, 100, 100 );
	Offset = Offset.xzy;
	return Offset;
}

void SurfaceReplacement( Input In, out SurfaceOutputStandard o )
{
	InitializeExpressions();

	float3 Z3 = float3( 0, 0, 0 );
	float4 Z4 = float4( 0, 0, 0, 0 );

	float3 UnrealWorldPos = float3( In.worldPos.x, In.worldPos.y, In.worldPos.z );

	float3 UnrealNormal = In.normal2;	

	FMaterialPixelParameters Parameters = (FMaterialPixelParameters)0;
#if NUM_TEX_COORD_INTERPOLATORS > 0			
	Parameters.TexCoords[ 0 ] = float2( In.uv_MainTex.x, 1.0 - In.uv_MainTex.y );
#endif
#if NUM_TEX_COORD_INTERPOLATORS > 1
	Parameters.TexCoords[ 1 ] = float2( In.uv2_Material_Texture2D_0.x, 1.0 - In.uv2_Material_Texture2D_0.y );
#endif
#if NUM_TEX_COORD_INTERPOLATORS > 2
	for( int i = 2; i < NUM_TEX_COORD_INTERPOLATORS; i++ )
	{
		Parameters.TexCoords[ i ] = float2( In.uv_MainTex.x, 1.0 - In.uv_MainTex.y );
	}
#endif
	Parameters.VertexColor = In.color;
	Parameters.WorldNormal = UnrealNormal;
	Parameters.ReflectionVector = half3( 0, 0, 1 );
	Parameters.CameraVector = normalize( _WorldSpaceCameraPos.xyz - UnrealWorldPos.xyz );
	//Parameters.CameraVector = mul( ( float3x3 )unity_CameraToWorld, float3( 0, 0, 1 ) ) * -1;
	Parameters.LightVector = half3( 0, 0, 0 );
	//float4 screenpos = In.screenPos;
	//screenpos /= screenpos.w;
	Parameters.SvPosition = In.screenPos;
	Parameters.ScreenPosition = Parameters.SvPosition;

	Parameters.UnMirrored = 1;

	Parameters.TwoSidedSign = 1;


	float3 InWorldNormal = UnrealNormal;	
	float4 tangentWorld = In.tangent;
	tangentWorld.xyz = normalize( tangentWorld.xyz );
	//float3x3 tangentToWorld = CreateTangentToWorldPerVertex( InWorldNormal, tangentWorld.xyz, tangentWorld.w );
	Parameters.TangentToWorld = float3x3( normalize( cross( InWorldNormal, tangentWorld.xyz ) * tangentWorld.w ), tangentWorld.xyz, InWorldNormal );

	//WorldAlignedTexturing in UE relies on the fact that coords there are 100x larger, prepare values for that
	//but watch out for any computation that might get skewed as a side effect
	UnrealWorldPos = ToUnrealPos( UnrealWorldPos );
	
	Parameters.AbsoluteWorldPosition = UnrealWorldPos;
	Parameters.WorldPosition_CamRelative = UnrealWorldPos;
	Parameters.WorldPosition_NoOffsets = UnrealWorldPos;

	Parameters.WorldPosition_NoOffsets_CamRelative = Parameters.WorldPosition_CamRelative;
	Parameters.LightingPositionOffset = float3( 0, 0, 0 );

	Parameters.AOMaterialMask = 0;

	Parameters.Particle.RelativeTime = 0;
	Parameters.Particle.MotionBlurFade;
	Parameters.Particle.Random = 0;
	Parameters.Particle.Velocity = half4( 1, 1, 1, 1 );
	Parameters.Particle.Color = half4( 1, 1, 1, 1 );
	Parameters.Particle.TranslatedWorldPositionAndSize = float4( UnrealWorldPos, 0 );
	Parameters.Particle.MacroUV = half4( 0, 0, 1, 1 );
	Parameters.Particle.DynamicParameter = half4( 0, 0, 0, 0 );
	Parameters.Particle.LocalToWorld = float4x4( Z4, Z4, Z4, Z4 );
	Parameters.Particle.Size = float2( 1, 1 );
	Parameters.Particle.SubUVCoords[ 0 ] = Parameters.Particle.SubUVCoords[ 1 ] = float2( 0, 0 );
	Parameters.Particle.SubUVLerp = 0.0;
	Parameters.TexCoordScalesParams = float2( 0, 0 );
	Parameters.PrimitiveId = 0;
	Parameters.VirtualTextureFeedback = 0;

	FPixelMaterialInputs PixelMaterialInputs = (FPixelMaterialInputs)0;
	PixelMaterialInputs.Normal = float3( 0, 0, 1 );
	PixelMaterialInputs.ShadingModel = 0;
	PixelMaterialInputs.FrontMaterial = 0;

	SetupCommonData( Parameters.PrimitiveId );
	//CustomizedUVs
	#if NUM_TEX_COORD_INTERPOLATORS > 0 && HAS_CUSTOMIZED_UVS
		float2 OutTexCoords[ NUM_TEX_COORD_INTERPOLATORS ];
		GetMaterialCustomizedUVs( Parameters, OutTexCoords );
		for( int i = 0; i < NUM_TEX_COORD_INTERPOLATORS; i++ )
		{
			Parameters.TexCoords[ i ] = OutTexCoords[ i ];
		}
	#endif
	//<-
	CalcPixelMaterialInputs( Parameters, PixelMaterialInputs );

	#define HAS_WORLDSPACE_NORMAL 0
	#if HAS_WORLDSPACE_NORMAL
		PixelMaterialInputs.Normal = mul( PixelMaterialInputs.Normal, (MaterialFloat3x3)( transpose( Parameters.TangentToWorld ) ) );
	#endif

	o.Albedo = PixelMaterialInputs.BaseColor.rgb;
	o.Alpha = PixelMaterialInputs.Opacity;
	//if( PixelMaterialInputs.OpacityMask < 0.333 ) discard;

	o.Metallic = PixelMaterialInputs.Metallic;
	o.Smoothness = 1.0 - PixelMaterialInputs.Roughness;
	o.Normal = normalize( PixelMaterialInputs.Normal );
	o.Emission = PixelMaterialInputs.EmissiveColor.rgb;
	o.Occlusion = PixelMaterialInputs.AmbientOcclusion;

	//BLEND_ADDITIVE o.Alpha = ( o.Emission.r + o.Emission.g + o.Emission.b ) / 3;
}