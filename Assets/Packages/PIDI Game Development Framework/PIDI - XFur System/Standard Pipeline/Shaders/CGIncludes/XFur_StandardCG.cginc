/*
XFur Studio™ v 1.2

You cannot sell, redistribute, share nor make public this code, even modified, through any means on any platform.
Modifications are allowed only for your own use and to make this product suit better your project's needs.
These modifications may not be redistributed, sold or shared in any way.

For more information, contact us at contact@irreverent-software.com

Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved.
*/


#include "UnityCG.cginc"
#include "../CGIncludes/XFur_MainCG.cginc"
#include "../CGIncludes/XFur_LightingCG.cginc"


void FurVertexPass( appdata_full v, out float4 pos, out fixed4 col, int currentPass ){
    

    float fStep = 0.3/_FurStep;
    float2 groomingUV = lerp( v.texcoord.xy*_UV0Scale1, v.texcoord1.xy*_UV0Scale1, _UV2Grooming );
    fixed4 furData0 = tex2Dlod( _FurData0, float4(v.texcoord.xy*_UV0Scale1,0,0) );
    fixed4 fDirection = tex2Dlod( _FurData1, float4(groomingUV,0,0 ) );
    fDirection.rgb = fDirection.rgb*2-fixed3(1,1,1);
    int y = (int)v.texcoord2.w/_FXTexSize;
    int x = (int)v.texcoord2.w-(_FXTexSize*y);
    float2 fxCoord = float2( saturate(1.0/_FXTexSize*x), saturate(1.0/_FXTexSize*y) );

    col = tex2Dlod( _FurFXMap, float4( fxCoord, 0, 0 ) );

    _FurGravity *= 1+0.3*col.b;

    float3 tpos = v.vertex.xyz;
    pos = v.vertex;
    half furLength = _FurLength*fStep*furData0.g*currentPass*(1-min(float2(col.r*_FX0Penetration,col.b*_FX2Penetration), 0.25));
    float3 normal = UnityObjectToWorldNormal(v.normal);
    
    float3 nPhysDir = float3(0,0,0);
    if ( _FurDirection.w < 1 ){
    
        half3 newDir = normal;
        newDir -= 0.00045*sign(newDir);
        half3 axis = cross(normalize(v.texcoord3.xyz), newDir );
        half angle = acos(dot(normalize(v.texcoord3.xyz), newDir));

        half s = sin(angle);
        half c = cos(angle);

        float oc = 1.0f-c;

        float3x3 rotMatrix = float3x3( oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,
                    oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,
                    oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c );
        
        float3 furDir = mul(rotMatrix, float3( _FurDirection.x, _FurDirection.y, _FurDirection.z ) * fDirection.rgb * furLength*4 );
        /*
        float3 nPhysDir = furDir*furLength;
        */
        
        nPhysDir = v.normal*furLength;
        nPhysDir += furDir;
    }
    else{
        half3 newDir = normal;
        newDir -= 0.00045*sign(newDir);
        half3 axis = cross(normalize(v.texcoord3.xyz), newDir );
        half angle = acos(dot(normalize(v.texcoord3.xyz), newDir));

        half s = sin(angle);
        half c = cos(angle);

        float oc = 1.0f-c;

        float3x3 rotMatrix = float3x3( oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,
                    oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,
                    oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c );
        
        float3 furDir = mul(rotMatrix, float3( _FurDirection.x, _FurDirection.y, _FurDirection.z ) * fDirection.rgb * furLength*4 );
        /*
        float3 nPhysDir = furDir*furLength;
        */
        
        nPhysDir = normalize(v.normal+furDir)*furLength;
    }

    if ( length(_VectorPhysics[v.texcoord3.w])+length(_AnglePhysics[v.texcoord3.w].y)+abs(_FurGravity)!=0 ){
        nPhysDir += fDirection.a*saturate(1-length(col.rg))*(1-col.b*0.25)*mul ( unity_WorldToObject, float4(_VectorPhysics[v.texcoord3.w]*((fStep*currentPass-0.15)*-6.66)+float3(_AnglePhysics[v.texcoord3.w].y*dot(normal,float3(0,0,1)),_AnglePhysics[v.texcoord3.w].x*dot(normal,float3(0,0,1))+_AnglePhysics[v.texcoord3.w].z*dot(normal,float3(1,0,0)),_AnglePhysics[v.texcoord3.w].y*dot(normal,float3(1,0,0)))+float3(0,-_FurGravity-(_FurGravity*0.3*col.b),0),0) * furLength*4 ).xyz;   
    }
    nPhysDir += fDirection.a*saturate(1-length(col.rgb))*_LocalWindStrength*mul ( unity_WorldToObject, float4( _WindDirection.x+(1-abs(_WindDirection.x))*_WindDirection.x*sin(fStep*currentPass*3+0.1*v.texcoord2.y+_Time.y*(32+(1.5*v.texcoord2.x))*_WindSpeed), _WindDirection.y+(1-abs(_WindDirection.y))*_WindDirection.y*sin(fStep*currentPass*3+1.345*v.texcoord2.z+_Time.z+_Time.y*(8.4+(v.texcoord2.z))*_WindSpeed), _WindDirection.z+(1-abs(_WindDirection.z))*_WindDirection.z*sin(fStep*currentPass*3+0.33*v.texcoord2.y+_Time.y*(12.6+(8.20*v.texcoord2.y))*_WindSpeed), 0 ) * furLength*4 ).xyz;
    
 
    pos.xyz += nPhysDir;  
    
    pos.xyz -= 1.15*normalize(nPhysDir)*length(pos.xyz-tpos.xyz)*_SelfCollision*saturate(1-sign(dot( normalize(pos.xyz-tpos),v.normal.xyz )-0.05));
    
    //float3 diffNX = cross(v.normal, v.texcoord.2);

    
    
    


}



