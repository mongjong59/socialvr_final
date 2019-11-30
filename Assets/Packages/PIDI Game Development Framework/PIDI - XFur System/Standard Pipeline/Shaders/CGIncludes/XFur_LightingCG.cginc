#include "UnityCG.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityStandardBRDF.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityLightingCommon.cginc"
#include "UnityGlobalIllumination.cginc"
#include "UnityPBSLighting.cginc"

#ifndef XFUR_INCLUDED
#define XFUR_INCLUDED




//AUXILIARY FUNCTIONS

//-------------------------------------------------------------------------------------
// Default BRDF to use:
#if !defined (UNITY_BRDF_PBS) // allow to explicitly override BRDF in custom shader
	#if (SHADER_TARGET < 30) || defined(SHADER_API_PSP2)
		// Fallback to low fidelity one for pre-SM3.0
		#define UNITY_BRDF_PBS BRDF3_Unity_PBS
	#elif defined(SHADER_API_MOBILE)
		// Somewhat simplified for mobile
		#define UNITY_BRDF_PBS BRDF2_Unity_PBS
	#else
		// Full quality for SM3+ PC / consoles
		#define UNITY_BRDF_PBS BRDF1_Unity_PBS
	#endif
#endif

//Additional BRDF methods
//-------------------------------------------------------------------------------------
// BRDF for lights extracted from *indirect* directional lightmaps (baked and realtime).
// Baked directional lightmap with *direct* light uses UNITY_BRDF_PBS.
// For better quality change to BRDF1_Unity_PBS.
// No directional lightmaps in SM2.0.

#if !defined(UNITY_BRDF_PBS_LIGHTMAP_INDIRECT)
	#define UNITY_BRDF_PBS_LIGHTMAP_INDIRECT BRDF2_Unity_PBS
#endif
#if !defined (UNITY_BRDF_GI)
	#define UNITY_BRDF_GI BRDF_Unity_Indirect
#endif



inline float4 PIDI_Object2ClipPos(in float3 pos)
{
    // More efficient than computing M*VP matrix product
    return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
}
inline float4 PIDI_Object2ClipPos(float4 pos) // overload for float4; avoids "implicit truncation" warning for existing shaders
{
    return PIDI_Object2ClipPos(pos.xyz);
}

half PIDI_PerceptualRoughnessToRoughness(half perceptualRoughness)
{
    return perceptualRoughness * perceptualRoughness;
}

// Smoothness is the user facing name
// it should be perceptualSmoothness but we don't want the user to have to deal with this name
half PIDI_SmoothnessToRoughness(half smoothness)
{
    return (1 - smoothness) * (1 - smoothness);
}

half PIDI_SmoothnessToPerceptualRoughness(half smoothness)
{
    return (1 - smoothness);
}

// Note: Disney diffuse must be multiply by diffuseAlbedo / PI. This is done outside of this function.
half PIDI_DisneyDiffuse(half NdotV, half NdotL, half LdotH, half perceptualRoughness)
{
    half fd90 = 0.5 + 2 * LdotH * LdotH * perceptualRoughness;
    // Two schlick fresnel term
    half lightScatter   = (1 + (fd90 - 1) * Pow5(1 - NdotL));
    half viewScatter    = (1 + (fd90 - 1) * Pow5(1 - NdotV));

    return lightScatter * viewScatter;
}

inline half PIDI_PerceptualRoughnessToSpecPower (half perceptualRoughness)
{
    half m = PIDI_PerceptualRoughnessToRoughness(perceptualRoughness);   // m is the true academic roughness.
    half sq = max(1e-4f, m*m);
    half n = (2.0 / sq) - 2.0;                          // https://dl.dropboxusercontent.com/u/55891920/papers/mm_brdf.pdf
    n = max(n, 1e-4f);                                  // prevent possible cases of pow(0,0), which could happen when roughness is 1.0 and NdotH is zero
    return n;
}


