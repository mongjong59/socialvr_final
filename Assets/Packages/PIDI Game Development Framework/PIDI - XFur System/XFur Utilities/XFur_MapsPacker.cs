/*
XFur Studio™ - XFur Painter™
XFur Studio and XFur Painter are trademarks of Jorge Pinal Negrete. Copyright© 2015-2018, Jorge Pinal Negrete. All Rights Reserved.
*/

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


public class XFurMapPacker:EditorWindow{

    public Texture2D redMap, greenMap, blueMap, alphaMap;
    public int mapKind;
    public int rMode,gMode,bMode,aMode;
    public bool throwWarning;
    public string warning;

    [MenuItem("Window/XFur Studio/Utilities/XFur Map Generator")]
    static void Init(){
        XFurMapPacker window = (XFurMapPacker)EditorWindow.GetWindow(typeof(XFurMapPacker));
        window.Show();
    }

    void OnGUI(){

        GUILayout.BeginHorizontal();

        GUILayout.Space(24);

        GUILayout.BeginVertical();

        GUILayout.Space(16);
        mapKind = EditorGUILayout.Popup( "XFur Data Type", mapKind, new string[]{"Basic XFur Data Map", "XFur Grooming Map"} );

        GUILayout.Space(12);

        EditorGUILayout.LabelField(mapKind==0?"Fur Mask":"X Axis Grooming (gray = No grooming)");

        GUILayout.Space(8);


        GUILayout.EndVertical();

        GUILayout.Space(24);

        GUILayout.EndHorizontal();



        GUILayout.BeginHorizontal();

        GUILayout.Space(32);

        redMap = (Texture2D)EditorGUILayout.ObjectField( redMap, typeof(Texture2D), false );
        
        GUILayout.Space(12);

        rMode = EditorGUILayout.Popup( "Read Data From", rMode, new string[]{"Red Channel","Green Channel","Blue Channel","Alpha Channel"} );

        GUILayout.Space(32);

        GUILayout.EndHorizontal();
        
        if ( redMap){
            var t = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(redMap));
            if ( !t.isReadable ){
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();GUILayout.Space(100);
                EditorGUILayout.HelpBox("Texture must be marked as readable",MessageType.Warning);
                GUILayout.Space(100);
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();GUILayout.Space(24);
        GUILayout.BeginVertical();

        EditorGUILayout.LabelField(mapKind==0?"Fur Length":"Y Axis Grooming (gray = No grooming)");

        GUILayout.EndVertical();
        GUILayout.Space(24);
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();

        GUILayout.Space(32);

        greenMap = (Texture2D)EditorGUILayout.ObjectField( greenMap, typeof(Texture2D), false );

        GUILayout.Space(12);

        gMode = EditorGUILayout.Popup( "Read Data From", gMode, new string[]{"Red Channel","Green Channel","Blue Channel","Alpha Channel"} );

        GUILayout.Space(32);

        GUILayout.EndHorizontal();

        if ( greenMap){
            var t = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(greenMap));
            if ( !t.isReadable ){
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();GUILayout.Space(100);
                EditorGUILayout.HelpBox("Texture must be marked as readable",MessageType.Warning);
                GUILayout.Space(100);
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();GUILayout.Space(24);
        GUILayout.BeginVertical();

        EditorGUILayout.LabelField(mapKind==0?"Fur Occlusion":"Z Axis Grooming (gray = No grooming)");

        GUILayout.EndVertical();
        GUILayout.Space(24);
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        GUILayout.BeginHorizontal();

        GUILayout.Space(32);

        blueMap = (Texture2D)EditorGUILayout.ObjectField( blueMap, typeof(Texture2D), false );

        GUILayout.Space(12);

        bMode = EditorGUILayout.Popup( "Read Data From", bMode, new string[]{"Red Channel","Green Channel","Blue Channel","Alpha Channel"} );

        GUILayout.Space(32);

        GUILayout.EndHorizontal();

        if ( blueMap){
            var t = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(blueMap));
            if ( !t.isReadable ){
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();GUILayout.Space(100);
                EditorGUILayout.HelpBox("Texture must be marked as readable",MessageType.Warning);
                GUILayout.Space(100);
                GUILayout.EndHorizontal();
            }
        }
        


    
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();GUILayout.Space(24);
            GUILayout.BeginVertical();

            EditorGUILayout.LabelField(mapKind==0?"Fur Thickness":"Fur Stiffness (1=No stiffness)");

            GUILayout.EndVertical();
            GUILayout.Space(24);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();

            GUILayout.Space(32);

            alphaMap = (Texture2D)EditorGUILayout.ObjectField( alphaMap, typeof(Texture2D), false );

            GUILayout.Space(12);

            aMode = EditorGUILayout.Popup( "Read Data From", aMode, new string[]{"Red Channel","Green Channel","Blue Channel","Alpha Channel"} );

            GUILayout.Space(32);

            GUILayout.EndHorizontal();

            if ( alphaMap){
            var t = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(alphaMap));
            if ( !t.isReadable ){
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();GUILayout.Space(100);
                EditorGUILayout.HelpBox("Texture must be marked as readable",MessageType.Warning);
                GUILayout.Space(100);
                GUILayout.EndHorizontal();
            }
        }
        

        GUILayout.Space(24);

        GUILayout.BeginHorizontal();GUILayout.Space(100);
        GUILayout.BeginVertical();
        if ( GUILayout.Button("Generate Map") ){
            throwWarning = false;

            if ( redMap == null && greenMap == null && blueMap == null && alphaMap == null ){
                throwWarning = true;
                warning = "At least one texture map slot must be assigned";
            }
            else{

                Vector2 rSize,gSize,bSize,aSize;
                rSize = redMap?new Vector2(redMap.width,redMap.height):Vector2.zero;
                gSize = greenMap?new Vector2(greenMap.width,greenMap.height):Vector2.zero;
                bSize = blueMap?new Vector2(blueMap.width,blueMap.height):Vector2.zero;
                aSize = alphaMap?new Vector2(alphaMap.width,alphaMap.height):Vector2.zero;
            
                if ( rSize == gSize || (rSize==Vector2.zero||gSize==Vector2.zero) ){
                    var tSize = Vector2.Max(rSize,gSize);
                    if ( tSize == bSize || ( tSize==Vector2.zero||bSize==Vector2.zero ) ){
                        tSize = Vector2.Max(tSize,bSize);
                        if ( tSize == aSize || ( tSize==Vector2.zero||aSize==Vector2.zero ) ){
                            
                            tSize = Vector2.Max(tSize,aSize);
                            var t = GenerateXFurMap( tSize );

                            var path = EditorUtility.SaveFilePanelInProject("Export Texture","New Fur Mask.png","png", "Export the current fur mask" );
                            
                            if (path.Length != 0){
                                var pngData = t.EncodeToPNG();
                                if (pngData != null)
                                    System.IO.File.WriteAllBytes(path, pngData);
                                    AssetDatabase.Refresh();
                            }

                            DestroyImmediate(t);
                        }
                        else{
                            throwWarning = true;
                            warning = "All Texture Maps must have the same size for the conversion tool to work";
                        }
                    }
                    else{
                        throwWarning = true;
                        warning = "All Texture Maps must have the same size for the conversion tool to work";
                        }
                }
                else{
                    throwWarning = true;
                    warning = "All Texture Maps must have the same size for the conversion tool to work";
                }
            }


        }
        GUILayout.Space(12);
        if ( throwWarning)
            EditorGUILayout.HelpBox( warning, MessageType.Warning );
        GUILayout.EndVertical();
        GUILayout.Space(100);GUILayout.EndHorizontal();
        

    }