void FurDynVertexPass( appdata_full v, out float4 pos, out fixed4 col ){
    
    int currentPass = (int)(v.texcoord1.z*24);

    float fStep = 0.3/_FurStep;

    float2 groomingUV = lerp( v.texcoord.xy*_UV0Scale1, v.texcoord1.xy*_UV0Scale1, _UV2Grooming );
    fixed4 furData0 = tex2Dlod( _FurData0, float4(v.texcoord.xy*_UV0Scale1,0,0) );
    fixed4 fDirection = tex2Dlod( _FurData1, float4(groomingUV,0,0 ) );
    fDirection.rgb = fDirection.rgb*2-fixed3(1,1,1);
    int y = (int)v.texcoord2.w/_FXTexSize;
    int x = (int)v.texcoord2.w-(_FXTexSize*y);
    float2 fxCoord = float2( saturate(1.0/_FXTexSize*x), saturate(1.0/_FXTexSize*y) );

    col = tex2Dlod( _FurFXMap, float4( fxCoord, 0, 0 ) );

    float3 tpos = v.vertex.xyz;
    pos = v.vertex;
    half furLength = _FurLength*fStep*furData0.g*currentPass*(1-min(float2(col.r*_FX0Penetration,col.b*_FX2Penetration), 0.5));
    float3 normal = UnityObjectToWorldNormal(v.normal);
    
    float3 nPhysDir = float3(0,0,0);
    if ( _FurDirection.w < 1 ){
    
        half3 newDir = normal;
        newDir -= 0.0003*sign(newDir);
        half3 axis = cross(normalize(v.texcoord3.xyz), newDir );
        half angle = acos(dot(normalize(v.texcoord3.xyz), newDir));

        half s = sin(angle);
        half c = cos(angle);

        float oc = 1.0f-c;

        float3x3 rotMatrix = float3x3( oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,
                    oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,
                    oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c );
        
        float3 furDir = mul(rotMatrix, float3( _FurDirection.x, _FurDirection.y, _FurDirection.z ) * fDirection.rgb * furLength*4 );
        /*
        float3 nPhysDir = furDir*furLength;
        */
        
        nPhysDir = v.normal*furLength;
        nPhysDir += furDir;
    }
    else{
        half3 newDir = normal;
        newDir -= 0.0003*sign(newDir);

        half3 axis = cross(normalize(v.texcoord3.xyz), newDir );
        half angle = acos(dot(normalize(v.texcoord3.xyz), newDir));

        half s = sin(angle);
        half c = cos(angle);

        float oc = 1.0f-c;

        float3x3 rotMatrix = float3x3( oc * axis.x * axis.x + c,           oc * axis.x * axis.y - axis.z * s,  oc * axis.z * axis.x + axis.y * s,
                    oc * axis.x * axis.y + axis.z * s,  oc * axis.y * axis.y + c,           oc * axis.y * axis.z - axis.x * s,
                    oc * axis.z * axis.x - axis.y * s,  oc * axis.y * axis.z + axis.x * s,  oc * axis.z * axis.z + c );
        
        float3 furDir = mul(rotMatrix, float3( _FurDirection.x, _FurDirection.y, _FurDirection.z ) * fDirection.rgb * furLength*4 );
        /*
        float3 nPhysDir = furDir*furLength;
        */
        
        nPhysDir = normalize(v.normal+furDir)*furLength;
    }

    

    if ( length(_VectorPhysics[v.texcoord3.w])+length(_AnglePhysics[v.texcoord3.w].y)+abs(_FurGravity)!=0 ){
        nPhysDir += fDirection.a*saturate(1-length(col.rg))*(1-col.b*0.25)*mul ( unity_WorldToObject, float4(_VectorPhysics[v.texcoord3.w]*((fStep*currentPass-0.15)*-6.66)+float3(_AnglePhysics[v.texcoord3.w].y*dot(normal,float3(0,0,1)),_AnglePhysics[v.texcoord3.w].x*dot(normal,float3(0,0,1))+_AnglePhysics[v.texcoord3.w].z*dot(normal,float3(1,0,0)),_AnglePhysics[v.texcoord3.w].y*dot(normal,float3(1,0,0)))+float3(0,-_FurGravity-(_FurGravity*0.3*col.b),0),0) * furLength*4 ).xyz;   
    }
    nPhysDir += fDirection.a*saturate(1-length(col.rgb))*_LocalWindStrength*mul ( unity_WorldToObject, float4( _WindDirection.x+(1-abs(_WindDirection.x))*_WindDirection.x*sin(fStep*currentPass*3+0.1*v.texcoord2.y+_Time.y*(32+(1.5*v.texcoord2.x))*_WindSpeed), _WindDirection.y+(1-abs(_WindDirection.y))*_WindDirection.y*sin(fStep*currentPass*3+1.345*v.texcoord2.z+_Time.z+_Time.y*(8.4+(v.texcoord2.z))*_WindSpeed), _WindDirection.z+(1-abs(_WindDirection.z))*_WindDirection.z*sin(fStep*currentPass*3+0.33*v.texcoord2.y+_Time.y*(12.6+(8.20*v.texcoord2.y))*_WindSpeed), 0 ) * furLength*4 ).xyz;
    
 
    pos.xyz += nPhysDir;  
    
    pos.xyz -= 1.15*normalize(nPhysDir)*length(pos.xyz-tpos.xyz)*_SelfCollision*saturate(1-sign(dot( normalize(pos.xyz-tpos),v.normal.xyz )-0.05));
    

}

