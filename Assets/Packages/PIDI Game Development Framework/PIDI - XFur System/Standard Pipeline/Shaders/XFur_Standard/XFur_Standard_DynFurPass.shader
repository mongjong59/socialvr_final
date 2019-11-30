/*
XFur Studio™ v 1.2

You cannot sell, redistribute, share nor make public this code, even modified, through any means on any platform.
Modifications are allowed only for your own use and to make this product suit better your project's needs.
These modifications may not be redistributed, sold or shared in any way.

For more information, contact us at contact@irreverent-software.com

Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved.
*/

Shader "PIDI Shaders Collection/XFur System/Full Shadows/Fur Pass" {
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
		[PerRendererData]_FurData0("Fur Data Map 0", 2D ) = "white"{}
		[PerRendererData]_FurData1("Fur Data Map 1", 2D ) = "white"{}
		[PerRendererData]_FurStep("Fur Step", float) = 24

		//Internal
		[PerRendererData]_LocalWindStrength("Local Wind Strength", Range(0,64)) = 1
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
		[PerRendererData]_RimColor("Rim Color", Color ) = (0.2,0.2,0.2,1)
		[PerRendererData]_FurRimStrength("Fur Rim Strength", Range(0,1) ) = 0.6
		[PerRendererData]_FurDirection("Fur Direction", Vector) = (0.2,0.2,0.2,0.2 )
		_Cull("Culling", Float ) = 2
	}
	SubShader {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		
		LOD 200
		ZWrite On
		Cull [_Cull]
	

	CGPROGRAM
	#include "../CGIncludes/XFur_StandardCG.cginc"
	#pragma surface surf AnisoRendering vertex:vert nolightmap novertexlights addshadow
	#pragma shader_feature ANISOTROPIC_ON
	#pragma shader_feature XFUR_PAINTER
	#pragma target 3.0

	void vert ( inout appdata_full v, out Input o )  {
		UNITY_INITIALIZE_OUTPUT(Input,o);
		o.vertexPos = v.texcoord2; o.vertexNormal = v.texcoord3;
		o.samplePass = v.texcoord1.z;
		FurDynVertexPass( v, v.vertex, v.color );
	}

	void surf (Input IN, inout SurfaceOutputFurRendering o) {
		FurUnderCoat( IN, o, (int)(IN.samplePass*24) );
	}
	ENDCG

	}
	CustomEditor "XFur_ShaderEditor"
	Fallback "Standard"
}
