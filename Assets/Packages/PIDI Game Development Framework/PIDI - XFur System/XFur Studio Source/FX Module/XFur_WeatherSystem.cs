using UnityEngine;

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

namespace XFurStudio{

    [ExecuteInEditMode]
    public class XFur_WeatherSystem:MonoBehaviour{

        public bool wind;
        public bool snow;
        public bool rain;

        public Vector3 windDir = Vector3.forward;
        public float windIntensity = 0.2f;
        public float windTurbulence = 0.65f;
        public float snowIntensity;
        public float rainIntensity;

        #if UNITY_EDITOR
        private Mesh arrowMesh;
        #endif
        public static Vector3 snowDirection;
        public static Vector3 rainDirection;
        public static float snowStrength;
        public static float rainStrength;

        public void Update(){
            windDir = transform.forward;

            rainDirection = Vector3.Lerp(Vector3.down, windDir, windIntensity );
            rainStrength = rain?rainIntensity:0;

            snowDirection = Vector3.Lerp(Vector3.down, windDir, windIntensity/2 );
            snowStrength = snow?snowIntensity:0;

            Shader.SetGlobalFloat( "_WindSpeed", windTurbulence );
		    Shader.SetGlobalVector( "_WindDirection", windDir*windIntensity );
        }


        void OnDrawGizmos(){
        #if UNITY_EDITOR
		if ( arrowMesh ){
			Gizmos.DrawWireMesh(arrowMesh,transform.position,transform.rotation, Vector3.one*windIntensity);
		}
		else{
			
			arrowMesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(UnityEditor.AssetDatabase.GUIDToAssetPath( UnityEditor.AssetDatabase.FindAssets( "Arrow_Model" )[0] ) );
		
		}
        #endif
	}

    }



    #if UNITY_EDITOR

    [CustomEditor(typeof(XFur_WeatherSystem))]
    public class XFur_WeatherEditor:Editor{

        public AnimBool[] folds;
        private GUISkin pidiSkin;

        private void OnEnable() {
            folds = new AnimBool[3];
            for ( int i = 0; i < 3; i++ ){
                folds[i] = new AnimBool();
                folds[i].valueChanged.AddListener(Repaint);
            }
        }

        public override void OnInspectorGUI(){

        var mTarget = (XFur_WeatherSystem)target;

        Undo.RecordObject(mTarget,"WeatherSystem"+mTarget.GetInstanceID());

        if ( !pidiSkin )
            PDEditor_GetCustomGUI(  );

        var tSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        GUI.skin = pidiSkin;

        GUILayout.BeginHorizontal();GUILayout.BeginVertical( pidiSkin.box );
        GUILayout.Space(8);



        if ( PDEditor_BeginFold("Weather Settings", ref folds[0] ) ){
            
            PDEditor_Toggle( new GUIContent("Enable Snow","Enables snow on this scene"), ref mTarget.snow );
            PDEditor_Toggle( new GUIContent("Enable Rain","Enables rain on this scene"), ref mTarget.rain );

            GUILayout.Space(12);
            PDEditor_BeginGroupingBox("Wind Settings");
            PDEditor_Slider(new GUIContent("Wind Turbulence","The global turbulence of the wind for this scene"), ref mTarget.windTurbulence, 0.0f, 1.0f );
            PDEditor_Slider(new GUIContent("Wind Strength","The global strength of the wind for this scene"), ref mTarget.windIntensity, 0.0f, 10.0f );
            GUILayout.Space(8);
            PDEditor_EndGroupingBox();

            GUILayout.Space(12);

            if ( mTarget.snow ){
                PDEditor_BeginGroupingBox("Snow Settings");
                PDEditor_Slider(new GUIContent("Snow Intensity","The global intensity of the snow for this scene"), ref mTarget.snowIntensity, 0.0f, 1.0f );
                GUILayout.Space(8);
                PDEditor_EndGroupingBox();
                GUILayout.Space(12);
            }

            if ( mTarget.rain ){
                PDEditor_BeginGroupingBox("Rain Settings");
                PDEditor_Slider(new GUIContent("Rain Intensity","The global intensity of the rain for this scene"), ref mTarget.rainIntensity, 0.0f, 1.0f );
                GUILayout.Space(8);
                PDEditor_EndGroupingBox();
                GUILayout.Space(12);
            }

        }
        PDEditor_EndFold();


        GUILayout.Space(10);
        var tempStyle = new GUIStyle();
        tempStyle.normal.textColor = new Color(0.75f,0.75f,0.75f);
        tempStyle.fontSize = 9;
        tempStyle.fontStyle = FontStyle.Italic;
        GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
        GUILayout.Label("PIDI - XFur Studio™. Version 1.2", tempStyle );
        GUILayout.FlexibleSpace();GUILayout.EndHorizontal();

		GUILayout.Space(8);
        GUILayout.EndVertical();GUILayout.Space(8);GUILayout.EndHorizontal();


        GUI.skin = tSkin;

        }



