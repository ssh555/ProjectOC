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
	float4 VectorExpressions[8];
	float4 ScalarExpressions[7];
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

#include "UnrealCommon.cginc"

static MaterialStruct Material;
void InitializeExpressions()
{
	Material.VectorExpressions[0] = float4(0.000000,0.000000,0.000000,0.000000);//SelectionColor
	Material.VectorExpressions[1] = float4(0.000000,0.000000,0.000000,0.000000);//(Unknown)
	Material.VectorExpressions[2] = float4(1.000000,1.000000,1.000000,1.000000);//Tint
	Material.VectorExpressions[3] = float4(1.000000,1.000000,1.000000,0.000000);//(Unknown)
	Material.VectorExpressions[4] = float4(1.000000,1.000000,1.000000,1.000000);//Cover - Tint
	Material.VectorExpressions[5] = float4(1.000000,1.000000,1.000000,0.000000);//(Unknown)
	Material.VectorExpressions[6] = float4(1.000000,0.612076,0.521967,1.000000);//Cover - Tint Color Var
	Material.VectorExpressions[7] = float4(1.000000,0.612076,0.521967,0.000000);//(Unknown)
	Material.ScalarExpressions[0] = float4(1.000000,1.000000,0.000000,3.000000);//Tiling Normal Strength (Unknown) Normal Detail Tiling
	Material.ScalarExpressions[1] = float4(0.650000,0.350000,1.000000,1.000000);//Normal Detail Strength (Unknown) Cover - Blend Normal Cover - World Blend Max
	Material.ScalarExpressions[2] = float4(1.000000,0.000000,1.150000,0.000000);//Cover - Opacity (Unknown) Brightness Saturation
	Material.ScalarExpressions[3] = float4(256.000000,256.000000,-256.000000,1.250000);//Cover - Grass - Tiling (Unknown) (Unknown) Cover - Brightness
	Material.ScalarExpressions[4] = float4(0.000000,512.000000,512.000000,-512.000000);//Cover - Saturation Cover - Grass - Noise Tiling (Unknown) (Unknown)
	Material.ScalarExpressions[5] = float4(1.000000,1.000000,1.000000,1.250000);//Roughness Power Roughness Multiply Cover - Grass - Roughness Power Cover - Grass - Roughness Multiply
	Material.ScalarExpressions[6] = float4(0.500000,3.000000,0.000000,0.000000);//Dither Random Pixel Depth Offset (Unknown) (Unknown)
}
MaterialFloat CustomExpression0(FMaterialPixelParameters Parameters,MaterialFloat2 p)
{
return Mod( ((uint)(p.x) + 2 * (uint)(p.y)) , 5 );
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
	return MaterialFloat3(0.00000000,0.00000000,0.00000000).rgb.rgb;;
}
void CalcPixelMaterialInputs(in out FMaterialPixelParameters Parameters, in out FPixelMaterialInputs PixelMaterialInputs)
{
	//WorldAligned texturing & others use normals & stuff that think Z is up
	Parameters.TangentToWorld[0] = Parameters.TangentToWorld[0].xzy;
	Parameters.TangentToWorld[1] = Parameters.TangentToWorld[1].xzy;
	Parameters.TangentToWorld[2] = Parameters.TangentToWorld[2].xzy;

	float3 WorldNormalCopy = Parameters.WorldNormal;

	// Initial calculations (required for Normal)
	MaterialFloat2 Local0 = (Material.ScalarExpressions[0].x * Parameters.TexCoords[0].xy);
	MaterialFloat Local1 = MaterialStoreTexCoordScale(Parameters, Local0, 1);
	MaterialFloat4 Local2 = UnpackNormalMap(Texture2DSampleBias(Material_Texture2D_0, sampler_Material_Texture2D_0,Local0,View.MaterialTextureMipBias));
	MaterialFloat Local3 = MaterialStoreTexSample(Parameters, Local2, 1);
	MaterialFloat3 Local4 = lerp(Local2.rgb.rgb,MaterialFloat3(0.00000000,0.00000000,1.00000000),MaterialFloat(Material.ScalarExpressions[0].z));
	MaterialFloat Local5 = (Local4.rgb.b + 1.00000000);
	MaterialFloat2 Local6 = (Material.ScalarExpressions[0].w * Parameters.TexCoords[0].xy);
	MaterialFloat Local7 = MaterialStoreTexCoordScale(Parameters, Local6, 1);
	MaterialFloat4 Local8 = UnpackNormalMap(Texture2DSampleBias(Material_Texture2D_1, sampler_Material_Texture2D_1,Local6,View.MaterialTextureMipBias));
	MaterialFloat Local9 = MaterialStoreTexSample(Parameters, Local8, 1);
	MaterialFloat3 Local10 = lerp(Local8.rgb,MaterialFloat3(0.00000000,0.00000000,1.00000000),MaterialFloat(Material.ScalarExpressions[1].y));
	MaterialFloat2 Local11 = (Local10.rg * -1.00000000);
	MaterialFloat Local12 = dot(MaterialFloat3(Local4.rgb.rg,Local5), MaterialFloat3(Local11,Local10.b));
	MaterialFloat3 Local13 = (MaterialFloat3(Local4.rgb.rg,Local5) * Local12);
	MaterialFloat3 Local14 = (Local5 * MaterialFloat3(Local11,Local10.b));
	MaterialFloat3 Local15 = (Local13 - Local14);
	MaterialFloat3 Local16 = lerp(MaterialFloat3(0.00000000,0.00000000,1.00000000),Local15,MaterialFloat(Material.ScalarExpressions[1].z));
	MaterialFloat3 Local17 = mul(Local16, (MaterialFloat3x3)(Parameters.TangentToWorld));
	MaterialFloat Local18 = dot(Local17, Local17);
	MaterialFloat Local19 = sqrt(Local18);
	MaterialFloat3 Local20 = (Local17 / Local19);
	MaterialFloat Local21 = dot(Local20, MaterialFloat3(0.00000000,0.00000000,1.00000000));
	MaterialFloat Local22 = (1.00000000 + Local21);
	MaterialFloat Local23 = (Local22 * 0.50000000);
	MaterialFloat Local24 = lerp(-4.00000000,Material.ScalarExpressions[1].w,Local23);
	MaterialFloat Local25 = min(max(Local24,0.00000000),1.00000000);
	MaterialFloat Local26 = lerp(0.00000000,Local25,Parameters.VertexColor.a);
	MaterialFloat Local27 = (Local26 * Material.ScalarExpressions[2].x);
	MaterialFloat3 Local28 = lerp(Local15.rgb,MaterialFloat3(0.00000000,0.00000000,1.00000000).rgb,MaterialFloat(Local27));

	// The Normal is a special case as it might have its own expressions and also be used to calculate other inputs, so perform the assignment here
	PixelMaterialInputs.Normal = Local28.rgb;


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
	MaterialFloat3 Local29 = lerp(MaterialFloat3(0.00000000,0.00000000,0.00000000).rgb.rgb,Material.VectorExpressions[1].rgb,MaterialFloat(Material.ScalarExpressions[2].y));
	MaterialFloat Local30 = MaterialStoreTexCoordScale(Parameters, Local0, 0);
	MaterialFloat4 Local31 = ProcessMaterialColorTextureLookup(Texture2DSampleBias(Material_Texture2D_2, sampler_Material_Texture2D_2,Local0,View.MaterialTextureMipBias));
	MaterialFloat Local32 = MaterialStoreTexSample(Parameters, Local31, 0);
	MaterialFloat3 Local33 = (Material.ScalarExpressions[2].z * Local31.rgb.rgb);
	MaterialFloat3 Local34 = (Local33 * Material.VectorExpressions[3].rgb);
	MaterialFloat Local35 = dot(Local34, MaterialFloat3(0.30000001,0.58999997,0.11000000));
	MaterialFloat3 Local36 = lerp(Local34,MaterialFloat3(Local35,Local35,Local35),MaterialFloat(Material.ScalarExpressions[2].w));
	MaterialFloat3 Local37 = saturate(Local36);
	MaterialFloat3 Local38 = (GetWorldPosition(Parameters) / Material.ScalarExpressions[3].z);
	MaterialFloat Local39 = MaterialStoreTexCoordScale(Parameters, Local38.rb, 0);
	MaterialFloat4 Local40 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_3, GetMaterialSharedSampler(sampler_Material_Texture2D_3,View_MaterialTextureBilinearWrapedSampler),Local38.rb));
	MaterialFloat Local41 = MaterialStoreTexSample(Parameters, Local40, 0);
	MaterialFloat Local42 = MaterialStoreTexCoordScale(Parameters, Local38.gb, 0);
	MaterialFloat4 Local43 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_3, GetMaterialSharedSampler(sampler_Material_Texture2D_3,View_MaterialTextureBilinearWrapedSampler),Local38.gb));
	MaterialFloat Local44 = MaterialStoreTexSample(Parameters, Local43, 0);
	MaterialFloat Local45 = abs(Parameters.TangentToWorld[2].r);
	MaterialFloat Local46 = lerp((0.00000000 - 1.00000000),(1.00000000 + 1.00000000),Local45);
	MaterialFloat Local47 = min(max(Local46,0.00000000),1.00000000);
	MaterialFloat3 Local48 = lerp(Local40.rgb,Local43.rgb,MaterialFloat(Local47.r.r));
	MaterialFloat Local49 = MaterialStoreTexCoordScale(Parameters, Local38.rg, 0);
	MaterialFloat4 Local50 = ProcessMaterialColorTextureLookup(Texture2DSample(Material_Texture2D_3, GetMaterialSharedSampler(sampler_Material_Texture2D_3,View_MaterialTextureBilinearWrapedSampler),Local38.rg));
	MaterialFloat Local51 = MaterialStoreTexSample(Parameters, Local50, 0);
	MaterialFloat Local52 = abs(Parameters.TangentToWorld[2].b);
	MaterialFloat Local53 = lerp((0.00000000 - 1.00000000),(1.00000000 + 1.00000000),Local52);
	MaterialFloat Local54 = min(max(Local53,0.00000000),1.00000000);
	MaterialFloat3 Local55 = lerp(Local48,Local50.rgb,MaterialFloat(Local54.r.r));
	MaterialFloat3 Local56 = (Local55 * Material.ScalarExpressions[3].w);
	MaterialFloat3 Local57 = (Local56 * Material.VectorExpressions[5].rgb);
	MaterialFloat Local58 = dot(Local57, MaterialFloat3(0.30000001,0.58999997,0.11000000));
	MaterialFloat3 Local59 = lerp(Local57,MaterialFloat3(Local58,Local58,Local58),MaterialFloat(Material.ScalarExpressions[4].x));
	MaterialFloat3 Local60 = (Local59 * Material.VectorExpressions[7].rgb);
	MaterialFloat3 Local61 = (GetWorldPosition(Parameters) / Material.ScalarExpressions[4].w);
	MaterialFloat Local62 = MaterialStoreTexCoordScale(Parameters, Local61.rb, 2);
	MaterialFloat4 Local63 = Texture2DSample(Material_Texture2D_4, GetMaterialSharedSampler(sampler_Material_Texture2D_4,View_MaterialTextureBilinearWrapedSampler),Local61.rb);
	MaterialFloat Local64 = MaterialStoreTexSample(Parameters, Local63, 2);
	MaterialFloat Local65 = MaterialStoreTexCoordScale(Parameters, Local61.gb, 2);
	MaterialFloat4 Local66 = Texture2DSample(Material_Texture2D_4, GetMaterialSharedSampler(sampler_Material_Texture2D_4,View_MaterialTextureBilinearWrapedSampler),Local61.gb);
	MaterialFloat Local67 = MaterialStoreTexSample(Parameters, Local66, 2);
	MaterialFloat3 Local68 = lerp(Local63.rgb,Local66.rgb,MaterialFloat(Local47.r.r));
	MaterialFloat Local69 = MaterialStoreTexCoordScale(Parameters, Local61.rg, 2);
	MaterialFloat4 Local70 = Texture2DSample(Material_Texture2D_4, GetMaterialSharedSampler(sampler_Material_Texture2D_4,View_MaterialTextureBilinearWrapedSampler),Local61.rg);
	MaterialFloat Local71 = MaterialStoreTexSample(Parameters, Local70, 2);
	MaterialFloat3 Local72 = lerp(Local68,Local70.rgb,MaterialFloat(Local54.r.r));
	MaterialFloat3 Local73 = lerp(Local59,Local60,Local72);
	MaterialFloat3 Local74 = lerp(Local37.rgb.rgb,Local73.rgb,MaterialFloat(Local27));
	MaterialFloat Local75 = MaterialStoreTexCoordScale(Parameters, Local0, 2);
	MaterialFloat4 Local76 = Texture2DSampleBias(Material_Texture2D_5, sampler_Material_Texture2D_5,Local0,View.MaterialTextureMipBias);
	MaterialFloat Local77 = MaterialStoreTexSample(Parameters, Local76, 2);
	MaterialFloat Local78 = PositiveClampedPow(Local76.r.r,Material.ScalarExpressions[5].x);
	MaterialFloat Local79 = (Local78 * Material.ScalarExpressions[5].y);
	MaterialFloat Local80 = min(max(Local79,0.00000000),1.00000000);
	MaterialFloat Local81 = MaterialStoreTexCoordScale(Parameters, Local38.rb, 2);
	MaterialFloat4 Local82 = Texture2DSample(Material_Texture2D_6, GetMaterialSharedSampler(sampler_Material_Texture2D_6,View_MaterialTextureBilinearWrapedSampler),Local38.rb);
	MaterialFloat Local83 = MaterialStoreTexSample(Parameters, Local82, 2);
	MaterialFloat Local84 = MaterialStoreTexCoordScale(Parameters, Local38.gb, 2);
	MaterialFloat4 Local85 = Texture2DSample(Material_Texture2D_6, GetMaterialSharedSampler(sampler_Material_Texture2D_6,View_MaterialTextureBilinearWrapedSampler),Local38.gb);
	MaterialFloat Local86 = MaterialStoreTexSample(Parameters, Local85, 2);
	MaterialFloat3 Local87 = lerp(Local82.rgb,Local85.rgb,MaterialFloat(Local47.r.r));
	MaterialFloat Local88 = MaterialStoreTexCoordScale(Parameters, Local38.rg, 2);
	MaterialFloat4 Local89 = Texture2DSample(Material_Texture2D_6, GetMaterialSharedSampler(sampler_Material_Texture2D_6,View_MaterialTextureBilinearWrapedSampler),Local38.rg);
	MaterialFloat Local90 = MaterialStoreTexSample(Parameters, Local89, 2);
	MaterialFloat3 Local91 = lerp(Local87,Local89.rgb,MaterialFloat(Local54.r.r));
	MaterialFloat Local92 = PositiveClampedPow(Local91.r,Material.ScalarExpressions[5].z);
	MaterialFloat Local93 = (Local92 * Material.ScalarExpressions[5].w);
	MaterialFloat Local94 = min(max(Local93,0.00000000),1.00000000);
	MaterialFloat3 Local95 = lerp(Local80.r.rrr,Local94.rrr,MaterialFloat(Local27));
	MaterialFloat3 Local96 = lerp(Local76.b.r.r.rrr,Local91.b.rrr,MaterialFloat(Local27));
	MaterialFloat2 Local97 = GetPixelPosition(Parameters);
	MaterialFloat Local98 = View.TemporalAAParams.x;
	MaterialFloat2 Local99 = (Local97 + MaterialFloat2(Local98,Local98));
	MaterialFloat Local100 = CustomExpression0(Parameters,Local99);
	MaterialFloat2 Local101 = (Local97 / MaterialFloat2(64.00000000,64.00000000));
	MaterialFloat Local102 = MaterialStoreTexCoordScale(Parameters, Local101, 6);
	MaterialFloat4 Local103 = ProcessMaterialLinearGreyscaleTextureLookup((Texture2DSampleBias(Material_Texture2D_7, sampler_Material_Texture2D_7,Local101,View.MaterialTextureMipBias)).r).rrrr;
	MaterialFloat Local104 = MaterialStoreTexSample(Parameters, Local103, 6);
	MaterialFloat Local105 = (Local103.r * Material.ScalarExpressions[6].x);
	MaterialFloat Local106 = (Local100 + Local105);
	MaterialFloat Local107 = (Local106 / 6.00000000);
	MaterialFloat Local108 = (0.50000000 + Local107);
	MaterialFloat Local109 = (Local108 + -0.50000000);
	MaterialFloat Local110 = (Local109 * Material.ScalarExpressions[6].y);

	PixelMaterialInputs.EmissiveColor = Local29;
	PixelMaterialInputs.Opacity = 1.00000000;
	PixelMaterialInputs.OpacityMask = 1.00000000;
	PixelMaterialInputs.BaseColor = Local74.rgb;
	PixelMaterialInputs.Metallic = 0.00000000.rrr.r;
	PixelMaterialInputs.Specular = 0.50000000.rrr.r;
	PixelMaterialInputs.Roughness = Local95.r;
	PixelMaterialInputs.Anisotropy = 0.00000000;
	PixelMaterialInputs.Tangent = MaterialFloat3(1.00000000,0.00000000,0.00000000);
	PixelMaterialInputs.Subsurface = 0;
	PixelMaterialInputs.AmbientOcclusion = Local96.r;
	PixelMaterialInputs.Refraction = 0;
	PixelMaterialInputs.PixelDepthOffset = Local110;
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