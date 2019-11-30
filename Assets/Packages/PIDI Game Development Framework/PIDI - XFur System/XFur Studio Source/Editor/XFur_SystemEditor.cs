/*
XFur Studio™ - XFur System Editor
Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif


namespace XFurStudio{

[CustomEditor(typeof(XFur_System),true)]

public class XFur_SystemEditor:Editor{

    public static GUISkin pidiSkin;
    public AnimBool[] folds;
    public XFur_System mTarget;

    void OnEnable(){
        mTarget = (XFur_System)target;

        if (!Application.isPlaying)
            mTarget.XFur_Start();
        if ( folds == null || folds.Length < 12+(mTarget.customModules==null?0:mTarget.customModules.Length) ){
        folds = new AnimBool[12+(mTarget.customModules==null?0:mTarget.customModules.Length)];
            for ( int i = 0; i < folds.Length; i++ ){
                folds[i] = new AnimBool();
                folds[i].valueChanged.AddListener(Repaint);
            }
        }

        if ( !pidiSkin )
            PDEditor_GetCustomGUI(  );

        if ( mTarget.coatingModule!=null ){
            mTarget.coatingModule.Module_StartUI( pidiSkin );
        }

        
        if ( mTarget.lodModule!=null ){
            mTarget.lodModule.Module_StartUI( pidiSkin );
        }

        
        if ( mTarget.physicsModule!=null ){
            mTarget.physicsModule.Module_StartUI( pidiSkin );
        }

        
        if ( mTarget.fxModule!=null ){
            mTarget.fxModule.Module_StartUI( pidiSkin );
        }

    }

    public override void OnInspectorGUI(){

        Undo.RecordObject(mTarget, "XFur System Editor Target "+mTarget.GetInstanceID() );
        SceneView.RepaintAll();

        if ( !pidiSkin )
            PDEditor_GetCustomGUI(  );

        var tSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        GUI.skin = pidiSkin;

        var buffDatabase = mTarget.database;

        GUILayout.BeginHorizontal();GUILayout.BeginVertical( pidiSkin.box );
        GUILayout.Space(8);
        PDEditor_ObjectField<XFur_DatabaseModule>( new GUIContent("XFur Database Asset", "The XFur Database asset, required for LOD management, mesh handling and most other internal features"), ref mTarget.database, false );
        GUILayout.Space(8);

        if ( buffDatabase != mTarget.database && mTarget.database != null ){
            mTarget.XFur_UpdateMeshData();
        }

        if ( !mTarget.database ){
            GUILayout.BeginHorizontal(); GUILayout.Space(12);
            EditorGUILayout.HelpBox("Please assign the XFur Database Asset to this component. It is required for internal functions and its absence can make this software unstable", MessageType.Error );
            GUILayout.Space(12);GUILayout.EndHorizontal();
            GUILayout.Space(8);
        }
        else{

            if ( mTarget.database.XFur_ContainsMeshData(mTarget.OriginalMesh) != -1 ){
                if ( mTarget.database.meshData[mTarget.database.XFur_ContainsMeshData(mTarget.OriginalMesh)].XFurVersion != mTarget.Version ){
                    GUILayout.BeginHorizontal(); GUILayout.Space(12);
                    EditorGUILayout.HelpBox("The current XFur version you are using does not match the version of the data stored in the database asset. You need to rebuild this data", MessageType.Error );
                    GUILayout.Space(12);GUILayout.EndHorizontal();
                    GUILayout.Space(8);
                    
                    GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
                    if ( GUILayout.Button("Rebuild Data", GUILayout.Width(200) ) ){
                        mTarget.database.XFur_DeleteMeshData(mTarget.database.XFur_ContainsMeshData(mTarget.OriginalMesh));
                        mTarget.XFur_UpdateMeshData();
                    }
                    GUILayout.FlexibleSpace();GUILayout.EndHorizontal();

                    GUILayout.Space(8);
                }
                else{
                    GUILayout.Space(8);
                    GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
                    if ( GUILayout.Button(new GUIContent("Rebuild Data", "Forcefully rebuild all mesh data for this model. Use it if you have made changes to the original model (modified UV coordinates, skinning weights, topology, etc)"), GUILayout.Width(200) ) ){
                        mTarget.database.XFur_DeleteMeshData(mTarget.database.XFur_ContainsMeshData(mTarget.OriginalMesh));
                        mTarget.XFur_UpdateMeshData();
                    }
                    GUILayout.FlexibleSpace();GUILayout.EndHorizontal();

                    GUILayout.Space(8);
                }
            }

        if ( PDEditor_BeginFold( "XFur - Main Settings", ref folds[0] ) ){
            
            PDEditor_Popup( new GUIContent("Material Slot", "The material we are currently editing"), ref mTarget.materialProfileIndex, mTarget.FurMatGUIS );
            
            GUILayout.Space(8);
            PDEditor_ObjectField<Material>( new GUIContent("Base Fur Material", "The fur material that will be used as a reference by this instance"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].originalMat, false );
            GUILayout.Space(8);

            if ( mTarget.materialProfiles[mTarget.materialProfileIndex].buffOriginalMat != mTarget.materialProfiles[mTarget.materialProfileIndex].originalMat ){
                mTarget.UpdateSharedData(mTarget.materialProfiles[mTarget.materialProfileIndex]);
                mTarget.materialProfiles[mTarget.materialProfileIndex].SynchToOriginalMat();
                mTarget.XFur_UpdateMeshData();
            }
            
            GUILayout.Space(4);
            if ( mTarget.materialProfiles[mTarget.materialProfileIndex].originalMat && mTarget.materialProfiles[mTarget.materialProfileIndex].furmatType == 2 ){
                
                if (( !Application.isPlaying || !mTarget.instancedMode ) && mTarget.materialProfiles[mTarget.materialProfileIndex].furmatShadowsMode == 0 ){
                    if ( XFur_System.materialReferences.ContainsKey(mTarget.materialProfiles[mTarget.materialProfileIndex].originalMat) ){
                        var samples = new List<GUIContent>();
            
                        
                        for ( int i = 0; i < mTarget.database.highQualityShaders.Length; i++ ){
                            var shaderName = XFur_System.materialReferences[mTarget.materialProfiles[mTarget.materialProfileIndex].originalMat][i].shader.name;
                            samples.Add( new GUIContent(shaderName.Split("/"[0])[shaderName.Split("/"[0]).Length-1] ) );
                        }

                    if ( mTarget.lodModule.State == XFurModuleState.Enabled ){
                            samples.Clear();
                            samples.Add(new GUIContent("LOD Driven"));
                            var n = 0;
                        PDEditor_Popup( new GUIContent("Fur Samples", "The amount of samples to use on this shader"), ref n, new GUIContent[]{ new GUIContent("LOD Driven")} );
                        
                        if ( !Application.isPlaying )
                            mTarget.materialProfiles[mTarget.materialProfileIndex].furmatSamples = 2;
                    }
                    else{
                        PDEditor_Popup( new GUIContent("Fur Samples", "The amount of samples to use on this shader"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatSamples, samples.ToArray() );
                    }
                    //PDEditor_Popup( new GUIContent("Fur Alpha Mode", "Controls the way the alpha is handled by the fur"), ref mTarget.furAlphaMode, new GUIContent[]{new GUIContent("Standard","The standard basic alpha handling used by older versions of XFur"), new GUIContent("Soft (BETA)", "Softer alpha handling with higher percision than in older versions, currently in BETA")} );
                    //PDEditor_Popup( new GUIContent("Fur Color Mode", "Controls the way the color is handled by the fur"), ref mTarget.furColorMode, new GUIContent[]{new GUIContent("Standard","The standard basic color handling used by older versions of XFur"), new GUIContent("Coating Module (BETA)", "The color of the fur of this instance will be controlled by the coating module")} );
                    }
                }
                else{
                     if ( ( mTarget.lodModule==null || (mTarget.lodModule!=null && mTarget.lodModule.State != XFurModuleState.Enabled) ) && mTarget.runMaterialReferences.ContainsKey(mTarget.materialProfiles[mTarget.materialProfileIndex].originalMat) ){
                        var samples = new List<GUIContent>();
                        

                        if ( mTarget.lodModule.State == XFurModuleState.Enabled ){
                            samples.Clear();
                            samples.Add(new GUIContent("LOD Driven"));
                        }

                    PDEditor_Popup( new GUIContent("Fur Samples", "The amount of samples to use on this shader"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatSamples, samples.ToArray() );
                    //PDEditor_Popup( new GUIContent("Fur Alpha Mode", "Controls the way the alpha is handled by the fur"), ref mTarget.furAlphaMode, new GUIContent[]{new GUIContent("Standard","The standard basic alpha handling used by older versions of XFur"), new GUIContent("Soft (BETA)", "Softer alpha handling with higher percision than in older versions, currently in BETA")} );
                    //PDEditor_Popup( new GUIContent("Fur Color Mode", "Controls the way the color is handled by the fur"), ref mTarget.furColorMode, new GUIContent[]{new GUIContent("Standard","The standard basic color handling used by older versions of XFur"), new GUIContent("Coating Module (BETA)", "The color of the fur of this instance will be controlled by the coating module")} );
                    }
                    else{
                        var n = 0;
                        PDEditor_Popup( new GUIContent("Fur Samples", "The amount of samples to use on this shader"), ref n, new GUIContent[]{ new GUIContent("LOD Driven")} );
                    }
                }

                var s = mTarget.materialProfiles[mTarget.materialProfileIndex].furmatShadowsMode;
                    
                PDEditor_Popup( new GUIContent("Shadows Mode", "Switches between the different shadow modes for the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatShadowsMode, new GUIContent[]{ new GUIContent("Standard Shadows","Simple shadow casting on forward and deferred with full shadow reception enabled only for deferred rendering"), new GUIContent("Full Shadows","Expensive method of full shadowing in forward and deferred that adds accurate shadows in casting and receiving mode based on each fur strand and layer")} );
                    
                if ( s == 1 ){
                    GUILayout.BeginHorizontal(); GUILayout.Space(12);
                    EditorGUILayout.HelpBox("Full shadows generate additional geometry and sub renderers, making it VERY expensive to compute. Do not use them on scenes with more than a couple characters nor in models with more than 6-10k polygons", MessageType.Warning );
                    GUILayout.Space(12);GUILayout.EndHorizontal();
                    GUILayout.Space(8);
                }

                if ( s != mTarget.materialProfiles[mTarget.materialProfileIndex].furmatShadowsMode && mTarget.materialProfiles[mTarget.materialProfileIndex].furmatShadowsMode == 1 ){
                    mTarget.XFur_GenerateShadowMesh( mTarget.materialProfiles[mTarget.materialProfileIndex] );
                }


                if ( mTarget.FurMaterials == 1
                    #if UNITY_2018_1_OR_NEWER
                    || true
                    #endif
                ){
                   
                    GUILayout.Space(8);

                    //PDEditor_Toggle( new GUIContent("Static Material", "If this fur material will not change its length, thickness, color, etc. at runtime it is recommended to toggle this value to handle the material as a static instance for better performance" ), ref mTarget.instancedMode );
                    mTarget.materialProfiles[mTarget.materialProfileIndex].furmatCollision = PDEditor_Toggle( new GUIContent("Basic Self-Collision", "Performs a basic self-collision algorithm on the shader to avoid (with a low precision) the fur from going inside the mesh"), mTarget.materialProfiles[mTarget.materialProfileIndex].furmatCollision == 1 )?1:0;
                    mTarget.materialProfiles[mTarget.materialProfileIndex].furmatTriplanar = PDEditor_Toggle( new GUIContent("Triplanar Mode", "Render fur using triplanar coordinates generated at runtime instead of the secondary UV channel of this mesh"), mTarget.materialProfiles[mTarget.materialProfileIndex].furmatTriplanar == 1 )?1:0;
                    GUILayout.Space(2);
                    mTarget.materialProfiles[mTarget.materialProfileIndex].furmatForceUV2Grooming = PDEditor_Toggle( new GUIContent("Force Grooming on UV2", "Forces triplanar coordinates to be used for fur projection, using the secondary UV map as coordinates for grooming instead"), mTarget.materialProfiles[mTarget.materialProfileIndex].furmatForceUV2Grooming == 1 )?1:0;
                    
                    if ( mTarget.materialProfiles[mTarget.materialProfileIndex].furmatForceUV2Grooming == 1 ){
                        mTarget.materialProfiles[mTarget.materialProfileIndex].furmatTriplanar = 1;
                    }
                    
                    GUILayout.Space(8);

                    if ( mTarget.materialProfiles[mTarget.materialProfileIndex].furmatTriplanar == 1 ){
                        PDEditor_Slider( new GUIContent("Triplanar Scale", "The scale used for the triplanar projection of the fur, multiplied by the fur UV1 and UV2 channels' sizes"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatTriplanarScale, 0.0f, 1.0f );
                    }


                    GUILayout.Space(8);

                    PDEditor_Popup( new GUIContent("Base Skin Mode","The mode in which the skin color and specularity are controlled for this instance"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatReadBaseSkin, new GUIContent[]{new GUIContent("From Material"), new GUIContent("From Instance")} );
                    PDEditor_Popup( new GUIContent("Fur Settings Mode","The mode in which the fur parameters are controlled for this instance"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatReadBaseFur, new GUIContent[]{new GUIContent("From Material"), new GUIContent("From Instance")} );
                    PDEditor_Popup( new GUIContent("Fur Gen Map","The mode in which the fur noise map is controlled for this instance"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatReadFurNoise, new GUIContent[]{new GUIContent("From Material"), new GUIContent("From Instance")} );

                    GUILayout.Space(8);
                    
                    XFur_CoatingProfile t = null;
                    PDEditor_ObjectField<XFur_CoatingProfile>( new GUIContent("Load Fur Profile","Allows you to assign a pre-made fur profile to easily load existing settings, colors, etc"), ref t, false );

                    if ( t!=null ){
                        mTarget.XFur_LoadFurProfile( t, mTarget.materialProfileIndex );
                    }

                    GUILayout.Space(8);
                    GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
                    if ( GUILayout.Button("Export Fur Settings", GUILayout.MaxWidth(200) ) ){
                        var k = mTarget.XFur_ExportMaterialProfile( mTarget.materialProfiles[mTarget.materialProfileIndex] );
                        if ( k ){
                            var path = EditorUtility.SaveFilePanelInProject( "Save Fur Profile", "New Fur Profile", "asset", "" );
                            AssetDatabase.CreateAsset( k, path );
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                        }
                    }
                    GUILayout.FlexibleSpace();GUILayout.EndHorizontal();
                    GUILayout.Space(8);

                    if ( mTarget.materialProfiles[mTarget.materialProfileIndex].furmatReadBaseSkin != 0 ){
                        GUILayout.Space(4);
                        PDEditor_BeginGroupingBox( "Base Skin" );
                        PDEditor_Color( new GUIContent("Main Color","Final tint to be applied to the skin under the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatBaseColor );
                        
                        if ( !mTarget.materialProfiles[mTarget.materialProfileIndex].furmatGlossSpecular ){
                            PDEditor_Color( new GUIContent("Specular Color","Specular color to be applied to the skin under the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatSpecular );
                        }
                        PDEditor_ObjectField<Texture2D>( new GUIContent("Main Texture", "Texture to be applied to the mesh under the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatBaseTex, false );
                        PDEditor_ObjectField<Texture2D>( new GUIContent("Specular Map", "Base Specular (RGB) and Smoothness (A) map to be used under the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatGlossSpecular, false );
                        GUILayout.Space(4);

                        if ( !mTarget.materialProfiles[mTarget.materialProfileIndex].furmatGlossSpecular ){
                            PDEditor_Slider( new GUIContent("Smoothness","Smoothness to be applied to the skin under the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatSmoothness, 0.0f, 1.0f );
                        }

                        GUILayout.Space(4);

                        PDEditor_ObjectField<Texture2D>( new GUIContent("Normalmap", "The normalmap to be applied to the skin under the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatNormalmap, false );
                        PDEditor_ObjectField<Texture2D>( new GUIContent("Occlusion Map", "The occlusion map to be applied to the skin under the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatOcclusion, false );
                        GUILayout.Space(8);
                        PDEditor_EndGroupingBox();
                    }

                    GUILayout.Space(8);

                    if ( mTarget.materialProfiles[mTarget.materialProfileIndex].furmatReadBaseFur != 0 ){
                        GUILayout.Space(4);
                        PDEditor_BeginGroupingBox( "Fur Settings" );
                        PDEditor_Color( new GUIContent("Fur Color A","Main tint to be applied to the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurColorA );
                        PDEditor_Color( new GUIContent("Fur Color B","Main tint to be applied to the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurColorB );
                        PDEditor_Color( new GUIContent("Fur Rim Color","Main tint to be applied to the fur's rim"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurRim );
                        
                        PDEditor_Color( new GUIContent("Specular Color","Specular color to be applied to the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurSpecular );
                        
                        PDEditor_ObjectField<Texture2D>( new GUIContent("Fur Color Map", "Texture to be applied to the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurColorMap, false );
                        PDEditor_ObjectField<Texture2D>( new GUIContent("Fur Data 0", "Main fur Data map (fur mask, length, thickness and occlusion)"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatData0, false );
                        PDEditor_ObjectField<Texture2D>( new GUIContent("Fur Data 1", "Secondary fur Data map (grooming and stiffness)"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatData1, false );

                        if (mTarget.materialProfiles[mTarget.materialProfileIndex].furmatReadFurNoise > 0 )
                            PDEditor_ObjectField<Texture2D>( new GUIContent("Fur Noise Gen", "Multi-layer noise map used as reference to generate the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurNoiseMap, false );
                        GUILayout.Space(4);

                        PDEditor_Slider( new GUIContent("Fur Occlusion","Shadowing and Occlusion to be applied to the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurOcclusion, 0.0f, 1.0f );
                        PDEditor_Slider( new GUIContent("Fur Smoothness","Smoothness to be applied to the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurSmoothness, 0.0f, 1.0f );
                        PDEditor_Slider( new GUIContent("Fur Length","Length of the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurLength, 0.0f, 4.0f );
                        PDEditor_Slider( new GUIContent("Fur Thickness","Thickness of the fur"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurThickness, 0.0f, 1.0f );
                         PDEditor_Slider( new GUIContent("Fur Rim Power","The power of the rim lighting effect"), ref mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurRimPower, 0.0f, 1.0f );
                        GUILayout.Space(4);

                        mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurUV1 = PDEditor_FloatField( new GUIContent("Fur UV 0 Scale", "Scale of the first fur specific UV channel"), mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurUV1 );
                        mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurUV2 = PDEditor_FloatField( new GUIContent("Fur UV 1 Scale", "Scale of the second fur specific UV channel"), mTarget.materialProfiles[mTarget.materialProfileIndex].furmatFurUV2 );

                        Vector3 furDir = mTarget.materialProfiles[mTarget.materialProfileIndex].furmatDirection;
                        int groomAlgorithm = (int)mTarget.materialProfiles[mTarget.materialProfileIndex].furmatDirection.w;

                        PDEditor_Popup( new GUIContent("Grooming Algorithm","The grooming algorithm to use when adding fur direction to this model"), ref groomAlgorithm, new GUIContent[]{new GUIContent("Original","The original grooming algorithm adds a bit of length to the fur as you groom allowing more creativity. Please use small fur direction values for best results"), new GUIContent("Accurate","The new algorithm for grooming is more accurate, bending the fur without adding any length. It allows for a more controlled, predictable grooming. Please make sure to use high fur direction values for best results")});
                        PDEditor_Vector3( new GUIContent("Fur Direction"), ref furDir );

                        mTarget.materialProfiles[mTarget.materialProfileIndex].furmatDirection = new Vector4(furDir.x,furDir.y,furDir.z,groomAlgorithm);



                        GUILayout.Space(8);
                        PDEditor_EndGroupingBox();

                        GUILayout.Space(8);
                    }
                    
                }
                else{
                    GUILayout.Space(6);

                    GUILayout.BeginHorizontal();GUILayout.Space(12);
                    EditorGUILayout.HelpBox("Per Instance parameters are not supported on models with more than 1 fur based material on Unity versions older than Unity 2018.1", MessageType.Warning );
                    GUILayout.Space(12);GUILayout.EndHorizontal();

                    GUILayout.Space(6);
                }

            }
            else if ( mTarget.materialProfiles[mTarget.materialProfileIndex].furmatType == 1 ){

                GUILayout.BeginHorizontal();GUILayout.Space(12);
                EditorGUILayout.HelpBox("This is a legacy XFur material. It must be upgraded. Please be warned that upgrading a material is a one time operation. It won't change any of the textures but will modify the material's properties", MessageType.Warning );
                GUILayout.Space(12);GUILayout.EndHorizontal();

                GUILayout.Space(12);

                GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
                if ( GUILayout.Button("Auto-Upgrade Material",GUILayout.MaxWidth(200))){
                    mTarget.AutoUpgradeMaterial( mTarget.materialProfiles[ mTarget.materialProfileIndex ] );
                }
                GUILayout.FlexibleSpace();GUILayout.EndHorizontal();
            }
            else{
                GUILayout.BeginHorizontal();GUILayout.Space(12);
                EditorGUILayout.HelpBox("This material is not a fur enabled material, no settings will be available for it.", MessageType.Warning );
                GUILayout.Space(12);GUILayout.EndHorizontal();                
            }
            GUILayout.Space(8);
        }
        PDEditor_EndFold();


        if ( PDEditor_BeginFold( mTarget.coatingModule!=null?"XFur - "+mTarget.coatingModule.ModuleName:"XFur - Coating Module (ERROR)", ref folds[1] ) ){
            mTarget.coatingModule.Module_UI( );
        }
        PDEditor_EndFold();


        if ( PDEditor_BeginFold( mTarget.lodModule!=null?"XFur - "+mTarget.lodModule.ModuleName:"XFur - Lod Module (ERROR)", ref folds[2] ) ){
            mTarget.lodModule.Module_UI( );
        }
        PDEditor_EndFold();


        if ( PDEditor_BeginFold( mTarget.physicsModule!=null?"XFur - "+mTarget.physicsModule.ModuleName:"XFur - Physics Module (ERROR)", ref folds[3] ) ){
            mTarget.physicsModule.Module_UI( );
        }
        PDEditor_EndFold();


        if ( PDEditor_BeginFold( mTarget.fxModule!=null?"XFur - "+mTarget.fxModule.ModuleName:"XFur - FX Module (ERROR)", ref folds[4] ) ){
            mTarget.fxModule.Module_UI( );
        }
        PDEditor_EndFold();

        GUILayout.Space(8);

        }

        var tempStyle = new GUIStyle();
        tempStyle.normal.textColor = new Color(0.75f,0.75f,0.75f);
        tempStyle.fontSize = 9;
        tempStyle.fontStyle = FontStyle.Italic;
        GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
        GUILayout.Label("PIDI - XFur Studio™. Version 1.4", tempStyle );
        GUILayout.FlexibleSpace();GUILayout.EndHorizontal();

		GUILayout.Space(8);
        GUILayout.EndVertical();GUILayout.Space(8);GUILayout.EndHorizontal();


        GUI.skin = tSkin;
    }


    #region GENERIC PIDI EDITOR FUNCTIONS

    public static GUISkin PDEditor_GetCustomGUI(  ){
        if ( !pidiSkin ){
            var basePath = AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets("XFur_System")[0]);
            pidiSkin = (GUISkin)AssetDatabase.LoadAssetAtPath(basePath.Replace("XFur_SystemEditor.cs","PIDI_EditorSkin.guiskin"), typeof(GUISkin));
        }
        return pidiSkin;
    }

	public static bool PDEditor_BeginFold( string label, ref AnimBool fold ){
            if ( GUILayout.Button(label, pidiSkin.button ) ){
                fold.target = !fold.target;
            }

            var b = EditorGUILayout.BeginFadeGroup( fold.faded );
            if ( b ){ 
                GUILayout.Space(8);}
            return b;
    }


    public static bool PDEditor_BeginFold( string label, ref bool fold ){
            if ( GUILayout.Button(label, pidiSkin.button ) ){
                fold  = !fold;
            }

            var b = EditorGUILayout.BeginFadeGroup( fold?1:0 );
            if ( b ){ 
                GUILayout.Space(8);}
            return b;
    }


    public static void PDEditor_EndFold( ){
        EditorGUILayout.EndFadeGroup();
    }

	public static void PDEditor_Toggle( GUIContent label, ref bool value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = GUILayout.Toggle( value, "", GUILayout.Width(16) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
    }

    public static bool PDEditor_Toggle( GUIContent label, bool value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = GUILayout.Toggle( value, "", GUILayout.Width(16) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            return value;
    }

        public static void PDEditor_TextField( GUIContent label, ref string value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = GUILayout.TextField( value, GUILayout.MinWidth(64), GUILayout.MaxWidth(250) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public static Enum PDEditor_EnumPopup( GUIContent label, Enum value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            var x = EditorGUILayout.EnumPopup( value, pidiSkin.button, GUILayout.MinWidth(64), GUILayout.MaxWidth(120) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            return x;
        }


        public static void PDEditor_ObjectField<T> ( GUIContent label, ref T value, bool fromScene )where T:UnityEngine.Object{
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(180));
            GUILayout.FlexibleSpace();
            var t = GUI.skin;
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            value = (T)EditorGUILayout.ObjectField( value, typeof(T), fromScene, GUILayout.MinWidth(64), GUILayout.MaxWidth(180) );
            GUI.skin = t;
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }

        public static T PDEditor_ObjectField<T> ( GUIContent label, T value, bool fromScene )where T:UnityEngine.Object{
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(180));
            GUILayout.FlexibleSpace();
            var t = GUI.skin;
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            value = (T)EditorGUILayout.ObjectField( value, typeof(T), fromScene, GUILayout.MinWidth(64), GUILayout.MaxWidth(180) );
            GUI.skin = t;
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            return value;
        }

        public static void PDEditor_Slider( GUIContent label, ref float value, float min, float max ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Slider( value, min, max, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public static float PDEditor_Slider( GUIContent label, float value, float min, float max ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Slider( value, min, max, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            return value;
        }


		public static void PDEditor_IntSlider( GUIContent label, ref int value, int min, int max ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.MaxWidth(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.IntSlider( value, min, max, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public static void PDEditor_Vector2( GUIContent label, ref Vector2 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector2Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public static void PDEditor_Vector3( GUIContent label, ref Vector3 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector3Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public static void PDEditor_Vector4( GUIContent label, ref Vector4 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector4Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public static Vector4 PDEditor_Vector4( GUIContent label, Vector4 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector4Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
            return value;
        }


        public static void PDEditor_Color( GUIContent label, ref Color value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            value = EditorGUILayout.ColorField( "", value, GUILayout.MaxWidth(64) );
            GUI.skin = pidiSkin;
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public static Color PDEditor_Color( GUIContent label, Color value, int finalWidth = 512 ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            value = EditorGUILayout.ColorField( "", value, GUILayout.MaxWidth(finalWidth) );
            GUI.skin = pidiSkin;
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
            return value;
        }

        public static void PDEditor_Popup( GUIContent label, ref int value, params GUIContent[] items ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Popup( value, items, pidiSkin.button, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public static void PDEditor_Popup( string label, ref int value, params string[] items ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.MaxWidth(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Popup( value, items, pidiSkin.button, GUILayout.MinWidth(100), GUILayout.MaxWidth(180) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public static void PDEditor_CenteredLabel( string label ){
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        public static void PDEditor_FloatField( GUIContent label, ref float value, bool overZero = true ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.FloatField("", value, pidiSkin.textField, GUILayout.MinWidth(25), GUILayout.MaxWidth(64), GUILayout.MinHeight(20) );

            if ( overZero )
                value = Mathf.Max( value, 0 );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }

        public static float PDEditor_FloatField( GUIContent label, float value, bool overZero = true ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.FloatField("", value, pidiSkin.textField, GUILayout.MinWidth(25), GUILayout.MaxWidth(64), GUILayout.MinHeight(20) );

            if ( overZero )
                value = Mathf.Max( value, 0 );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            return value;
        }

        

        public static void PDEditor_IntField( GUIContent label, ref int value, bool overZero = true ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.IntField("", value, pidiSkin.textField, GUILayout.MinWidth(25), GUILayout.MaxWidth(64), GUILayout.MinHeight(20) );

            if ( overZero )
                value = Mathf.Max( value, 0 );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public static void PDEditor_BeginGroupingBox( string label ){
        GUILayout.BeginHorizontal();GUILayout.Space(12);
        GUILayout.BeginVertical( label, pidiSkin.customStyles[1] );
        GUILayout.Space(30);
        }


        public static void PDEditor_EndGroupingBox( ){
        GUILayout.Space(4);
        GUILayout.EndVertical();
        GUILayout.Space(12);
        GUILayout.EndHorizontal();
        }


		public static void PDEditor_LayerMaskField ( GUIContent label, ref LayerMask selected) {
		
		List<string> layers = null;
		string[] layerNames = null;
		
		if (layers == null) {
			layers = new List<string>();
			layerNames = new string[4];
		} else {
			layers.Clear ();
		}
		
		int emptyLayers = 0;
		for (int i=0;i<32;i++) {
			string layerName = LayerMask.LayerToName (i);
			
			if (layerName != "") {
				
				for (;emptyLayers>0;emptyLayers--) layers.Add ("Layer "+(i-emptyLayers));
				layers.Add (layerName);
			} else {
				emptyLayers++;
			}
		}
		
		if (layerNames.Length != layers.Count) {
			layerNames = new string[layers.Count];
		}
		for (int i=0;i<layerNames.Length;i++) layerNames[i] = layers[i];
		
		GUILayout.BeginHorizontal();
        GUILayout.Space(12);
        GUILayout.Label(label, GUILayout.Width(175));
        GUILayout.FlexibleSpace();

		selected.value =  EditorGUILayout.MaskField (selected.value,layerNames, GUILayout.MaxWidth(256) );
		
		GUILayout.Space(4);
        GUILayout.EndHorizontal();
		
		}


	#endregion

}

}