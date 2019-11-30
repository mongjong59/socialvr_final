/*
XFur Studio™ v 1.2

You cannot sell, redistribute, share nor make public this code, even modified, through any means on any platform.
Modifications are allowed only for your own use and to make this product suit better your project's needs.
These modifications may not be redistributed, sold or shared in any way.

For more information, contact us at contact@irreverent-software.com

Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved.
*/

using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

namespace XFurStudio{
    [System.Serializable]
    public class XFur_PhysicsModule:XFur_SystemModule {
        
                
        [SerializeField]protected bool useBasicPhysics;
        [SerializeField]protected bool useAngularPhysics = true;
        [SerializeField]protected float inertia = 0.3f;
        [SerializeField]protected int iterations = 45;
        [SerializeField]protected float physicsSensitivity = 0.5f;
        [SerializeField]protected float physicsStrength = 0.2f;
        [SerializeField]protected float gravityStrength = 1;
 
        private Vector4[] vectorialPhysics = new Vector4[128];
        private Vector4[] angularPhysics = new Vector4[128];
        private Quaternion[] buffRotations = new Quaternion[129];
        private Vector3[] buffPositions = new Vector3[129];
        private Vector3[] targetAngles = new Vector3[129];
        private Vector3[] movingAngles = new Vector3[129];
        private Vector3[] targetVectors = new Vector3[129];
        private Vector3[] movingVector = new Vector3[129];

        private float timer;
        private Transform[] bones;

        public override void Module_Start(XFur_System owner){
            systemOwner = owner;
            buffPositions[0] = systemOwner.transform.position;
            buffRotations[0] = systemOwner.transform.rotation;

            if ( systemOwner.GetComponent<SkinnedMeshRenderer>() ){
                bones = systemOwner.GetComponent<SkinnedMeshRenderer>().bones;

                for ( int i = 0; i < bones.Length; i++ ){
                    buffPositions[i] = bones[i].position;
                    buffRotations[i] = bones[i].rotation;
                }
            }
        }

        public override void Module_Execute(){
            if ( !systemOwner || !systemOwner.database )
                return; 

            var lTimer = iterations*1.0f;
            if ( systemOwner.lodModule.State == XFurModuleState.Enabled ){
                if ( systemOwner.lodModule.XFur_LODLevel == 2 ){
                    lTimer = iterations*0.5f;
                }
                
                if ( systemOwner.lodModule.XFur_LODLevel == 1 ){
                    lTimer = iterations*0.25f;
                }
                if ( systemOwner.lodModule.XFur_LODLevel == 0 ){
                    lTimer = iterations*0.05f;
                }
            }
            var tIteration = 1.0f/lTimer;

            if ( Application.isPlaying && State == XFurModuleState.Enabled && Time.timeSinceLevelLoad > timer ){
                
                Vector3 tDirection; 
                Vector3 tAngle; 
                

                if ( systemOwner.GetComponent<MeshRenderer>() ){
                    tDirection = (systemOwner.transform.position)-buffPositions[0];
                        buffPositions[0] = systemOwner.transform.position;
                        targetVectors[0] = Vector3.ClampMagnitude( targetVectors[0]+tDirection*6*physicsSensitivity, physicsStrength);
                        
                        if ( systemOwner.lodModule.XFur_LODLevel > 1 && !useBasicPhysics ){
                            if ( useAngularPhysics ){
                                tAngle = (buffRotations[0]*Quaternion.Inverse(systemOwner.transform.rotation)).eulerAngles/180.0f;
                                buffRotations[0] = systemOwner.transform.rotation;
                                targetAngles[0] = Vector3.ClampMagnitude( targetAngles[0]+tAngle*6*physicsSensitivity, physicsStrength);
                                angularPhysics[0] = Vector3.SmoothDamp( (Vector3)angularPhysics[0], targetAngles[0], ref movingAngles[0], 0.05f, Mathf.Infinity, tIteration );

                                if ( ( (Vector3)angularPhysics[0]-targetAngles[0] ).sqrMagnitude < 0.0005f ){
                                    targetAngles[0] *= -0.85f*inertia;                        
                                }
                            }
                            else{
                                angularPhysics[0] = Vector3.zero;
                            }
                            
                            vectorialPhysics[0] = Vector3.SmoothDamp( (Vector3)vectorialPhysics[0], targetVectors[0], ref movingVector[0], 0.05f, Mathf.Infinity, tIteration );
                            
                            if ( ( (Vector3)vectorialPhysics[0]-targetVectors[0] ).sqrMagnitude < 0.0005f ){
                                targetVectors[0] *= -0.85f*inertia;
                                targetVectors[0].y = 0;
                            }                            
                        
                        }
                        else if (systemOwner.lodModule.XFur_LODLevel > 0){
                            if ( useAngularPhysics ){
                                tAngle = (buffRotations[0]*Quaternion.Inverse(systemOwner.transform.rotation)).eulerAngles/180.0f;
                                buffRotations[0] = systemOwner.transform.rotation;
                                targetAngles[0] = Vector3.ClampMagnitude( targetAngles[0]+tAngle*6*physicsSensitivity, physicsStrength);
                                angularPhysics[0] = Vector4.MoveTowards( angularPhysics[0], targetAngles[0], physicsStrength*tIteration*20 );
                            }
                            else{
                                buffRotations[0] = systemOwner.transform.rotation;
                                angularPhysics[0] = Vector3.zero;
                            }
                            vectorialPhysics[0] = Vector4.MoveTowards( vectorialPhysics[0], targetVectors[0], physicsStrength*tIteration*20 );
                        }
                        else if ( systemOwner.lodModule.XFur_LODLevel == 0 ) {
                            buffRotations[0] = systemOwner.transform.rotation;
                            buffPositions[0] = systemOwner.transform.position;
                            angularPhysics[0] = vectorialPhysics[0] = Vector4.zero;

                        }
                    
                }
                else if ( systemOwner.GetComponent<SkinnedMeshRenderer>() ){
                    
                    for ( int i = 0; i < bones.Length; i++ ){
                        
                        tDirection = (bones[i].position)-buffPositions[i];
                        buffPositions[i] = bones[i].position;
                        targetVectors[i] = Vector3.ClampMagnitude( targetVectors[i]+tDirection*6*physicsSensitivity, physicsStrength);
                        
                        if ( systemOwner.lodModule.XFur_LODLevel > 1 && !useBasicPhysics ){
                            if ( useAngularPhysics ){
                                tAngle = (buffRotations[i]*Quaternion.Inverse(bones[i].rotation)).eulerAngles/180.0f;
                                buffRotations[i] = bones[i].rotation;
                                targetAngles[i] = Vector3.ClampMagnitude( targetAngles[i]+tAngle*6*physicsSensitivity, physicsStrength);
                                angularPhysics[i] = Vector3.SmoothDamp( (Vector3)angularPhysics[i], targetAngles[i], ref movingAngles[i], 0.1f, Mathf.Infinity, tIteration );

                                if ( ( (Vector3)angularPhysics[i]-targetAngles[i] ).sqrMagnitude < 0.0005f ){
                                    targetAngles[i] *= -0.85f*inertia;                        
                                }
                            }
                            else{
                                angularPhysics[i] = Vector3.zero;
                            }
                            
                            vectorialPhysics[i] = Vector3.SmoothDamp( (Vector3)vectorialPhysics[i], targetVectors[i], ref movingVector[i], 0.1f, Mathf.Infinity, tIteration );
                            
                            if ( ( (Vector3)vectorialPhysics[i]-targetVectors[i] ).sqrMagnitude < 0.0005f ){
                                targetVectors[i] *= -0.85f*inertia;
                                targetVectors[i].y = 0;
                            }                            
                        
                        }
                        else if (systemOwner.lodModule.XFur_LODLevel > 0){
                            if ( useAngularPhysics ){
                                tAngle = (buffRotations[i]*Quaternion.Inverse(bones[i].rotation)).eulerAngles/180.0f;
                                buffRotations[i] = bones[i].rotation;
                                targetAngles[i] = Vector3.ClampMagnitude( targetAngles[i]+tAngle*6*physicsSensitivity, physicsStrength);
                                angularPhysics[i] = Vector4.MoveTowards( angularPhysics[i], targetAngles[i], physicsStrength*tIteration*10 );
                            }
                            else{
                                buffRotations[i] = bones[i].rotation;
                                angularPhysics[i] = Vector3.zero;
                            }
                            vectorialPhysics[i] = Vector4.MoveTowards( vectorialPhysics[i], targetVectors[i], physicsStrength*tIteration*10 );
                        }
                        else if ( systemOwner.lodModule.XFur_LODLevel == 0 ) {
                            buffRotations[i] = bones[i].rotation;
                            buffPositions[i] = bones[i].position;
                            angularPhysics[i] = vectorialPhysics[i] = Vector4.zero;

                        }
                        
                        
                    }
                }

                systemOwner.XFur_UpdateFurMaterials();

                timer = Time.timeSinceLevelLoad+tIteration+Random.Range(0.0f,Time.deltaTime*2);
            }
        }

