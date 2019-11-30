 /*
XFur Studio™ - XFur Generic Module
Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved
*/

using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

namespace XFurStudio{
    [System.Serializable]
    public class XFur_CoatingModule:XFur_SystemModule {
        
        public XFur_CoatingSettings[] coatingSettings = new XFur_CoatingSettings[0];
        public bool profileVariation;
        

        public override void Module_Start( XFur_System owner ){
            
            systemOwner = owner;

            if ( !systemOwner || !systemOwner.database )
                return; 

            if ( coatingSettings.Length != systemOwner.OriginalMesh.subMeshCount ){
                coatingSettings = new XFur_CoatingSettings[systemOwner.OriginalMesh.subMeshCount];
                for ( int i = 0; i < coatingSettings.Length; i++ ){
                    coatingSettings[i] = new XFur_CoatingSettings();
                    systemOwner.CopyFurProperties( systemOwner.materialProfiles[i], ref coatingSettings[i].originalP );
                }
            }

            for ( int i = 0; i < coatingSettings.Length; i++ ){
                if ( coatingSettings[i].originalP == null ){
                    coatingSettings[i].originalP = new XFur_MaterialProperties();
                }
                systemOwner.CopyFurProperties( systemOwner.materialProfiles[i], ref coatingSettings[i].originalP );
                coatingSettings[i].originalP.originalMat = systemOwner.materialProfiles[i].originalMat;
            }            
            

            if ( Application.isPlaying && State == XFurModuleState.Enabled ){
                RandomizeFur();
            }
        
        }

        public override void Module_Execute(){

        }

        public override void Module_End(){

        }


        public void RandomizeFur(){
            for( int i = 0; i < coatingSettings.Length; i++ ){
                    if ( systemOwner.materialProfiles[i].furmatType == 2 && coatingSettings[i].coatingProfiles.Length > 0 ){
                        var rnd = Random.Range(0,coatingSettings[i].coatingProfiles.Length);
                        systemOwner.XFur_LoadFurProfile( coatingSettings[i].coatingProfiles[rnd], i );
                        
                        if ( profileVariation ){
                            systemOwner.materialProfiles[i].furmatFurColorA = Color.Lerp( coatingSettings[i].coatingProfiles[rnd].furColorA_Min, coatingSettings[i].coatingProfiles[rnd].furColorA_Max, Random.Range(0.0f,1.1f) );
                            systemOwner.materialProfiles[i].furmatFurColorB = Color.Lerp( coatingSettings[i].coatingProfiles[rnd].furColorB_Min, coatingSettings[i].coatingProfiles[rnd].furColorB_Max, Random.Range(0.0f,1.1f) );
                        }
                    }
                }
        }

        #if UNITY_EDITOR

        public override void Module_StartUI( GUISkin editorSkin ){
            base.Module_StartUI( editorSkin );
            moduleName = "Randomization Module 1.0 (BETA)";

        }


        public override void Module_UI(){
            base.Module_UI();
            GUILayout.Space(8);

            if ( State == XFurModuleState.Enabled ){
                for ( int i = 0; i < coatingSettings.Length; i++ ){

                    PDEditor_BeginGroupingBox( coatingSettings[i].originalP.originalMat.name );

                    GUILayout.Space(8);
                    PDEditor_Toggle( new GUIContent("Per Profile Color Variation"), ref profileVariation );
                    GUILayout.Space(8);

                    for ( int c = 0; c < coatingSettings[i].coatingProfiles.Length; c++ ){
                        GUILayout.BeginHorizontal();
                        PDEditor_ObjectField<XFur_CoatingProfile>( new GUIContent("Fur Profile "+c,"The fur profile to use as a random option for this module" ), ref coatingSettings[i].coatingProfiles[c], false );
                        if ( GUILayout.Button( "Remove", GUILayout.MaxWidth(100) ) ){
                            var l = new List<XFur_CoatingProfile>(coatingSettings[i].coatingProfiles);
                            l.RemoveAt(c);
                            coatingSettings[i].coatingProfiles = l.ToArray();
                        }
                        GUILayout.Space(8);GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(8);

                    GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                    if ( GUILayout.Button( "Add New Profile", GUILayout.MaxWidth(200) ) ){
                        var l = new List<XFur_CoatingProfile>(coatingSettings[i].coatingProfiles);
                        l.Add(null);
                        coatingSettings[i].coatingProfiles = l.ToArray();
                    }
                    GUILayout.FlexibleSpace();GUILayout.EndHorizontal();


                    PDEditor_EndGroupingBox();

                    GUILayout.Space(12);
                }
            }
        }
        

        #endif



        
    }


    [System.Serializable]
    public class XFur_CoatingSettings{
        public XFur_CoatingProfile[] coatingProfiles = new XFur_CoatingProfile[0];
        public XFur_MaterialProperties originalP = new XFur_MaterialProperties();

        
    }
    
}