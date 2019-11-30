/*
XFur Studio™ - XFur Database Module
Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved


This module stores the list of all shaders included with the system and is updated with each new version.
It is automatically linked to the XFur System component.

*/


using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XFurStudio{

    public class XFur_CoatingProfile:ScriptableObject{

        public XFur_MaterialProperties profile;

        public Color furColorA_Min = Color.white;
        public Color furColorA_Max = Color.white;
        public Color furColorB_Min = Color.white;
        public Color furColorB_Max = Color.white;
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(XFur_CoatingProfile))]
    public class XFur_CoatingProfileEditor:Editor{
        public override void OnInspectorGUI(){
            var mTarget = (XFur_CoatingProfile)target;

            GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
            GUILayout.Label( "Fur Color A Variation" );
            GUILayout.FlexibleSpace();GUILayout.EndHorizontal();

                mTarget.furColorA_Min = EditorGUILayout.ColorField( new GUIContent("From Color"), mTarget.furColorA_Min );
                mTarget.furColorA_Max = EditorGUILayout.ColorField( new GUIContent("To Color"), mTarget.furColorA_Max );

            GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
            GUILayout.Label( "Fur Color B Variation" );
            GUILayout.FlexibleSpace();GUILayout.EndHorizontal();

                mTarget.furColorB_Min = EditorGUILayout.ColorField( new GUIContent("From Color"), mTarget.furColorB_Min );
                mTarget.furColorB_Max = EditorGUILayout.ColorField( new GUIContent("To Color"), mTarget.furColorB_Max );

        }
    }
    #endif

}