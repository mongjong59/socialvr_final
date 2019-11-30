 
/*
XFur Studio™ v 1.2

You cannot sell, redistribute, share nor make public this code, even modified, through any means on any platform.
Modifications are allowed only for your own use and to make this product suit better your project's needs.
These modifications may not be redistributed, sold or shared in any way.

For more information, contact us at contact@irreverent-software.com

Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved.
*/

Shader "PIDI Shaders Collection/XFur System/Standard/High Quality/Twenty Four Samples" {
	Properties {
		
		[Space(4)][Header(Base Skin Settings)][Space(4)]
		[PerRendererData]_BaseColor("Main Color",Color) = (1,1,1,1)
		[PerRendererData]_BaseSpecular("Main Specular", Color ) = (0.2,0.2,0.2,1)
		[PerRendererData][NoScaleOffset]_BaseTex("Base Tex",2D) = "white"{}
		[PerRendererData][NoScaleOffset]_GlossSpecular("Specular (RGB) Smooothness (A)", 2D ) = "black"{}
		[PerRendererData]_BaseSmoothness("Base Smoothness", Range(0,1) ) = 0.3
		[PerRendererData][NoScaleOffset]_Normalmap("Normalmap", 2D) = "bump"{}
		[PerRendererData][NoScaleOffset]_OcclusionMap("Occlusion", 2D) = "white"{}

		[Space(4)][Header(Fur Settings)][Space(4)]
		[PerRendererData]_FurCutoff("Fur Cutoff", Range(0,1)) = 0.4
		[PerRendererData]_FurNoiseMap( "Fur Gen Map", 2D ) = "black"{}
		[PerRendererData]_FurLength( "Fur Length", Range(0,4) ) = 1
		[PerRendererData]_FurSmoothness("Fur Smoothness", Range(0,1)) = 0.4
		[PerRendererData]_AnisotropicOffset("Aniso Offset", Range(-1,1)) = 0.4
		[PerRendererData]_FurThin( "Fur Thinness", Range(0,1) ) = 0.5
		[PerRendererData]_FurOcclusion("Fur Occlusion", Range(0,1)) = 1
		[PerRendererData]_FurColorMap("Fur Color Map", 2D ) = "white"{}
		[PerRendererData]_FurFXNoise("Fur FX Noise", 2D ) = "white"{}
		[PerRendererData]_FurPainter("Fur Painter", 2D ) = "black"{}
		[PerRendererData]_FurSpecular("Fur Specular", Color ) = (0.3,0.3,0.3,1)
		[PerRendererData]_FurColorA( "Fur Color A", Color ) = (1,1,1,1)
		[PerRendererData]_FurColorB( "Fur Color B", Color ) = (1,1,1,1)
		[PerRendererData]_RimColor("Rim Color", Color ) = (0.2,0.2,0.2,1)
		[PerRendererData]_FurData0("Fur Data Map 0", 2D ) = "white"{}
		[PerRendererData]_FurData1("Fur Data Map 1", 2D ) = "white"{}

		//Internal
		[PerRendererData]_LocalWindStrength("Local Wind Strength", Range(0,64)) = 0
		[PerRendererData]_UV0Scale1( "UV0 Scale", Float ) = 1	
		[PerRendererData]_UV0Scale2( "UV0 Scale 2", Float ) = 2	
		[PerRendererData]_UV1Scale1( "UV1 Scale 1", Float ) = 2	
		[PerRendererData]_UV1Scale2( "UV1 Scale 2", Float ) = 3	
		[PerRendererData]_UV1Scale3( "UV1 Scale 3", Float ) = 2
		[PerRendererData]_UV1Scale4( "UV1 Scale 4", Float ) = 3
		[PerRendererData]_TriplanarScale( "Triplanar Scale", Float ) = 2
		[PerRendererData]_TriplanarBias("Triplanar Bias", Range(0,1)) = 0.85
		[PerRendererData]_TriplanarMode("Triplanar Mode", Float ) = 0
		[PerRendererData]_UV2Grooming("UV2 Grooming Mode", Float ) = 0
		[PerRendererData]_UV2Painting("UV2 Painting Mode", Float ) = 0
		[PerRendererData]_SelfCollision("Self Collision", Float ) = 0
		[PerRendererData]_HasGlossMap( "Has Gloss Map", Float ) = 0
		[PerRendererData]_FurRimStrength("Fur Rim Strength", Range(0,1) ) = 0.6
		[PerRendererData]_FurDirection("Fur Direction", Vector ) = (0.2,0.2,0.2,0.2)
		_Cull( "Cull", Float ) = 2

	}
	SubShader {

		Tags { "Queue"="AlphaTest" "RenderType"="Opaque" }
		
		LOD 500
		CGPROGRAM
		#include "../CGIncludes/XFur_MainCG.cginc"
		#pragma shader_feature ANISOTROPIC_ON
		#pragma shader_feature XFUR_PAINTER
		#pragma surface surf StandardSpecular nolightmap nometa 
		#pragma target 3.0

		

	void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			
		float2 firstUV = IN.uv_BaseTex*_UV0Scale1;
		float2 secondUV = IN.uv2_FurNoiseMap*_UV1Scale1;

		fixed4 furData0 = tex2D( _FurData0, firstUV );
		fixed4 mainColor = tex2D( _BaseTex, firstUV )*_BaseColor;
		fixed4 specColor = tex2D( _GlossSpecular, firstUV );

		o.Albedo = mainColor;
		o.Normal = UnpackNormal( tex2D ( _Normalmap, firstUV ));
		o.Specular = specColor.rgb+_BaseSpecular*(1-_HasGlossMap);
		o.Smoothness = specColor.a+_BaseSmoothness*(1-_HasGlossMap);
		o.Occlusion = tex2D( _OcclusionMap, firstUV );
		o.Emission = tex2D(_FurPainter, firstUV );
	}

	ENDCG
	
	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa
	#pragma shader_feature ANISOTROPIC_ON
	#pragma shader_feature XFUR_PAINTER   
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;		
		FurVertexPass( v, v.vertex, v.color,  1 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 1 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa 
	#pragma shader_feature ANISOTROPIC_ON 
	#pragma shader_feature XFUR_PAINTER   
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  2 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 2 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa
	#pragma shader_feature ANISOTROPIC_ON     
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  3 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 3 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON 
	#pragma shader_feature XFUR_PAINTER  
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  4 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 4 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  5 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 5 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  6 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 6 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  7 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 7 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  8 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 8 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  9 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 9 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  10 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 10 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa  
	#pragma shader_feature ANISOTROPIC_ON    
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  11 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 11 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  12 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 12 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  13 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 13 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  14 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 14 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  15 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 15 );
	}
	ENDCG

	
	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER 
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  16 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 16 );
	}
	ENDCG

	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa    
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  17 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 17 );
	}
	ENDCG

	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa    
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  18 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 18 );
	}
	ENDCG

	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER 
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  19 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 19 );
	}
	ENDCG

	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa    
	#pragma shader_feature ANISOTROPIC_ON  
	#pragma shader_feature XFUR_PAINTER 
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  20 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 20 );
	}
	ENDCG

	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa    
	#pragma shader_feature ANISOTROPIC_ON   
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  21 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 21 );
	}
	ENDCG

	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa    
	#pragma shader_feature ANISOTROPIC_ON
	#pragma shader_feature XFUR_PAINTER   
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  22 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 22 );
	}
	ENDCG

	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa    
	#pragma shader_feature ANISOTROPIC_ON
	#pragma shader_feature XFUR_PAINTER   
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  23 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 23 );
	}
	ENDCG

	ZWrite On
	Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap nometa   
	#pragma shader_feature ANISOTROPIC_ON
	#pragma shader_feature XFUR_PAINTER    
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		_FurStep = 24;
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		FurVertexPass( v, v.vertex, v.color,  24 );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		_FurStep = 24;
		FurUnderCoat( IN, o, 24 );
	}
	ENDCG

	
	}
	CustomEditor "XFur_ShaderEditor"
	Fallback "Standard"
}