    #region GENERIC PIDI EDITOR FUNCTIONS

    public GUISkin PDEditor_GetCustomGUI(  ){
        if ( !pidiSkin ){
            var basePath = AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets("XFur_System")[0]);
            pidiSkin = (GUISkin)AssetDatabase.LoadAssetAtPath(basePath.Replace("XFur_SystemEditor.cs","PIDI_EditorSkin.guiskin"), typeof(GUISkin));
        }
        return pidiSkin;
    }

	public bool PDEditor_BeginFold( string label, ref AnimBool fold ){
            if ( GUILayout.Button(label, pidiSkin.button ) ){
                fold.target = !fold.target;
            }

            var b = EditorGUILayout.BeginFadeGroup( fold.faded );
            if ( b ){ 
                GUILayout.Space(8);}
            return b;
    }


    public bool PDEditor_BeginFold( string label, ref bool fold ){
            if ( GUILayout.Button(label, pidiSkin.button ) ){
                fold  = !fold;
            }

            var b = EditorGUILayout.BeginFadeGroup( fold?1:0 );
            if ( b ){ 
                GUILayout.Space(8);}
            return b;
    }


        public void PDEditor_EndFold( ){
            EditorGUILayout.EndFadeGroup();
        }

	    public void PDEditor_Toggle( GUIContent label, ref bool value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = GUILayout.Toggle( value, "", GUILayout.Width(16) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }

        public bool PDEditor_Toggle( GUIContent label, bool value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = GUILayout.Toggle( value, "", GUILayout.Width(16) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            return value;
        }

        public void PDEditor_TextField( GUIContent label, ref string value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = GUILayout.TextField( value, GUILayout.MinWidth(64), GUILayout.MaxWidth(250) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public Enum PDEditor_EnumPopup( GUIContent label, Enum value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            var x = EditorGUILayout.EnumPopup( value, pidiSkin.button, GUILayout.MinWidth(64), GUILayout.MaxWidth(120) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            return x;
        }


        public void PDEditor_ObjectField<T> ( GUIContent label, ref T value, bool fromScene )where T:UnityEngine.Object{
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

        public T PDEditor_ObjectField<T> ( GUIContent label, T value, bool fromScene )where T:UnityEngine.Object{
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

        public void PDEditor_Slider( GUIContent label, ref float value, float min, float max ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Slider( value, min, max, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public float PDEditor_Slider( GUIContent label, float value, float min, float max ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Slider( value, min, max, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            return value;
        }


		public void PDEditor_IntSlider( GUIContent label, ref int value, int min, int max ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.MaxWidth(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.IntSlider( value, min, max, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public void PDEditor_Vector2( GUIContent label, ref Vector2 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector2Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_Vector3( GUIContent label, ref Vector3 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector3Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_Vector4( GUIContent label, ref Vector4 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector4Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }


        public void PDEditor_Color( GUIContent label, ref Color value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            value = EditorGUILayout.ColorField( "", value, GUILayout.MinWidth(100) );
            GUI.skin = pidiSkin;
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public Color PDEditor_Color( GUIContent label, Color value, int finalWidth = 512 ){
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

        public void PDEditor_Popup( GUIContent label, ref int value, params GUIContent[] items ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Popup( value, items, pidiSkin.button, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public void PDEditor_Popup( string label, ref int value, params string[] items ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.MaxWidth(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Popup( value, items, pidiSkin.button, GUILayout.MinWidth(100), GUILayout.MaxWidth(180) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public void PDEditor_CenteredLabel( string label ){
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        public void PDEditor_FloatField( GUIContent label, ref float value, bool overZero = true ){
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

        public float PDEditor_FloatField( GUIContent label, float value, bool overZero = true ){
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

        

        public void PDEditor_IntField( GUIContent label, ref int value, bool overZero = true ){
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


        public void PDEditor_BeginGroupingBox( string label ){
        GUILayout.BeginHorizontal();GUILayout.Space(12);
        GUILayout.BeginVertical( label, pidiSkin.customStyles[1] );
        GUILayout.Space(30);
        }


        public void PDEditor_EndGroupingBox( ){
        GUILayout.Space(4);
        GUILayout.EndVertical();
        GUILayout.Space(12);
        GUILayout.EndHorizontal();
        }


		public void PDEditor_LayerMaskField ( GUIContent label, ref LayerMask selected) {
		
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

    #endif

}