void FurUnderCoat( Input IN, inout SurfaceOutputFurRendering o, int currentPass ){
    
    float fStep = 0.33/_FurStep;
    float2 furUV1 = IN.uv2_FurNoiseMap*_UV1Scale1;
    float2 furUV2 = IN.uv2_FurNoiseMap*_UV1Scale2;
 
    float4 furNoise = float4(0,0,0,0);
    float4 furNoise2 = float4(0,0,0,0);
    float4 furfxNoise = float4(0,0,0,0);

    half3 blend = abs(IN.vertexNormal); 
    blend/= dot(blend, (float3)1);

    if ( _TriplanarMode == 0 ){
        furNoise = tex2D( _FurNoiseMap, furUV1 );
        furNoise2 = tex2D( _FurNoiseMap, furUV2*max(1-IN.color.b*_FX2Penetration,0.85) );
        half4 fxNoiseX = tex2D( _FurFXNoise, IN.vertexPos.yz*_UV0Scale2*_TriplanarScale ) * blend.x;
        half4 fxNoiseY = tex2D( _FurFXNoise, IN.vertexPos.xz*_UV0Scale2*_TriplanarScale ) * blend.y;
        half4 fxNoiseZ = tex2D( _FurFXNoise, IN.vertexPos.xy*_UV0Scale2*_TriplanarScale ) * blend.z;
        
        furfxNoise = fxNoiseX+fxNoiseY+fxNoiseZ;
    }
    else{    


        half4 furNoiseX = tex2D( _FurNoiseMap, IN.vertexPos.yz*_UV1Scale1*_TriplanarScale ) * blend.x;
        half4 furNoiseY = tex2D( _FurNoiseMap, IN.vertexPos.xz*_UV1Scale1*_TriplanarScale ) * blend.y;
        half4 furNoiseZ = tex2D( _FurNoiseMap, IN.vertexPos.xy*_UV1Scale1*_TriplanarScale ) * blend.z;

        half4 furNoise2X = tex2D( _FurNoiseMap, IN.vertexPos.yz*_UV1Scale2*_TriplanarScale ) * blend.x;
        half4 furNoise2Y = tex2D( _FurNoiseMap, IN.vertexPos.xz*_UV1Scale2*_TriplanarScale ) * blend.y;
        half4 furNoise2Z = tex2D( _FurNoiseMap, IN.vertexPos.xy*_UV1Scale2*_TriplanarScale ) * blend.z;

        furNoise = furNoiseX+furNoiseY+furNoiseZ;
        furNoise2 = furNoise2X+furNoise2Y+furNoise2Z;

        half4 fxNoiseX = tex2D( _FurFXNoise, IN.vertexPos.yz*_UV0Scale2*_TriplanarScale ) * blend.x;
        half4 fxNoiseY = tex2D( _FurFXNoise, IN.vertexPos.xz*_UV0Scale2*_TriplanarScale ) * blend.y;
        half4 fxNoiseZ = tex2D( _FurFXNoise, IN.vertexPos.xy*_UV0Scale2*_TriplanarScale ) * blend.z;
        
        furfxNoise = fxNoiseX+fxNoiseY+fxNoiseZ;
        
    }
    half4 fDat0 = tex2D( _FurData0, IN.uv_BaseTex*_UV0Scale1);
    float fur1Alpha = pow( furNoise.r, 1+fStep*32*_FurThin*fDat0.a*currentPass );
    float fur3Alpha = pow( furNoise.g, 1+fStep*32*_FurThin*fDat0.a*currentPass );
    float fur2Alpha = furNoise2.a;

    half4 fColor = tex2D(_FurColorMap, IN.uv_BaseTex*_UV0Scale1);
    half4 c = lerp( fColor*_FurColorA*2, lerp(fColor*_FurColorA*2,_FurColorB*2,0.55f), furNoise2.b )*lerp( 1, saturate( fur1Alpha*fStep*currentPass*4+(1.8*_FurThin) )* fur2Alpha, _FurOcclusion*fDat0.b );
    

    half4 blood = _FXColor0*furfxNoise.r;
    half bloodA = (1-(IN.color.b*furfxNoise.a*0.5))*saturate(pow( furfxNoise.a*IN.color.r, 2*(1.15-IN.color.r) ))*saturate(((1.0/_FurStep*currentPass)-(1-_FX0Penetration))/(1.00001-_FX0Penetration)); 
    half bloodOcclusion = 0.1;

    half4 snow = _FXColor1*furfxNoise.g;
    half snowA = (1-IN.color.r*furfxNoise.a)*saturate(pow( furfxNoise.a*IN.color.g, 3*(1.35-IN.color.g) ))*saturate((1.0/_FurStep*currentPass-(1-_FX1Penetration))/(1.00001-_FX1Penetration)); 
    half snowOcclusion = 0.9;

    half4 water = _FXColor2*c;
    half waterA = (1-(IN.color.g*furfxNoise.a+IN.color.r*0.5*furfxNoise.a))*saturate(pow( IN.color.b, 2.8*(1.35-IN.color.b) ))*saturate((1.0/_FurStep*currentPass-(1-_FX2Penetration))/(1.00001-_FX2Penetration));
    

    half waterOcclusion = 0.2;

    half4 alphas = float4(bloodA,snowA,waterA,0);
    half invAlphas = (1-length(alphas));
    
    half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal ));
    o.Alpha =  ( ( fur1Alpha+saturate(2-currentPass*1.0) ) * fur2Alpha ) + ( saturate(0.15*bloodA*fur1Alpha*(1+_FX0Penetration)) ) + ( saturate(0.4*snowA*fur1Alpha*(1+_FX1Penetration)) )+( saturate(0.05*waterA*fur1Alpha*(1+_FX2Penetration)) );
    o.Smoothness = (_FurSmoothness*8*fStep*currentPass*fur2Alpha)*invAlphas+(_FXSpecSmooth0.a*bloodA*furfxNoise.r)+(_FXSpecSmooth1.a*snowA)+(_FXSpecSmooth2.a*waterA*fur2Alpha);
    o.AnisoOffset = _AnisotropicOffset*max(0.4,fur1Alpha);
    o.Occlusion = lerp(1, 8*fStep*currentPass*fur2Alpha*invAlphas+bloodOcclusion+snowOcclusion+waterOcclusion, _FurOcclusion*fDat0.b*(length(alphas)) );
    o.Specular = (_FurSpecular*4*fStep*currentPass*fur2Alpha)*invAlphas+(_FXSpecSmooth0.rgb*IN.color.r)+(_FXSpecSmooth1.rgb*snowA)+(_FXSpecSmooth2.rgb*waterA*fur2Alpha);
    
    o.Albedo = c*invAlphas+(blood*bloodA)+(snow*snowA)+(water*waterA*pow(fur2Alpha,2));
    #ifdef XFUR_PAINTER
    float2 uvSet = lerp( IN.uv_BaseTex, IN.uv2_FurNoiseMap, _UV2Painting )*_UV0Scale1;
    half4 paint = tex2D(_FurPainter,uvSet*_UV0Scale1);
    #else
    half4 paint = half4(0,0,0,0);
    #endif
    
    
 
    o.Emission = paint.rgb+(_RimColor*(1-length(IN.color.rgb))*fur2Alpha*pow(o.Occlusion,3)*pow(rim, max(_FurRimStrength,0.1)*8));

    clip(o.Alpha*fDat0.r-_FurCutoff);
}