half4 PIDI_SIMPLEANISO_BRDF (half3 diffColor, half3 specColor, half anisoOffset, half3 anisoDirection, half oneMinusReflectivity, half smoothness, half3 normal, half3 viewDir, UnityLight light, UnityIndirect gi, half occlusion = 1){

			half perceptualRoughness = PIDI_SmoothnessToPerceptualRoughness (smoothness);
			half3 halfDir = Unity_SafeNormalize (light.dir + viewDir);


			#if UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV
				// The amount we shift the normal toward the view vector is defined by the dot product.
				half shiftAmount = dot(normal, viewDir);
				normal = shiftAmount < 0.0f ? normal + viewDir * (-shiftAmount + 1e-5f) : normal;

				
				// A re-normalization should be applied here but as the shift is small we don't do it to save ALU.
				//normal = normalize(normal);
				half nv = saturate(dot(normal, viewDir)); // TODO: this saturate should no be necessary here
			#else
				half nv = abs(dot(normal, viewDir));    // This abs allow to limit artifact
			#endif


				fixed HdotA = dot(Unity_SafeNormalize (normal + anisoDirection), halfDir);
				float aniso = max(0, sin(radians((HdotA + anisoOffset) * 180)));

				half inv = 1-nv;
				half nl = saturate(dot(normal, light.dir));
				half uNL = saturate(dot(normal,light.dir)+0.45)/(1.45);
				half nh = saturate(dot(normal, halfDir));
				
				half lv = saturate(dot(light.dir, viewDir));
				half lh = saturate(dot(light.dir, halfDir));

				// Diffuse term
				half diffuseTerm = PIDI_DisneyDiffuse(nv, uNL, lh, perceptualRoughness)*uNL;
				half strongDiffuse = PIDI_DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
				half powL = pow(light.color,4);

				//half transDiffuse = pow( saturate(dot(viewDir,transNL)), 2 );

				// Specular term
				// HACK: theoretically we should divide diffuseTerm by Pi and not multiply specularTerm!
				// BUT 1) that will make shader look significantly darker than Legacy ones
				// and 2) on engine side "Non-important" lights have to be divided by Pi too in cases when they are injected into ambient SH
				half roughness = PIDI_PerceptualRoughnessToRoughness(perceptualRoughness);
			#if UNITY_BRDF_GGX
				// GGX with roughtness to 0 would mean no specular at all, using max(roughness, 0.002) here to match HDrenderloop roughtness remapping.
				roughness = max(roughness, 0.002);
				half V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
				half D = GGXTerm (nh, roughness);
			#else
				// Legacy
				half V = SmithBeckmannVisibilityTerm (nl, nv, roughness);
				half D = NDFBlinnPhongNormalizedTerm (nh, PIDI_PerceptualRoughnessToSpecPower(perceptualRoughness));
			#endif

			half specularTerm = saturate(pow(aniso,smoothness*64)*specColor*(4+(24*smoothness))); // Torrance-Sparrow model, Fresnel is applied later

			// specularTerm * nl can be NaN on Metal in some cases, use max() to make sure it's a sane value
			specularTerm = max(0, specularTerm * nl);
			

				// surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(roughness^2+1)
				half surfaceReduction;
			#ifdef UNITY_COLORSPACE_GAMMA
					surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;      // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
			#else
					surfaceReduction = 1.0 / (roughness*roughness + 1.0);           // fade \in [0.5;1]
			#endif

			// To provide true Lambert lighting, we need to be able to kill specular completely.
			specularTerm *= any(specColor) ? 1.0 : 0.0;
			
			half3 scatter = saturate(diffColor*2+saturate(uNL))*diffuseTerm;
			half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
			half3 color =   diffColor * (gi.diffuse + light.color * scatter );
			
			//Perform translucency operations ONLY when translucency is not 0
			//if ( translucency > 0 )
				//
			

			color += specularTerm * light.color * FresnelTerm (specColor, lh) * strongDiffuse;

			color += surfaceReduction *0.3* gi.specular * FresnelLerp (specColor, grazingTerm, nv);
			
			return half4(color, 1);
		}


struct SurfaceOutputFurRendering{
			half3 Albedo;
			half3 Normal;
			half AnisoOffset;
			half Smoothness;
			half3 Specular;
			half Occlusion;
			half3 Emission;
			half Alpha;
	};



    inline half4 LightingAnisoRendering_Deferred (SurfaceOutputFurRendering s, half3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal){
			// energy conservation
			half oneMinusReflectivity;
			s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);

			#ifdef ANISOTROPIC_ON
            half4 c = PIDI_SIMPLEANISO_BRDF (s.Albedo, s.Specular, s.AnisoOffset, s.Normal, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect, 1-pow(s.Occlusion,2));
            #else
            half4 c = UNITY_BRDF_PBS (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect );
            #endif

			outDiffuseOcclusion = half4( s.Albedo+c.rgb, s.Occlusion);
			outSpecSmoothness = half4(s.Specular, s.Smoothness);
			outNormal = half4(s.Normal * 0.5 + 0.5, 1);			
			
			  
			
			half4 emission = half4( s.Emission+c.rgb, 1);
			return emission;
	}


inline half4 LightingAnisoRendering (SurfaceOutputFurRendering s, half3 viewDir, UnityGI gi){
	s.Normal = normalize(s.Normal);

	// energy conservation
	half oneMinusReflectivity;
	s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);

	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
	half outputAlpha;
	s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);
    #ifdef ANISOTROPIC_ON
	half4 c = PIDI_SIMPLEANISO_BRDF (s.Albedo, s.Specular, s.AnisoOffset, s.Normal, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect, 1-pow(s.Occlusion,2));
	#else
    half4 c = UNITY_BRDF_PBS (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect );
    #endif
    c.rgb += UNITY_BRDF_GI (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);
	c.a = outputAlpha;
	return c;
	}



    
    inline void LightingAnisoRendering_GI ( SurfaceOutputFurRendering s, UnityGIInput data, inout UnityGI gi){
		UNITY_GI(gi, s, data);
	    }






#endif
