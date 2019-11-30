/*
XFur Studio™ v 1.2

You cannot sell, redistribute, share nor make public this code, even modified, through any means on any platform.
Modifications are allowed only for your own use and to make this product suit better your project's needs.
These modifications may not be redistributed, sold or shared in any way.

For more information, contact us at contact@irreverent-software.com

Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved.
*/


//=====================  VARIABLES USED BY XFUR 1.2 AND ABOVE  ========================

//Textures
sampler2D _BaseTex; //PER MATERIAL/INSTANCE. Skin diffuse color
sampler2D _OcclusionMap; //NOT READY
sampler2D _Normalmap; //PER MATERIAL/INSTANCE. Normalmap used for the skin areas and, if enabled, the fur areas.
sampler2D _GlossSpecular; //PER MATERIAL/INSTANCE. Gloss Specular Map used for the skin areas
sampler2D _FurNoiseMap; //PER MATERIAL/INSTANCE. First Layer (R), Second layer (G), Color Variation(B) Final Fibers (A)
sampler2D _FurFXNoise; //GLOBAL. Noise to be applied to the fur FX. RG = First noise / tone variation map, BA = Second noise / tone variation map
sampler2D _FurGradient; //PER INSTANCE. Main color variations gradient for natural appearance
sampler2D _FurColorMap; //PER MATERIAL/INSTANCE. Main diffuse color to be used in the fur
sampler2D _FurData0; //PER INSTANCE. Fur Mask, Length, Thickness and Occlusion
sampler2D _FurData1; //PER INSTANCE. Fur Direction X, Y, Z and stiffness
sampler2D _FurData2; //NOT READY.
sampler2D _FurPainter; //INTERNAL, USED BY XFUR PAINTER. NOT ASSIGNABLE
sampler2D _VertexNormals;
sampler2D _FurFXMap;

//Colors
fixed4 _FurColorA; //PER INSTANCE. Color modifier for the fur's base coloration
fixed4 _FurColorB; //PER INSTANCE. IN PROGRESS. Color modifier for the fur's secondary strands
fixed4 _FurColorC; //IN PROGRESS. Color modifier for the fur's third strands level
fixed4 _BaseColor; //PER INSTANCE. Color modifier for the skin's base coloration
fixed4 _RimColor; //REVIEW MODE. might be removed. Controls the final coloration of the rim lighting applied to the fur
fixed4 _BaseSpecular; //PER INSTANCE. Specular color for the skin.
fixed4 _FurSpecular; //PER INSTANCE. Specular color for the fur. 
fixed4 _FurGradient0; //PER INSTANCE.Fur Gradiend 0 Color, used for precise fur coloring.
fixed4 _FurGradient1; //PER INSTANCE.Fur Gradiend 1 Color, used for precise fur coloring.
fixed4 _FurGradient2; //PER INSTANCE.Fur Gradiend 2 Color, used for precise fur coloring.
fixed4 _FurGradient3; //PER INSTANCE.Fur Gradiend 3 Color, used for precise fur coloring.
fixed4 _FurPainterColor; //INTERNAL, USED BY XFUR PAINTER
fixed4 _FXColor0; //GLOBAL/PER INSTANCE. Color of the first fur FX applied to the fur. Default is red, used for blood
fixed4 _FXColor1; //GLOBAL/PER INSTANCE. Color of the second fur FX applied to the fur. Default is white-blue*noise. Used for snow
fixed4 _FXColor2; //GLOBAL/PER INSTANCE. Color of the third fur FX applied to the fur. Default is brown*noise, used for dust.
fixed4 _FXColor3; //GLOBAL/PER INSTANCE. Color of the fourth fur FX applied to the fur. Default is clear coloration. Customizable.
fixed4 _FXSpecSmooth0;
fixed4 _FXSpecSmooth1;
fixed4 _FXSpecSmooth2;
fixed4 _FXSpecSmooth3;

//Toggles
half _PerInstanceBaseSpec;
half _PerInstanceFurNoise;
half _PerInstanceFurData0;
half _PerInstanceFurData1;
half _PerInstanceFurData2;
half _HasGlossMap;
half _TriplanarMode;
half _UV2Grooming;
half _UV2Painting;
half _SelfCollision;


//Numeric Data
half _UV0Scale1 = 1;
half _UV0Scale2 = 1;
half _UV1Scale1 = 1;
half _UV1Scale2 = 1;
half _UV1Scale3 = 1;
half _UV1Scale4 = 1;
half _FXTexSize;
half _TriplanarScale = 2;
half _BaseSmoothness; //PER INSTANCE. Controls smoothness of the base skin.
half _FurSmoothness; //PER INSTANCE. Controls the smoothness of the fur.
half _AnisotropicOffset; //PER INSTANCE. Controls the offset of the anisotropic specularity effects
half _FurStep; //INTERNALLY DEFINED
half _FurLength; //PER MATERIAL/INSTANCE. Controls the fur length of this instance.
half _FurStiffness; //PER MATERIAL/INSTANCE. Controls the fur stiffness of this instance.
half _FurThin; //PER MATERIAL/INSTANCE. Controls the fur thickness of this instance.
half _FurOcclusion; //PER MATERIAL/INSTANCE. Controls the fur occlusion of this instance.
half _FurShadowing; //PER MATERIAL/INSTANCE. Controls the fur shadowin of this instance.
half _FurRimStrength;
half _WindSpeed; //GLOBAL. Wind Speed
half _LocalWindStrength; //PER INSTANCE. The global wind strength will be multiplied by this value.
half _LocalPhysicsStrength; //PER INSTANCE. The final physics influence will be multiplied by this value.
half _FX0Penetration; //PER INSTANCE. Global opacity of the first fur FX.
half _FX1Penetration; //PER INSTANCE. Global opacity of the second fur FX.
half _FX2Penetration; //PER INSTANCE. Global opacity of the third fur FX.
half _FX3Penetration; //PER INSTANCE. Global opacity of the fourth fur FX.
half _FurCutoff;
half _FurGravity;
half _FurPhysics;

//Vectorial Data
fixed4 _FurDirection; //PER INSTANCE. Global direction applied to the fur on a per-object basis

//Custom Lighting Data. NOT READY, CURRENTLY UNDER HEAVY WORKS
fixed4 _CustomLight0Dir;
fixed4 _CustomLight1Dir;
fixed4 _CustomLight0Dat;
fixed4 _CustomLight1Dat;
fixed4 _CustomLight0Color;
fixed4 _CustomLight1Color;

//Vectorial forces
fixed4 _WindDirection; //GLOBAL. Direction / Strength of the wind
fixed4 _RadialForce0; //NOT READY
fixed4 _RadialForce1; //NOT READY

//Vectorial Physics
fixed4 _VectorPhysics[128]; //PER INSTANCE. Vectorial forces to be used for the new Physics module. Up to 128 bones are supported.
fixed4 _AnglePhysics[128]; //PER INSTANCE. Angular forces to be used for the new Physics module.

struct Input {
	float2 uv_BaseTex;
	float2 uv2_FurNoiseMap;
    float3 viewDir;
    float4 vertexPos;
    float4 vertexNormal;
    float samplePass;
    fixed4 color:COLOR;
};