        public override void Module_OnRender(){
            if ( !Application.isPlaying && State == XFurModuleState.Enabled ){
                
            }
        }

        public override void Module_UpdateFurData(ref MaterialPropertyBlock m){
            m.SetVectorArray("_VectorPhysics", vectorialPhysics );
            m.SetVectorArray("_AnglePhysics", angularPhysics );
            m.SetFloat("_FurGravity", State==XFurModuleState.Enabled?gravityStrength:0 );
        }

        public override void Module_End(){
            
        }

        #if UNITY_EDITOR

        public override void Module_StartUI( GUISkin editorSkin ){
            base.Module_StartUI( editorSkin );
            moduleName = "Physics Module 2.0 (BETA)";
        }

        public override void Module_UI(){
            base.Module_UI();

            if ( State == XFurModuleState.Enabled ){
                PDEditor_Toggle(new GUIContent("Use Basic Physics", "Use a less complex and less accurate physics algorithm. Improves performance"), ref useBasicPhysics );
                PDEditor_Toggle(new GUIContent("Use Angular forces", "Calculate physics for angular movements (rotations). Affects performance"), ref useAngularPhysics );
                GUILayout.Space(8);
                PDEditor_IntField( new GUIContent("Physics iterations", "Amount of iterations per second that the physics simulation will perform. Impacts performance"), ref iterations );
                PDEditor_Slider( new GUIContent("Physics sensitivity", "The sensitivity of the fur to movement. The higher the sensitivity, the smaller the movements need to be to be tracked by the physics simulation"), ref physicsSensitivity, 0.0f, 1.0f );
                PDEditor_Slider( new GUIContent("Physics strength", "The strength of the physics to be applied to the fur"), ref physicsStrength, 0.0f, 1.0f );
                PDEditor_Slider( new GUIContent("Gravity strength", "The strength of the gravity to be applied to the fur"), ref gravityStrength, 0.0f, 1.0f );
                PDEditor_Slider( new GUIContent("Inertia strength", "The strength of the inertia to be applied to the fur"), ref inertia, 0.0f, 1.0f );
            }
            GUILayout.Space(8);
        }

        #endif
        
    }
    
}