    public Texture2D GenerateXFurMap( Vector2 size ){
        
        Texture2D outputMap = new Texture2D((int)size.x,(int)size.y);
        Color[] finalColor = new Color[(int)size.x*(int)size.y];

        if ( redMap ){
            var c = redMap.GetPixels();
            for ( int i = 0; i < c.Length; i++ ){
                switch( rMode ){
                    case 0:
                    finalColor[i].r = c[i].r;
                    break;

                    case 1:
                    finalColor[i].r = c[i].g;
                    break;

                    case 2:
                    finalColor[i].r = c[i].b;
                    break;

                    case 3:
                    finalColor[i].r = c[i].a;
                    break;
                }
            }
        }

        if ( greenMap ){
            var c = greenMap.GetPixels();
            for ( int i = 0; i < c.Length; i++ ){
                switch( gMode ){
                    case 0:
                    finalColor[i].g = c[i].r;
                    break;

                    case 1:
                    finalColor[i].g = c[i].g;
                    break;

                    case 2:
                    finalColor[i].g = c[i].b;
                    break;

                    case 3:
                    finalColor[i].g = c[i].a;
                    break;
                }
            }
        }

        if ( blueMap ){
            var c = blueMap.GetPixels();
            for ( int i = 0; i < c.Length; i++ ){
                switch( bMode ){
                    case 0:
                    finalColor[i].b = c[i].r;
                    break;

                    case 1:
                    finalColor[i].b = c[i].g;
                    break;

                    case 2:
                    finalColor[i].b = c[i].b;
                    break;

                    case 3:
                    finalColor[i].b = c[i].a;
                    break;
                }
            }
        }

        if ( alphaMap ){
            var c = alphaMap.GetPixels();
            for ( int i = 0; i < c.Length; i++ ){
                switch( aMode ){
                    case 0:
                    finalColor[i].a = c[i].r;
                    break;

                    case 1:
                    finalColor[i].a = c[i].g;
                    break;

                    case 2:
                    finalColor[i].a = c[i].b;
                    break;

                    case 3:
                    finalColor[i].a = c[i].a;
                    break;
                }
            }
        }

        outputMap.SetPixels(finalColor);
        outputMap.Apply();

        return outputMap;

    }

}
#endif