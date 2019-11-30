/*
XFur Studio™ v 1.2

You cannot sell, redistribute, share nor make public this code, even modified, through any means on any platform.
Modifications are allowed only for your own use and to make this product suit better your project's needs.
These modifications may not be redistributed, sold or shared in any way.

For more information, contact us at contact@irreverent-software.com

Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved.
*/

using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using XFurStudio;
using System;

public class XFur_ShaderEditor : ShaderGUI{

    private Material targetMat;
    public bool[] folds;

    void EnableGUI(){
        folds = new bool[24];
        
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties){
       

        if ( !targetMat ){
            targetMat = materialEditor.target as Material;
            EnableGUI();
        }

        

        if ( targetMat.name.Contains(" Samples")){
            GUILayout.BeginHorizontal();GUILayout.Space(12);
            EditorGUILayout.HelpBox("This material is a managed instanced material used internally by XFur. Its properties cannot be edited directly. Please either select the original material or edit the per instance properties to modify this material's appearance", MessageType.Info );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }
        else{

            Undo.RecordObject(targetMat, "XFUR SHADER EDITOR ID"+targetMat.GetInstanceID());

            var pidiSkin = XFur_SystemEditor.PDEditor_GetCustomGUI();
            var tSkin = GUI.skin;

            GUI.skin = pidiSkin;
            GUILayout.BeginHorizontal();GUILayout.BeginVertical(pidiSkin.box);
            GUILayout.Space(8);
            
            
            if ( XFur_SystemEditor.PDEditor_BeginFold("Base Properties", ref folds[0]) ){   
                targetMat.SetColor( "_BaseColor", XFur_SystemEditor.PDEditor_Color( new GUIContent("Main Color", "The final tint of the mesh under the fur"), targetMat.GetColor("_BaseColor"), 64 ) );
                if ( targetMat.GetFloat("_HasGlossMap") == 0 )
                    targetMat.SetColor( "_BaseSpecular", XFur_SystemEditor.PDEditor_Color( new GUIContent("Specular Color", "The specular color of the mesh under the fur"), targetMat.GetColor("_BaseSpecular"), 64 ) );
                    
                targetMat.SetTexture( "_BaseTex", XFur_SystemEditor.PDEditor_ObjectField<Texture2D>( new GUIContent("Main Texture", "The texture that will be applied to the mesh under the fur"), (Texture2D)targetMat.GetTexture("_BaseTex"), false ) );
                targetMat.SetTexture( "_GlossSpecular", XFur_SystemEditor.PDEditor_ObjectField<Texture2D>( new GUIContent("Main Specular Map", "The texture that will control the Specular Color (RGB) and glossiness (A) of the mesh under the fur"), (Texture2D)targetMat.GetTexture("_GlossSpecular"), false ) );
                
                if ( targetMat.GetFloat("_HasGlossMap") == 0 )
                    targetMat.SetFloat("_BaseSmoothness", XFur_SystemEditor.PDEditor_Slider( new GUIContent("Smoothness", "The glossiness of the mesh under the fur"), targetMat.GetFloat("_BaseSmoothness"), 0, 1 ) );
                        
                targetMat.SetFloat("_HasGlossMap", targetMat.GetTexture("_GlossSpecular")?1:0 );
                targetMat.SetTexture( "_Normalmap", XFur_SystemEditor.PDEditor_ObjectField<Texture2D>( new GUIContent("Normalmap", "The normalmap that will be applied to the mesh under the fur"), (Texture2D)targetMat.GetTexture("_Normalmap"), false ) );
                targetMat.SetTexture( "_OcclusionMap", XFur_SystemEditor.PDEditor_ObjectField<Texture2D>( new GUIContent("Occlusion Map", "The occlusion map that will be applied to the mesh under the fur"), (Texture2D)targetMat.GetTexture("_OcclusionMap"), false ) );

                GUILayout.Space(4);

                targetMat.SetFloat("_UV0Scale1", XFur_SystemEditor.PDEditor_FloatField( new GUIContent("UV 0 Scale", "The scale of the main UV coordinates channel"), targetMat.GetFloat("_UV0Scale1"), true ) );

                GUILayout.Space(8);

            }

            XFur_SystemEditor.PDEditor_EndFold();

            if ( XFur_SystemEditor.PDEditor_BeginFold("Fur Properties", ref folds[1]) ){
                
                targetMat.SetFloat("_Cull", XFur_SystemEditor.PDEditor_Toggle( new GUIContent("Double sided Fur"), targetMat.GetFloat("_Cull")==0 )?0:2);

                var t = targetMat.IsKeywordEnabled("ANISOTROPIC_ON");
                t = XFur_SystemEditor.PDEditor_Toggle( new GUIContent("Anisotropic Specular"), t );
                
                if ( t ){
                    targetMat.EnableKeyword( "ANISOTROPIC_ON" );
                    targetMat.SetFloat("_AnisotropicOffset", XFur_SystemEditor.PDEditor_Slider( new GUIContent("Anisotropic Offset"), targetMat.GetFloat("_AnisotropicOffset"), -1.0f, 1.0f ) );
                }
                else{
                    targetMat.DisableKeyword("ANISOTROPIC_ON");
                }

                targetMat.SetFloat("_TriplanarMode", XFur_SystemEditor.PDEditor_Toggle(new GUIContent("Triplanar Mode"), targetMat.GetFloat("_TriplanarMode")==1)?1:0);
                if ( targetMat.GetFloat("_TriplanarMode") == 1 ){
                    targetMat.SetFloat("_TriplanarScale", XFur_SystemEditor.PDEditor_FloatField(new GUIContent("Triplanar Scale"), targetMat.GetFloat("_TriplanarScale") ) );
                }
                targetMat.SetFloat("_LocalWindStrength", XFur_SystemEditor.PDEditor_Slider(new GUIContent("Wind Strength","The influence the wind will have over this material"), targetMat.GetFloat("_LocalWindStrength"), 0, 64 ) );
                GUILayout.Space(4);
                targetMat.SetTexture( "_FurColorMap", XFur_SystemEditor.PDEditor_ObjectField<Texture2D>( new GUIContent("Fur Color Map", "The diffuse texture to be used by the fur"), (Texture2D)targetMat.GetTexture("_FurColorMap"), false ) );
                targetMat.SetTexture( "_FurData0", XFur_SystemEditor.PDEditor_ObjectField<Texture2D>( new GUIContent("Fur Data 0", "The fur data map and mask"), (Texture2D)targetMat.GetTexture("_FurData0"), false ) );
                targetMat.SetTexture( "_FurData1", XFur_SystemEditor.PDEditor_ObjectField<Texture2D>( new GUIContent("Fur Data 1", "The Directional/Stiffness map for this material"), (Texture2D)targetMat.GetTexture("_FurData1"), false ) );
                targetMat.SetTexture( "_FurNoiseMap", XFur_SystemEditor.PDEditor_ObjectField<Texture2D>( new GUIContent("Fur Gen Map", "The noise based multi-layer texture used for fur generation"), (Texture2D)targetMat.GetTexture("_FurNoiseMap"), false ) );
                
                GUILayout.Space(4);
                targetMat.SetColor( "_FurColorA", XFur_SystemEditor.PDEditor_Color( new GUIContent("Main Fur Color", "The main final tint of the fur"), targetMat.GetColor("_FurColorA"), 64 ) );
                targetMat.SetColor( "_FurColorB", XFur_SystemEditor.PDEditor_Color( new GUIContent("Secondary Fur Color", "The secondary final tint of the fur"), targetMat.GetColor("_FurColorB"), 64 ) );
                targetMat.SetColor( "_FurSpecular", XFur_SystemEditor.PDEditor_Color( new GUIContent("Fur Specular", "The specular color of the fur"), targetMat.GetColor("_FurSpecular"), 64 ) );
                GUILayout.Space(4);

                targetMat.SetColor( "_RimColor", XFur_SystemEditor.PDEditor_Color(new GUIContent("Fur Rim Color"), targetMat.GetColor("_RimColor"), 64));
                targetMat.SetFloat("_FurRimStrength",XFur_SystemEditor.PDEditor_Slider(new GUIContent("Rim Power"), targetMat.GetFloat("_FurRimStrength"), 0, 1 ) );

                if ( targetMat.GetTexture("_FurNoiseMap") ){
                    targetMat.SetFloat("_FurSmoothness", XFur_SystemEditor.PDEditor_Slider( new GUIContent("Fur Smoothness","The smoothness to be applied to the generated fur"), targetMat.GetFloat("_FurSmoothness"), 0, 1 ) );
                    targetMat.SetFloat("_FurCutoff", XFur_SystemEditor.PDEditor_Slider( new GUIContent("Fur Cutoff","The alpha cutoff to be applied to the generated fur"), targetMat.GetFloat("_FurCutoff"), 0, 1 ) );
                    targetMat.SetFloat("_FurOcclusion", XFur_SystemEditor.PDEditor_Slider( new GUIContent("Fur Occlusion", "The occlusion of the fur"), targetMat.GetFloat("_FurOcclusion"), 0, 1 ));
                    targetMat.SetFloat("_FurLength", XFur_SystemEditor.PDEditor_Slider( new GUIContent("Fur Length","The length of the fur"), targetMat.GetFloat("_FurLength"), 0, 4 ) );
                    targetMat.SetFloat("_FurThin", XFur_SystemEditor.PDEditor_Slider( new GUIContent("Fur Thickness","Make each fur strand look finer"), targetMat.GetFloat("_FurThin"), 0, 1 ) );
                }

                targetMat.SetVector("_FurDirection", XFur_SystemEditor.PDEditor_Vector4( new GUIContent("Fur Direction"), targetMat.GetVector("_FurDirection") ) );

                GUILayout.Space(4);

                GUILayout.Space(4);
                
                
                
                targetMat.SetFloat("_UV1Scale1", XFur_SystemEditor.PDEditor_FloatField( new GUIContent("Fur UV Scale 1", "The scale of the first fur UV coordinates channel"), targetMat.GetFloat("_UV1Scale1"), true ) );
                targetMat.SetFloat("_UV1Scale2", XFur_SystemEditor.PDEditor_FloatField( new GUIContent("Fur UV Scale 2", "The scale of the second fur UV coordinates channel"), targetMat.GetFloat("_UV1Scale2"), true ) );
                
                
                GUILayout.Space(8);
            }
            XFur_SystemEditor.PDEditor_EndFold();
            
            if ( XFur_SystemEditor.PDEditor_BeginFold("Additional Settings", ref folds[2])){
                targetMat.SetTexture("_FurFXNoise", XFur_SystemEditor.PDEditor_ObjectField<Texture2D>( new GUIContent("FX Param. Noise","The parametric noise texture used to generate and control fx over the fur"), (Texture2D)targetMat.GetTexture("_FurFXNoise"), false ) );
                targetMat.SetFloat("_UV0Scale2", XFur_SystemEditor.PDEditor_FloatField( new GUIContent("FX UV Scale"), targetMat.GetFloat("_UV0Scale2")));
            }
            XFur_SystemEditor.PDEditor_EndFold();
            
            GUILayout.Space(8);

            var tempStyle = new GUIStyle();
            tempStyle.normal.textColor = new Color(0.75f,0.75f,0.75f,0.75f);
            tempStyle.fontSize = 9;
            tempStyle.fontStyle = FontStyle.Italic;
            GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
            GUILayout.Label("PIDI - XFur Studio™. Version 1.4", tempStyle );
            GUILayout.FlexibleSpace();GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.EndVertical();GUILayout.Space(8);GUILayout.EndHorizontal();
            GUI.skin = tSkin;
        }

    }
}