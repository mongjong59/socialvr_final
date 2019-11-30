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
using System.Collections.Generic;
#if UNITY_2018_1_OR_NEWER
using UnityEngine.Experimental.Rendering;
#endif

namespace XFurStudio{

	//public enum XFurQualityLevel{Low,Medium,High};
	//public enum XFurModuleType{ RenderModule, UpdateModule, TimedModule };
	public enum XFurModuleState{AssetMode,Enabled,Disabled};

	
	
	[ExecuteInEditMode]
	public class XFur_System : MonoBehaviour {
		
		public static Dictionary<Material, Material[]> materialReferences; //INTERNAL
		public Dictionary<Material, Material[]> runMaterialReferences; //INTERNAL
		
		//DEFAULT MODULES

		/// <summary>The main FX Module of this XFur System</summary>
		public XFur_FXModule fxModule = new XFur_FXModule();
		/// <summary>The main LOD Module of this XFur System</summary>
		public XFur_LODModule lodModule = new XFur_LODModule();
		/// <summary>The main Physics Module of this XFur System</summary>
		public XFur_PhysicsModule physicsModule = new XFur_PhysicsModule();
		/// <summary>The main Randomization Module of this XFur System</summary>
		public XFur_CoatingModule coatingModule = new XFur_CoatingModule();
		
		//PLACEHOLDER

		public XFur_SystemModule[] customModules;
		public Renderer skinRenderer;

		//MAIN DATABASE
		public XFur_DatabaseModule database;
		
		
		public bool srpMode; // INTERNAL, NOT IMPLEMENTED YET
		public bool instancedMode; //INTERNAL, NOT USED YET

		private Renderer rend;
		
		
		//=============== MAIN FUR SETTINGS =================

		//Material configuration and properties
		public XFur_MaterialProperties[] materialProfiles = new XFur_MaterialProperties[0];		

		[SerializeField]protected Mesh originalMesh;
		public int materialProfileIndex;

		public Texture2D bumpEmpty;
		private Material[] sharedMats;
		private float timerStep;
		private Texture2D normalTexture;
		private string internalVersion = "1.25";
		
		//IN-EDITOR LAYER GENERATION
		
		private bool generateLayers;
		private int layersSoFar;

//============================================================================
//================= MAIN SCRIPT ==============================================

		void OnEnable(){
			XFur_Start();

		}

		void Start(){
			
			XFur_Start();
			sharedMats = GetComponent<Renderer>().sharedMaterials;
		}


		/// <summary>
		/// Kickstarts all the different modules of the XFur system and prepares all the needed materials and lods
		/// </summary>
		public void XFur_Start() {

		
			rend = GetComponent<Renderer>();

			
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.update+=XFur_GenerateLayers;
			#endif

			#if UNITY_2018_1_OR_NEWER
        	RenderPipeline.beginCameraRendering += SRP_OnRender;
        	#endif

			sharedMats = GetComponent<Renderer>().sharedMaterials;

			if ( materialProfiles.Length != sharedMats.Length ){
				materialProfiles = new XFur_MaterialProperties[sharedMats.Length];
				for( int i = 0; i < materialProfiles.Length; i++ ){
					materialProfiles[i] = new XFur_MaterialProperties();
					materialProfiles[i].originalMat = sharedMats[i];
					materialProfiles[i].SynchToOriginalMat();
					materialProfiles[i].furmatIndex = i;
				}
			}

			foreach( XFur_MaterialProperties mat in materialProfiles ){
				if ( mat.furmatType == 2 )
					UpdateSharedData( mat );
			}

			if ( ( GetComponent<MeshFilter>() && GetComponent<MeshFilter>().sharedMesh && !GetComponent<MeshFilter>().sharedMesh.name.Contains("_XFMSH_PATCHED") ) || ( GetComponent<SkinnedMeshRenderer>() && GetComponent<SkinnedMeshRenderer>().sharedMesh && !GetComponent<SkinnedMeshRenderer>().sharedMesh.name.Contains("_XFMSH_PATCHED") ) ){
				XFur_UpdateMeshData();
			}

			if ( GetComponent<MeshFilter>() && !GetComponent<MeshFilter>().sharedMesh ){
				GetComponent<MeshFilter>().sharedMesh = originalMesh;
			}

			if ( GetComponent<SkinnedMeshRenderer>() && !GetComponent<SkinnedMeshRenderer>().sharedMesh ){
				GetComponent<SkinnedMeshRenderer>().sharedMesh = originalMesh;
			}

		
			if ( coatingModule != null ){
				coatingModule.Module_Start(this);
			}


			if ( lodModule != null ){
				lodModule.Module_Start(this);
			}

			if ( physicsModule != null ){
				physicsModule.Module_Start(this);
			}


			if ( fxModule != null ){
				fxModule.Module_Start(this);
			}

			XFur_UpdateFurMaterials();

		}


		/// <summary>
		/// Calls the update function on each material
		/// </summary>
		public void XFur_UpdateFurMaterials(){
			
			if ( (!rend || !rend.isVisible) && timerStep > 0 ){
				return;
			}

			

			foreach ( XFur_MaterialProperties mat in materialProfiles ){
				if ( mat.furmatType == 2 )
					UpdateFurMaterial( mat );
			}
		}

		/// <summary>
		/// Main execution cycle for the system
		/// </summary>
		void LateUpdate(){ 
			
			#if !UNITY_2017_1_OR_NEWER
			if ( rend )
				rend.lightProbeUsage = Camera.main.renderingPath==RenderingPath.Forward?UnityEngine.Rendering.LightProbeUsage.BlendProbes:UnityEngine.Rendering.LightProbeUsage.Off;
			#endif
			
			for ( int i = 0; i < materialProfiles.Length; i++ ){
				materialProfiles[i].originalSynch = false;
				foreach ( Renderer r in materialProfiles[i].subRenders ){
					if ( r!=null){
						r.gameObject.hideFlags = HideFlags.HideInHierarchy|HideFlags.NotEditable;
						r.lightProbeUsage = Camera.main.renderingPath==RenderingPath.Forward?UnityEngine.Rendering.LightProbeUsage.BlendProbes:UnityEngine.Rendering.LightProbeUsage.Off;
						if ( r.GetComponent<Renderer>().sharedMaterials.Length>1 ){
							var sharMats = new Material[]{r.GetComponent<Renderer>().sharedMaterial};
							r.GetComponent<Renderer>().sharedMaterials = sharMats;
						}
						r.gameObject.SetActive(materialProfiles[i].furmatShadowsMode == 1 );
						if ( r.GetComponent<MeshFilter>()){
							r.transform.parent = transform;
						}
					}
				}
			}

			if ( Time.timeSinceLevelLoad > timerStep && physicsModule.State!=XFurModuleState.Enabled && !instancedMode && ( lodModule.State==XFurModuleState.Disabled||lodModule.XFur_LODLevel>0||timerStep==0 ) ){
				XFur_UpdateFurMaterials();
				timerStep = Time.timeSinceLevelLoad+Random.Range(0.15f,0.75f);
			}

			coatingModule.Module_Execute();
			fxModule.Module_Execute();
			lodModule.Module_Execute();
			physicsModule.Module_Execute();
		}

		
		/// <summary>
		/// Handles resources destruction and memory cleaning
		/// </summary>
		void OnDisable(){

			#if UNITY_EDITOR
			UnityEditor.EditorApplication.update-=XFur_GenerateLayers;
			#endif


			#if UNITY_2018_1_OR_NEWER
        	RenderPipeline.beginCameraRendering -= SRP_OnRender;
        	#endif

			if ( GetComponent<MeshFilter>() ){
				GetComponent<MeshFilter>().sharedMesh = originalMesh;
			}

			 
			if ( GetComponent<SkinnedMeshRenderer>() ){
				GetComponent<SkinnedMeshRenderer>().sharedMesh = originalMesh;
			}  

			if ( ( !Application.isPlaying || GameObject.FindObjectsOfType<XFur_System>().Length == 0 ) && materialReferences != null && materialReferences.Count > 0 ){
				for ( int m = 0; m < materialProfiles.Length; m++ ){
					if ( materialReferences.ContainsKey(materialProfiles[m].originalMat) ){
						for ( int i = 0; i < materialReferences[materialProfiles[m].originalMat].Length; i++ ){
							DestroyImmediate(materialReferences[materialProfiles[m].originalMat][i]);
						}
						materialReferences[materialProfiles[m].originalMat] = null;
					}
				}

				materialReferences.Clear();
			}

			if ( Application.isPlaying && instancedMode ){
				for ( int m = 0; m < materialProfiles.Length; m++ ){
					if ( runMaterialReferences!=null && runMaterialReferences[materialProfiles[m].originalMat]!= null )
						for ( int i = 0; i < runMaterialReferences[materialProfiles[m].originalMat].Length; i++ ){
							DestroyImmediate(runMaterialReferences[materialProfiles[m].originalMat][i]);
						}
						runMaterialReferences[materialProfiles[m].originalMat] = null;
					}

				runMaterialReferences.Clear();
			}

			sharedMats = GetComponent<Renderer>().sharedMaterials;

			foreach( XFur_MaterialProperties mat in materialProfiles ){
				if ( mat.originalMat ){
					if ( mat.furmatType == 2 )
						sharedMats[mat.furmatIndex] = mat.originalMat;
					GetComponent<Renderer>().sharedMaterials = sharedMats;
				}
			}

			if ( coatingModule!=null ){
				coatingModule.Module_End();
			}

			if ( lodModule!=null ){
				lodModule.Module_End();
			}

			if ( physicsModule!=null ){
				physicsModule.Module_End();
			}

			if ( fxModule!=null ){
				fxModule.Module_End();
			}

		}

		



//======================================================================

#region GENERAL FUNCTIONS

	public void XFur_LoadFurProfile( XFur_CoatingProfile profile, int materialIndex ){
		CopyFurProperties( profile.profile, ref materialProfiles[materialIndex] );
	}

	public void CopyFurProperties ( XFur_MaterialProperties src, ref XFur_MaterialProperties dest ){
		dest.furmatAnisoOffset = src.furmatAnisoOffset;
		dest.furmatBaseColor = src.furmatBaseColor;
		dest.furmatBaseTex = src.furmatBaseTex;
		dest.furmatCollision = src.furmatCollision;
		dest.furmatData0 = src.furmatData0;
		dest.furmatData1 = src.furmatData1;
		dest.furmatFurColorA = src.furmatFurColorA;
		dest.furmatFurColorB = src.furmatFurColorB;
		dest.furmatFurColorMap = src.furmatFurColorMap;
		dest.furmatFurCutoff = src.furmatFurCutoff; 
		dest.furmatFurLength = src.furmatFurLength;
		dest.furmatFurNoiseMap = src.furmatFurNoiseMap;
		dest.furmatFurOcclusion = src.furmatFurOcclusion;
		dest.furmatFurRim = src.furmatFurRim;
		dest.furmatFurRimPower = src.furmatFurRimPower;
		dest.furmatFurSmoothness = src.furmatFurSmoothness;
		dest.furmatFurSpecular = src.furmatFurSpecular;
		dest.furmatFurThickness = src.furmatFurThickness;
		dest.furmatFurUV1 = src.furmatFurUV1;
		dest.furmatFurUV2 = src.furmatFurUV2;
		dest.furmatGlossSpecular = src.furmatGlossSpecular;
		dest.furmatNormalmap = src.furmatNormalmap;
		dest.furmatOcclusion = src.furmatOcclusion;
		dest.furmatSmoothness = src.furmatSmoothness;
		dest.furmatSpecular = src.furmatSpecular;
		dest.furmatUV1 = src.furmatUV1;
	}


#if UNITY_EDITOR
public void AutoUpgradeMaterial( XFur_MaterialProperties mat ){
	if ( mat.furmatType == 1 ){
		var srcMat = mat.originalMat;
		var newMat = new Material(database.highQualityShaders[database.highQualityShaders.Length-1]);
		newMat.SetTexture("_BaseTex", srcMat.GetTexture("_MainTex") );
		newMat.SetTexture("_FurColorMap", srcMat.GetTexture("_FurColorMap") );
		newMat.SetTexture("_Normalmap", srcMat.GetTexture("_BumpMap") );
		newMat.SetTexture("_GlossSpecular", srcMat.GetTexture("_GlossMap") );
		newMat.SetTexture("_FurNoiseMap", srcMat.GetTexture("_FurMap") );
		newMat.SetTexture("_FurData0", srcMat.GetTexture("_FurData") );
		newMat.SetTexture("_FurData1", srcMat.GetTexture("_FurDirMap") );
		newMat.SetColor("_BaseColor", srcMat.GetColor("_Color") );
		newMat.SetColor("_BaseSpecular", srcMat.GetColor("_SpecColor") );
		newMat.SetColor("_FurSpecular", srcMat.GetColor("_FurSpecular") );
		newMat.SetColor("_RimColor", srcMat.GetColor("_FurRimColor") );
		newMat.SetColor("_FurColorA", srcMat.GetColor("_FurColor")*0.5f );
		newMat.SetColor("_FurColorB", srcMat.GetColor("_FurColor") );
		newMat.SetFloat("_BaseSmoothness", srcMat.GetFloat("_Glossiness") );
		newMat.SetFloat("_FurSmoothness", srcMat.GetFloat("_FurGloss")*0.5f );
		newMat.SetFloat("_FurLength", srcMat.GetFloat("_FurLength") );
		newMat.SetFloat("_FurThin", srcMat.GetFloat("_FurFade")*0.6f );
		newMat.SetFloat("_FurCutoff", srcMat.GetFloat("_Cutoff") );
		newMat.SetFloat("_FurRimStrength", srcMat.GetFloat("_FurRim")*0.125f );
		newMat.SetFloat("_FurOcclusion", srcMat.GetFloat("_FurOcclusion") );
		newMat.SetFloat("_LocalWindStrength", srcMat.GetFloat("_LocalWindSpeed") );
		newMat.SetFloat("_FurDirection", srcMat.GetFloat("_FurDirection") );
		newMat.SetFloat("_UV1Scale1", srcMat.GetTextureScale("_FurMap").x );
		newMat.SetFloat("_UV1Scale2", srcMat.GetTextureScale("_FurMap").x );
		newMat.SetFloat("_TriplanarScale", srcMat.HasProperty("_FurTiling")?srcMat.GetFloat("_FurTiling"):2 );

		if ( srcMat.shader.name.Contains("Aniso") )
			newMat.EnableKeyword("ANISOTROPIC_ON");
		else
			newMat.DisableKeyword("ANISOTROPIC_ON");

		newMat.SetFloat("_TriplanarMode", srcMat.shader.name.Contains("Triplanar")?1:0 );

		srcMat.shader = newMat.shader;
		srcMat.shaderKeywords = newMat.shaderKeywords;

		srcMat.CopyPropertiesFromMaterial(newMat);

		DestroyImmediate(newMat);
		mat.furmatType = 2;
		mat.furmatSamples = 2;

	}	
}


public XFur_CoatingProfile XFur_ExportMaterialProfile( XFur_MaterialProperties mat ){
	
	if ( mat.furmatType == 2 ){
		var temp = (XFur_CoatingProfile)ScriptableObject.CreateInstance("XFur_CoatingProfile");
		temp.profile = new XFur_MaterialProperties();
		if ( mat.furmatReadBaseFur == 1 ){
			CopyFurProperties( mat, ref temp.profile );
			temp.furColorA_Max = temp.furColorA_Min = mat.furmatFurColorA;
			temp.furColorB_Max = temp.furColorB_Min = mat.furmatFurColorB;
		}
		else{
			var mat2 = new XFur_MaterialProperties();
			mat2.originalMat = mat.originalMat;
			mat2.SynchToOriginalMat();
			CopyFurProperties( mat2, ref temp.profile );
			temp.furColorA_Max = temp.furColorA_Min = mat2.furmatFurColorA;
			temp.furColorB_Max = temp.furColorB_Min = mat2.furmatFurColorB;
		}
		return temp;
	}
	return null;
}


#endif

#endregion

#region XFUR_MATERIALS_UPDATE
//======================================================================

		public void UpdateSharedData( XFur_MaterialProperties mat ){

			if ( mat.furmatType != 2 ){
				return;
			}

			if ( materialReferences == null ){
				materialReferences = new Dictionary<Material, Material[]>();
			}

			if ( runMaterialReferences == null ){
				runMaterialReferences = new Dictionary<Material, Material[]>();
			}
	
			if ( database && mat.originalMat && mat.originalMat.shader.name.Contains("XFur") && !mat.originalMat.shader.name.Contains("Legacy") && !mat.originalMat.name.Contains("_LODInstance_") ){
				if ( !Application.isPlaying || !instancedMode ){
					if ( !materialReferences.ContainsKey(mat.originalMat) || materialReferences[mat.originalMat] == null || materialReferences[mat.originalMat].Length != database.highQualityShaders.Length+database.lowQualityShaders.Length+2 || materialReferences[mat.originalMat][0]==null ){
						if ( !materialReferences.ContainsKey(mat.originalMat) ){

							var mats = new Material[database.highQualityShaders.Length+database.lowQualityShaders.Length+2];

							for ( int i = 0; i < database.highQualityShaders.Length; i++ ){
								mats[i] = Instantiate(mat.originalMat);
								mats[i].shader = database.highQualityShaders[i];
								mats[i].name = mat.originalMat.name+"_"+database.highQualityShaders[i].name.Split("/"[0])[database.highQualityShaders[i].name.Split("/"[0]).Length-1];
							}

							for ( int i = 0; i < database.lowQualityShaders.Length; i++ ){
								mats[i+database.highQualityShaders.Length] = Instantiate(mat.originalMat);
								mats[i+database.highQualityShaders.Length].shader = database.lowQualityShaders[i];
								mats[i].name = mat.originalMat.name+"_"+database.lowQualityShaders[i].name.Split("/"[0])[database.lowQualityShaders[i].name.Split("/"[0]).Length-1];
							}

							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length] = Instantiate(mat.originalMat);
							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length].shader = database.DynamicShellSkin;
							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length].name = mat.originalMat.name+"_"+"XFurBasePass";
							
							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length+1] = Instantiate(mat.originalMat);
							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length+1].shader = database.DynamicShellShader;
							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length+1].name = mat.originalMat.name+"_"+"XFurDynPass";
							
							
							materialReferences.Add( mat.originalMat, mats );
						}
						else{

							var mats = new Material[database.highQualityShaders.Length+database.lowQualityShaders.Length+2];

							for ( int i = 0; i < database.highQualityShaders.Length; i++ ){
								if ( materialReferences[mat.originalMat]!=null ){
									for ( int x = 0; x < materialReferences[mat.originalMat].Length; x++ ){
										if ( materialReferences[mat.originalMat][x]!=null && materialReferences[mat.originalMat][x].shader == database.highQualityShaders[i] ){
											mats[i] = materialReferences[mat.originalMat][x];
										}
									}
								}

								if ( !mats[i] ){
									mats[i] = Instantiate(mat.originalMat);
									mats[i].shader = database.highQualityShaders[i];
									mats[i].name = mat.originalMat.name+"_"+database.highQualityShaders[i].name.Split("/"[0])[database.highQualityShaders[i].name.Split("/"[0]).Length-1];
								}
							}

							for ( int i = 0; i < database.lowQualityShaders.Length; i++ ){

								if ( materialReferences[mat.originalMat]!=null ){
									for ( int x = 0; x < materialReferences[mat.originalMat].Length; x++ ){
										if ( materialReferences[mat.originalMat][x]!=null && materialReferences[mat.originalMat][x].shader == database.lowQualityShaders[i] ){
											mats[i] = materialReferences[mat.originalMat][x];
										}
									}
								}

								if ( !mats[i] ){
									mats[i+database.highQualityShaders.Length] = Instantiate(mat.originalMat);
									mats[i+database.highQualityShaders.Length].shader = database.lowQualityShaders[i];
									mats[i].name = mat.originalMat.name+"_"+database.lowQualityShaders[i].name.Split("/"[0])[database.lowQualityShaders[i].name.Split("/"[0]).Length-1];
								}
							}

							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length] = Instantiate(mat.originalMat);
							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length].shader = database.DynamicShellSkin;
							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length].name = mat.originalMat.name+"_"+"XFurBasePass";
							
							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length+1] = Instantiate(mat.originalMat);
							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length+1].shader = database.DynamicShellShader;
							mats[database.highQualityShaders.Length+database.lowQualityShaders.Length+1].name = mat.originalMat.name+"_"+"XFurDynPass";
							

							var fMats = new List<Material>(mats);

							if ( materialReferences[mat.originalMat] != null ){
								for ( int x = 0; x < materialReferences[mat.originalMat].Length; x++ ){
									if ( materialReferences[mat.originalMat][x]!=null && !fMats.Contains(materialReferences[mat.originalMat][x]) ){
										DestroyImmediate(materialReferences[mat.originalMat][x]);
									}
								}
							}

							materialReferences[mat.originalMat] = mats;
						}

					}
				}
				else{
					if ( !runMaterialReferences.ContainsKey(mat.originalMat) || runMaterialReferences[mat.originalMat] == null || runMaterialReferences[mat.originalMat].Length != database.highQualityShaders.Length+database.lowQualityShaders.Length || runMaterialReferences[mat.originalMat][0]==null ){
						if ( !runMaterialReferences.ContainsKey(mat.originalMat) ){

							var mats = new Material[database.highQualityShaders.Length+database.lowQualityShaders.Length];

							for ( int i = 0; i < database.highQualityShaders.Length; i++ ){
								mats[i] = Instantiate(mat.originalMat);
								mats[i].shader = database.highQualityShaders[i];
								mats[i].name = mat.originalMat.name+"_INSTANCED_"+database.highQualityShaders[i].name.Split("/"[0])[database.highQualityShaders[i].name.Split("/"[0]).Length-1];
							}

							for ( int i = 0; i < database.lowQualityShaders.Length; i++ ){
								mats[i+database.highQualityShaders.Length] = Instantiate(mat.originalMat);
								mats[i+database.highQualityShaders.Length].shader = database.lowQualityShaders[i];
								mats[i].name = mat.originalMat.name+"_INSTANCED_"+database.lowQualityShaders[i].name.Split("/"[0])[database.lowQualityShaders[i].name.Split("/"[0]).Length-1];
							}

							runMaterialReferences.Add( mat.originalMat, mats );
						}
						else{

							var mats = new Material[database.highQualityShaders.Length+database.lowQualityShaders.Length];

							for ( int i = 0; i < database.highQualityShaders.Length; i++ ){
								if ( runMaterialReferences[mat.originalMat]!=null ){
									for ( int x = 0; x < runMaterialReferences[mat.originalMat].Length; x++ ){
										if ( runMaterialReferences[mat.originalMat][x]!=null && runMaterialReferences[mat.originalMat][x].shader == database.highQualityShaders[i] ){
											mats[i] = runMaterialReferences[mat.originalMat][x];
										}
									}
								}

								if ( !mats[i] ){
									mats[i] = Instantiate(mat.originalMat);
									mats[i].shader = database.highQualityShaders[i];
									mats[i].name = mat.originalMat.name+"_INSTANCED_"+database.highQualityShaders[i].name.Split("/"[0])[database.highQualityShaders[i].name.Split("/"[0]).Length-1];
								}
							}

							for ( int i = 0; i < database.lowQualityShaders.Length; i++ ){

								if ( runMaterialReferences[mat.originalMat]!=null ){
									for ( int x = 0; x < runMaterialReferences[mat.originalMat].Length; x++ ){
										if ( runMaterialReferences[mat.originalMat][x]!=null && runMaterialReferences[mat.originalMat][x].shader == database.lowQualityShaders[i] ){
											mats[i] = runMaterialReferences[mat.originalMat][x];
										}
									}
								}

								if ( !mats[i] ){
									mats[i+database.highQualityShaders.Length] = Instantiate(mat.originalMat);
									mats[i+database.highQualityShaders.Length].shader = database.lowQualityShaders[i];
									mats[i].name = mat.originalMat.name+"_INSTANCED_"+database.lowQualityShaders[i].name.Split("/"[0])[database.lowQualityShaders[i].name.Split("/"[0]).Length-1];
								}
							}

							var fMats = new List<Material>(mats);

							if ( runMaterialReferences[mat.originalMat] != null ){
								for ( int x = 0; x < runMaterialReferences[mat.originalMat].Length; x++ ){
									if ( runMaterialReferences[mat.originalMat][x]!=null && !fMats.Contains(runMaterialReferences[mat.originalMat][x]) ){
										DestroyImmediate(runMaterialReferences[mat.originalMat][x]);
									}
								}
							}

							runMaterialReferences[mat.originalMat] = mats;
						}

					}
				}
			}



		}

		
		
		
		void OnWillRenderObject(){
		
			if ( !Application.isPlaying ){
				if ( ( GetComponent<MeshFilter>() && GetComponent<MeshFilter>().sharedMesh && !GetComponent<MeshFilter>().sharedMesh.name.Contains("_XFMSH_PATCHED") ) || ( GetComponent<SkinnedMeshRenderer>() && GetComponent<SkinnedMeshRenderer>().sharedMesh && !GetComponent<SkinnedMeshRenderer>().sharedMesh.name.Contains("_XFMSH_PATCHED") ) ){
					XFur_UpdateMeshData();
				}

				physicsModule.Module_OnRender();
			}
			
			foreach ( XFur_MaterialProperties mat in materialProfiles ){
				if ( !Application.isPlaying ){

					if ( mat.furmatType == 2 )
						UpdateFurMaterial( mat );

					if ( !mat.originalSynch && mat.furmatType == 2 ){
						if ( materialReferences.ContainsKey(mat.originalMat) ){
							foreach ( Material samp in materialReferences[mat.originalMat] ){
								if ( samp == null ){
									UpdateSharedData(mat);
									break;
								}
								samp.CopyPropertiesFromMaterial(mat.originalMat);
							}

						}
						mat.originalSynch = true;
					}
				}

				if ( !mat.originalSynch && !Application.isPlaying && mat.furmatType == 2 ){
					if ( materialReferences.ContainsKey(mat.originalMat) ){
						foreach ( Material samp in materialReferences[mat.originalMat] ){
							samp.CopyPropertiesFromMaterial(mat.originalMat);
						}
					}
					mat.originalSynch = true;
				}
			}

			
			
		}
		

		#if UNITY_2018_1_OR_NEWER
		void SRP_OnRender( Camera cam ){


		}
		#endif



		public void XFur_SwitchMaterialSamples( XFur_MaterialProperties mat ){

			if ( mat.furmatType != 2 ){
				return;
			}

			#if UNITY_EDITOR
			if ( UnityEditor.PrefabUtility.GetPrefabType( gameObject ) == UnityEditor.PrefabType.Prefab ){
				for ( int i = 0; i < materialProfiles.Length; i++ ){
					sharedMats[i] = materialProfiles[i].originalMat;
				}
				
				GetComponent<Renderer>().sharedMaterials = sharedMats;
				return;
			}
			#endif

			// MANAGED SINGLE MATERIAL PARAMETRICAL VALUES
			if ( !Application.isPlaying || !instancedMode ){
				if ( mat.furmatShadowsMode == 0 && materialReferences.ContainsKey( mat.originalMat ) ){
					if ( database && materialReferences[mat.originalMat]!=null ){
						if ( database.highQualityShaders.Length > mat.furmatSamples ){
							sharedMats = GetComponent<Renderer>().sharedMaterials;
							sharedMats[mat.furmatIndex] = materialReferences[mat.originalMat][mat.furmatSamples];
							GetComponent<Renderer>().sharedMaterials = sharedMats;
						}
					}
				}
				else if ( materialReferences.ContainsKey( mat.originalMat ) ){
					if ( database && materialReferences[mat.originalMat]!=null ){
						if ( database.DynamicShellSkin ){
							sharedMats = GetComponent<Renderer>().sharedMaterials;
							sharedMats[mat.furmatIndex] = materialReferences[mat.originalMat][database.highQualityShaders.Length+database.lowQualityShaders.Length];
							GetComponent<Renderer>().sharedMaterials = sharedMats;

							foreach( Renderer r in mat.subRenders ){
								if ( r!= null )
									r.sharedMaterial = materialReferences[mat.originalMat][database.highQualityShaders.Length+database.lowQualityShaders.Length+1];
							}
							
						}
					}
				}
				else{
					sharedMats = GetComponent<Renderer>().sharedMaterials;
					if ( mat.furmatType == 2 )
						sharedMats[mat.furmatIndex] = mat.originalMat;
					GetComponent<Renderer>().sharedMaterials = sharedMats;
				}
			}
			//INSTANCED MODE PARAMETERS
			else{
				if ( mat.furmatShadowsMode == 0 && runMaterialReferences.ContainsKey( mat.originalMat ) ){
					if ( database && runMaterialReferences[mat.originalMat]!=null ){
						if ( database.highQualityShaders.Length > mat.furmatSamples ){
							sharedMats = GetComponent<Renderer>().sharedMaterials;
							sharedMats[mat.furmatIndex] = runMaterialReferences[mat.originalMat][mat.furmatSamples];
							GetComponent<Renderer>().sharedMaterials = sharedMats;
						}
					}
				}
				else if ( runMaterialReferences.ContainsKey( mat.originalMat ) ){
					if ( database && runMaterialReferences[mat.originalMat]!=null ){
						if ( database.lowQualityShaders.Length > mat.furmatSamples ){
							sharedMats = GetComponent<Renderer>().sharedMaterials;
							sharedMats[mat.furmatIndex] = runMaterialReferences[mat.originalMat][mat.furmatSamples+database.highQualityShaders.Length];
							GetComponent<Renderer>().sharedMaterials = sharedMats;
						}
					}
				}
				else{
					sharedMats = GetComponent<Renderer>().sharedMaterials;
					if ( mat.furmatType == 2 )
						sharedMats[mat.furmatIndex] = mat.originalMat;
					GetComponent<Renderer>().sharedMaterials = sharedMats;
				}
			}
		}

		

		void UpdateFurMaterial( XFur_MaterialProperties mat ){

			if ( bumpEmpty == null ){
				#if UNITY_EDITOR
				bumpEmpty = (Texture2D)UnityEditor.AssetDatabase.LoadAssetAtPath( UnityEditor.AssetDatabase.GUIDToAssetPath( UnityEditor.AssetDatabase.FindAssets("NullNormal t:Texture2D")[0] ), typeof(Texture2D) );
				#endif
			}

			if ( mat.furmatType != 2 ){
				return;
			}


			
			if ( !materialReferences.ContainsKey(mat.originalMat) ){
				UpdateSharedData(mat);
			}

			XFur_SwitchMaterialSamples( mat );

			if ( srpMode || mat.furmatIndex < 0 || !mat.originalMat ){
				return;
			}

			if ( !mat.originalMat.shader.name.Contains("XFur") ){
				return;
			}
			else{
				if ( mat.originalMat.shader.name.Contains("Legacy") ){
					return;
				}
			}

			if ( mat.furmatNormalmap == null ){
				mat.furmatNormalmap = bumpEmpty;
			}

			MaterialPropertyBlock m = new MaterialPropertyBlock();

				if ( !Application.isPlaying || !instancedMode ){
					#if UNITY_2018_1_OR_NEWER
					GetComponent<Renderer>().GetPropertyBlock( m, mat.furmatIndex );
					var furs = 0;
					#else
					GetPropertyBlock( ref m, mat.furmatIndex );
					var furs = 0;
					#endif
					
					m.SetFloat( "_TriplanarMode", furs>1?mat.originalMat.GetFloat("_TriplanarMode"):mat.furmatTriplanar );
					m.SetFloat("_UV2Grooming",furs>1?mat.originalMat.GetFloat("_UV2Grooming"):mat.furmatForceUV2Grooming );
					m.SetFloat( "_SelfCollision", furs>1?mat.originalMat.GetFloat("_SelfCollision"):mat.furmatCollision );
					m.SetFloat( "_TriplanarScale", furs>1?mat.originalMat.GetFloat("_TriplanarScale"):mat.furmatTriplanarScale );

					if ( mat.furmatReadBaseSkin > 0 && furs < 2 ){
						m.SetColor("_BaseColor", mat.furmatBaseColor );
						m.SetColor("_BaseSpecular", mat.furmatSpecular );
						m.SetTexture( "_BaseTex", mat.furmatBaseTex?mat.furmatBaseTex:Texture2D.whiteTexture );
						m.SetFloat( "_HasGlossMap", mat.furmatGlossSpecular?1:0);
						m.SetTexture( "_GlossSpecular", mat.furmatGlossSpecular?mat.furmatGlossSpecular:Texture2D.blackTexture );
						m.SetFloat( "_BaseSmoothness", mat.furmatSmoothness );
						m.SetTexture( "_Normalmap", mat.furmatNormalmap?mat.furmatNormalmap:bumpEmpty );
						m.SetTexture( "_OcclusionMap", mat.furmatOcclusion?mat.furmatOcclusion:Texture2D.whiteTexture );
					} 
					else{
						m.SetColor("_BaseColor", mat.originalMat.GetColor("_BaseColor") );
						m.SetColor("_BaseSpecular", mat.originalMat.GetColor("_BaseSpecular") );
						m.SetTexture( "_BaseTex", mat.originalMat.GetTexture("_BaseTex")?mat.originalMat.GetTexture("_BaseTex"):Texture2D.whiteTexture );
						m.SetFloat( "_HasGlossMap", mat.originalMat.GetTexture("_GlossSpecular")?1:0);
						m.SetTexture( "_GlossSpecular", mat.originalMat.GetTexture("_GlossSpecular")?mat.originalMat.GetTexture("_GlossSpecular"):Texture2D.blackTexture );
						m.SetFloat( "_BaseSmoothness", mat.originalMat.GetFloat("_BaseSmoothness") );
						m.SetTexture( "_Normalmap", mat.originalMat.GetTexture("_Normalmap")?mat.originalMat.GetTexture("_Normalmap"):bumpEmpty );
						m.SetTexture( "_OcclusionMap", mat.originalMat.GetTexture("_OcclusionMap")?mat.originalMat.GetTexture("_OcclusionMap"):Texture2D.whiteTexture );
					}

					

					if ( mat.furmatReadBaseFur > 0 && furs < 2 ){
						if ( mat.furmatReadFurNoise > 0  && furs < 2 ){
							m.SetTexture("_FurNoiseMap", mat.furmatFurNoiseMap?mat.furmatFurNoiseMap:Texture2D.whiteTexture);
						}
						else{ 
							m.SetTexture("_FurNoiseMap", mat.originalMat.GetTexture("_FurNoiseMap")?mat.originalMat.GetTexture("_FurNoiseMap"):Texture2D.whiteTexture);
						}
						m.SetTexture("_FurColorMap", mat.furmatFurColorMap?mat.furmatFurColorMap:Texture2D.whiteTexture);
						m.SetTexture( "_FurData0", mat.furmatData0?mat.furmatData0:Texture2D.whiteTexture );
						m.SetTexture( "_FurData1", mat.furmatData1?mat.furmatData1:Texture2D.whiteTexture );
						m.SetColor("_FurColorA", mat.furmatFurColorA );
						m.SetColor("_FurColorB", mat.furmatFurColorB );
						m.SetColor("_FurSpecular", mat.furmatFurSpecular );
						m.SetFloat("_FurSmoothness", mat.furmatFurSmoothness );
						m.SetFloat("_FurOcclusion", mat.furmatFurOcclusion );
						m.SetFloat("_FurLength", mat.furmatFurLength );
						m.SetFloat("_FurThin", mat.furmatFurThickness );
						m.SetFloat("_UV1Scale1", mat.furmatFurUV1 );
						m.SetFloat("_UV1Scale2", mat.furmatFurUV2 );
						m.SetColor("_RimColor",mat.furmatFurRim );
						m.SetFloat("_FurRimStrength", mat.furmatFurRimPower );
						m.SetVector("_FurDirection",mat.furmatDirection );
					}
					else{
						m.SetTexture("_FurNoiseMap",mat.originalMat.GetTexture("_FurNoiseMap")?mat.originalMat.GetTexture("_FurNoiseMap"):Texture2D.whiteTexture);
						m.SetTexture("_FurColorMap",mat.originalMat.GetTexture("_FurColorMap")?mat.originalMat.GetTexture("_FurColorMap"):Texture2D.whiteTexture);
						m.SetTexture( "_FurData0", mat.originalMat.GetTexture("_FurData0")?mat.originalMat.GetTexture("_FurData0"):Texture2D.whiteTexture );
						m.SetTexture( "_FurData1", mat.originalMat.GetTexture("_FurData1")?mat.originalMat.GetTexture("_FurData1"):Texture2D.whiteTexture );
						m.SetColor("_FurColorA",mat.originalMat.GetColor("_FurColorA") );
						m.SetColor("_FurColorB",mat.originalMat.GetColor("_FurColorB") );
						m.SetColor("_FurSpecular",mat.originalMat.GetColor("_FurSpecular") );
						m.SetFloat("_FurOcclusion",mat.originalMat.GetFloat("_FurOcclusion") );
						m.SetFloat("_FurSmoothness",mat.originalMat.GetFloat("_FurSmoothness") );
						m.SetFloat("_FurLength",mat.originalMat.GetFloat("_FurLength") );
						m.SetFloat("_FurThin",mat.originalMat.GetFloat("_FurThin") );
						m.SetFloat("_UV1Scale1",mat.originalMat.GetFloat("_UV1Scale1") );
						m.SetFloat("_UV1Scale2",mat.originalMat.GetFloat("_UV1Scale2") );
						m.SetColor("_RimColor", mat.originalMat.GetColor("_RimColor"));
						m.SetFloat("_FurRimStrength",mat.originalMat.GetFloat("_FurRimStrength"));
						m.SetVector("_FurDirection",mat.originalMat.GetVector("_FurDirection") );
					}
					
#if !UNITY_2018_1_OR_NEWER

					if ( FurMaterials > 1 ){
						m.Clear();
						for ( int mainMat = 0; mainMat < sharedMats.Length; mainMat++ ){
							if ( sharedMats[mainMat] && sharedMats[mainMat].HasProperty("_FurStep"))	
								sharedMats[mainMat].SetFloat("_FurStep", 24 );
							else if ( !sharedMats[mainMat] )
								sharedMats[mainMat] = materialProfiles[mainMat].originalMat;

							sharedMats[mainMat].CopyPropertiesFromMaterial(materialProfiles[mainMat].originalMat);
						}
					}

#endif
					
					lodModule.Module_UpdateFurData( ref m );
					coatingModule.Module_UpdateFurData( ref m );
					physicsModule.Module_UpdateFurData(ref m );
					fxModule.Module_UpdateFurData( ref m );

					SetPropertyBlock( m, mat.furmatIndex );
				}
				else{
					var furs = FurMaterials;
					var material = sharedMats[mat.furmatIndex];
					material.SetFloat( "_TriplanarMode", furs>1?mat.originalMat.GetFloat("_TriplanarMode"):mat.furmatTriplanar );
					material.SetFloat("_UV2Grooming",furs>1?mat.originalMat.GetFloat("_UV2Grooming"):mat.furmatForceUV2Grooming );
					material.SetFloat( "_SelfCollision", furs>1?mat.originalMat.GetFloat("_SelfCollision"):mat.furmatCollision );
					material.SetFloat( "_TriplanarScale", furs>1?mat.originalMat.GetFloat("_TriplanarScale"):mat.furmatTriplanarScale );

					if ( mat.furmatReadBaseSkin > 0 && furs < 2 ){
						material.SetColor("_BaseColor", mat.furmatBaseColor );
						material.SetColor("_BaseSpecular", mat.furmatSpecular );
						material.SetTexture( "_BaseTex", mat.furmatBaseTex?mat.furmatBaseTex:Texture2D.whiteTexture );
						material.SetFloat( "_HasGlossMap", mat.furmatGlossSpecular?1:0);
						material.SetTexture( "_GlossSpecular", mat.furmatGlossSpecular?mat.furmatGlossSpecular:Texture2D.blackTexture );
						material.SetFloat( "_BaseSmoothness", mat.furmatSmoothness );
						material.SetTexture( "_Normalmap", mat.furmatNormalmap?mat.furmatNormalmap:bumpEmpty );
						material.SetTexture( "_OcclusionMap", mat.furmatOcclusion?mat.furmatOcclusion:Texture2D.whiteTexture );
					} 
					else{
						material.SetColor("_BaseColor", mat.originalMat.GetColor("_BaseColor") );
						material.SetColor("_BaseSpecular", mat.originalMat.GetColor("_BaseSpecular") );
						material.SetTexture( "_BaseTex", mat.originalMat.GetTexture("_BaseTex")?mat.originalMat.GetTexture("_BaseTex"):Texture2D.whiteTexture );
						material.SetFloat( "_HasGlossMap", mat.originalMat.GetTexture("_GlossSpecular")?1:0);
						material.SetTexture( "_GlossSpecular", mat.originalMat.GetTexture("_GlossSpecular")?mat.originalMat.GetTexture("_GlossSpecular"):Texture2D.blackTexture );
						material.SetFloat( "_BaseSmoothness", mat.originalMat.GetFloat("_BaseSmoothness") );
						material.SetTexture( "_Normalmap", mat.originalMat.GetTexture("_Normalmap")?mat.originalMat.GetTexture("_Normalmap"):bumpEmpty );
						material.SetTexture( "_OcclusionMap", mat.originalMat.GetTexture("_OcclusionMap")?mat.originalMat.GetTexture("_OcclusionMap"):Texture2D.whiteTexture );
					}

					

					if ( mat.furmatReadBaseFur > 0 && furs < 2 ){
						if ( mat.furmatReadFurNoise > 0  && furs < 2 ){
							material.SetTexture("_FurNoiseMap", mat.furmatFurNoiseMap?mat.furmatFurNoiseMap:Texture2D.whiteTexture);
						}
						else{ 
							material.SetTexture("_FurNoiseMap", mat.originalMat.GetTexture("_FurNoiseMap")?mat.originalMat.GetTexture("_FurNoiseMap"):Texture2D.whiteTexture);
						}
						material.SetTexture("_FurColorMap", mat.furmatFurColorMap?mat.furmatFurColorMap:Texture2D.whiteTexture);
						material.SetTexture( "_FurData0", mat.furmatData0?mat.furmatData0:Texture2D.whiteTexture );
						material.SetTexture( "_FurData1", mat.furmatData1?mat.furmatData1:Texture2D.whiteTexture );
						material.SetColor("_FurColorA", mat.furmatFurColorA );
						material.SetColor("_FurColorB", mat.furmatFurColorB );
						material.SetColor("_FurSpecular", mat.furmatFurSpecular );
						material.SetFloat("_FurSmoothness", mat.furmatFurSmoothness );
						material.SetFloat("_FurOcclusion", mat.furmatFurOcclusion );
						material.SetFloat("_FurLength", mat.furmatFurLength );
						material.SetFloat("_FurThin", mat.furmatFurThickness );
						material.SetFloat("_UV1Scale1", mat.furmatFurUV1 );
						material.SetFloat("_UV1Scale2", mat.furmatFurUV2 );
						material.SetColor("_RimColor",mat.furmatFurRim );
						material.SetFloat("_FurRimStrength", mat.furmatFurRimPower );
						m.SetVector("_FurDirection",mat.furmatDirection );
					}
					else{
						material.SetTexture("_FurNoiseMap",mat.originalMat.GetTexture("_FurNoiseMap")?mat.originalMat.GetTexture("_FurNoiseMap"):Texture2D.whiteTexture);
						material.SetTexture("_FurColorMap",mat.originalMat.GetTexture("_FurColorMap")?mat.originalMat.GetTexture("_FurColorMap"):Texture2D.whiteTexture);
						material.SetTexture( "_FurData0", mat.originalMat.GetTexture("_FurData0")?mat.originalMat.GetTexture("_FurData0"):Texture2D.whiteTexture );
						material.SetTexture( "_FurData1", mat.originalMat.GetTexture("_FurData1")?mat.originalMat.GetTexture("_FurData1"):Texture2D.whiteTexture );
						material.SetColor("_FurColorA",mat.originalMat.GetColor("_FurColorA") );
						material.SetColor("_FurColorB",mat.originalMat.GetColor("_FurColorB") );
						material.SetColor("_FurSpecular",mat.originalMat.GetColor("_FurSpecular") );
						material.SetFloat("_FurOcclusion",mat.originalMat.GetFloat("_FurOcclusion") );
						material.SetFloat("_FurSmoothness",mat.originalMat.GetFloat("_FurSmoothness") );
						material.SetFloat("_FurLength",mat.originalMat.GetFloat("_FurLength") );
						material.SetFloat("_FurThin",mat.originalMat.GetFloat("_FurThin") );
						material.SetFloat("_UV1Scale1",mat.originalMat.GetFloat("_UV1Scale1") );
						material.SetFloat("_UV1Scale2",mat.originalMat.GetFloat("_UV1Scale2") );
						material.SetColor("_RimColor", mat.originalMat.GetColor("_RimColor"));
						material.SetFloat("_FurRimStrength",mat.originalMat.GetFloat("_FurRimStrength"));
						m.SetVector("_FurDirection",mat.originalMat.GetVector("_FurDirection") );
					}



					lodModule.Module_InstancedFurData( material );
					coatingModule.Module_InstancedFurData( material );
					physicsModule.Module_InstancedFurData( material );
					fxModule.Module_InstancedFurData( material );
				}
			
		}


		



		public void GetPropertyBlock( ref MaterialPropertyBlock block, int index ){
			if ( block == null || index < 0 || !GetComponent<Renderer>() || sharedMats.Length <= index ){
				return;
			}
			else{
				sharedMats = GetComponent<Renderer>().sharedMaterials;
        		var t = sharedMats[0];
        		sharedMats[0] = sharedMats[index];
        		GetComponent<Renderer>().sharedMaterials = sharedMats;
        		GetComponent<Renderer>().GetPropertyBlock( block );
        		sharedMats[0] = t;
        		GetComponent<Renderer>().sharedMaterials = sharedMats;
			}
		}


		public void SetPropertyBlock( MaterialPropertyBlock block, int index ){
			if ( block == null || index < 0 || !GetComponent<Renderer>() || sharedMats.Length <= index ){
				return;
			}
			else{
				if ( materialProfiles[index].furmatShadowsMode == 0 ){
					#if UNITY_2018_1_OR_NEWER
					GetComponent<Renderer>().SetPropertyBlock( block, index );
					#else
					sharedMats = GetComponent<Renderer>().sharedMaterials;
					var t = sharedMats[0];
					sharedMats[0] = sharedMats[index];
					GetComponent<Renderer>().sharedMaterials = sharedMats;
					GetComponent<Renderer>().SetPropertyBlock( block );
					sharedMats[0] = t;
					GetComponent<Renderer>().sharedMaterials = sharedMats;
					#endif
				}
				else{

					#if UNITY_2018_1_OR_NEWER
					GetComponent<Renderer>().SetPropertyBlock( block, index );
					#else
					sharedMats = GetComponent<Renderer>().sharedMaterials;
					var t = sharedMats[0];
					sharedMats[0] = sharedMats[index];
					GetComponent<Renderer>().sharedMaterials = sharedMats;
					GetComponent<Renderer>().SetPropertyBlock( block );
					sharedMats[0] = t;
					GetComponent<Renderer>().sharedMaterials = sharedMats;

					#endif
					for ( int i = 0; i < materialProfiles[index].subRenders.Length; i++ ){
						if ( materialProfiles[index].subRenders[i] != null ){
							materialProfiles[index].subRenders[i].SetPropertyBlock( block );
						}
					}
				}
			}
		}




#region MESH PATCHERS
//================ XFUR - MESH PATCHER ===================


		public void XFur_GenerateShadowMesh( XFur_MaterialProperties mat ){
			


			if ( mat.subRenders.Length == 0 || mat.subRenders[0] == null ){

				GameObject baseRenderer = Instantiate(this.gameObject);

				DestroyImmediate( baseRenderer.GetComponent<XFur_System>() );

				var t = baseRenderer.GetComponentsInChildren<Transform>();

				for ( int i = 0; i < t.Length; i++ ){
					if ( t[i] != baseRenderer.transform )
						DestroyImmediate( t[i].gameObject );
				}

				baseRenderer.GetComponent<Renderer>().sharedMaterials = new Material[1];
				
				
				
				if ( mat.subRenders == null || mat.subRenders.Length!=8 || mat.subRenders[0] == null ){
						for (int i = 0; i < mat.subRenders.Length; i++ ){
							if ( mat.subRenders[i] ){
								DestroyImmediate(mat.subRenders[i].gameObject);
							}
						}

						mat.subRenders = new Renderer[8];

						for ( int i = 0; i < mat.subRenders.Length; i++ ){
							mat.subRenders[i] = Instantiate(baseRenderer).GetComponent<Renderer>();
							mat.subRenders[i].name = "XFur_SubRender_Shells_"+mat.originalMat.name+"_"+i;
							mat.subRenders[i].transform.parent = transform.parent;
						}

						
				}
				
				DestroyImmediate(baseRenderer.gameObject);

			}

			
			if ( GetComponent<SkinnedMeshRenderer>()){
				var s = new List<SkinnedMeshRenderer>();

				foreach( Renderer r in mat.subRenders ){
					if ( r!= null )
						s.Add( r.GetComponent<SkinnedMeshRenderer>() );
				}

				if ( database.XFur_GetShadowMesh( originalMesh, mat.furmatIndex, s.ToArray() ) ){
					return;
				}
				Debug.Log("Generating Shadow Mesh");
			}
			else{
				var s = new List<MeshFilter>();

				foreach( Renderer r in mat.subRenders ){
					if ( r!= null )
						s.Add( r.GetComponent<MeshFilter>() );
				}

				if ( database.XFur_GetShadowMesh( originalMesh, mat.furmatIndex, s.ToArray() ) ){
					return;
				}
				Debug.Log("Generating Shadow Mesh");
			}



			sharedMats = GetComponent<Renderer>().sharedMaterials;
			generateLayers = false;

			
			generateLayers = true;
			
		}

#if UNITY_EDITOR

		public void XFur_GenerateLayers(  ){
			if ( !generateLayers ){
				UnityEditor.EditorUtility.ClearProgressBar();
				return;
			}

			if ( layersSoFar == 24 ){
				UnityEditor.EditorUtility.ClearProgressBar();
				layersSoFar = 0;
				generateLayers = false;

				if ( GetComponent<SkinnedMeshRenderer>() ){
					var s = new List<SkinnedMeshRenderer>();

					foreach( Renderer r in materialProfiles[materialProfileIndex].subRenders ){
						if ( r!= null )
							s.Add( r.GetComponent<SkinnedMeshRenderer>() );
					}
					database.XFur_GetShadowMesh( originalMesh, materialProfileIndex, s.ToArray() );
				}
				else{
					var s = new List<MeshFilter>();

					foreach( Renderer r in materialProfiles[materialProfileIndex].subRenders ){
						if ( r!= null )
							s.Add( r.GetComponent<MeshFilter>() );
					}
					database.XFur_GetShadowMesh( originalMesh, materialProfileIndex, s.ToArray() );
				}
				return;
			}


			if ( Mesh.GetSubmesh(materialProfileIndex).vertexCount < 15000 ){
				GenerateShells(materialProfileIndex, 3);
				layersSoFar +=3;
				UnityEditor.EditorUtility.DisplayProgressBar("XFur Mesh Patcher 1.0", "Generating Fur Data", 1/24.0f*layersSoFar );

			}
			else{
				UnityEditor.EditorUtility.ClearProgressBar();
				layersSoFar = 0;
				generateLayers = false;
				Debug.LogError("Cannot use full advanced shadows with models with more than 15,000 vertices covered in fur");
				materialProfiles[materialProfileIndex].furmatShadowsMode = 0;
			}

			
		}

	void GenerateShells(int group, int shells){

		var UV0 = new List<Vector2>();
		var UV1 = new List<Vector4>();
		var UV2 = new List<Vector4>();
		var UV3 = new List<Vector4>();
		var tangents = new List<Vector4>();
		var colors = new List<Color>();
		var normals = new List<Vector3>(); 
		var boneWeights = new List<BoneWeight>();
		var vertices = new List<Vector3>();
		var triangles = new List<int>();

		var mesh = Mesh.GetSubmesh( group );
		mesh.bindposes = Mesh.bindposes;

		if ( mesh.vertexCount == 0 ){
			generateLayers = false;
			layersSoFar = 0;
			return;
		}

		for ( int i = 0; i < shells; i++ ){
			var uv1 = new List<Vector4>();
			var uv2 = new List<Vector4>();
			mesh.GetUVs(1, uv1);

			if ( uv1.Count < mesh.vertexCount ){
				uv1.Clear();
				uv1.AddRange( new Vector4[mesh.vertexCount] );
			}
			mesh.GetUVs(2, uv2);
			var uv3 = new Vector4[mesh.vertexCount];
			var tris = mesh.triangles;

			for ( int v = 0; v < mesh.vertexCount; v++ ){
				uv1[v] = new Vector3(uv1[v].x,uv1[v].y,(i+1+(layersSoFar))*(1/24.0f) );
				uv2[v] = new Vector4( mesh.vertices[v].x, mesh.vertices[v].y, mesh.vertices[v].z, uv2[v].w );
				uv3[v] = mesh.normals[v];

				var boneW = 0.0f;
				var boneI = 0;

				if ( mesh.boneWeights.Length > 0 ){
					if ( mesh.boneWeights[v].weight0 > boneW ){
					boneI = mesh.boneWeights[v].boneIndex0;
					boneW = mesh.boneWeights[v].weight0;
					}

					if ( mesh.boneWeights[v].weight1 > boneW ){
						boneI = mesh.boneWeights[v].boneIndex1;
						boneW = mesh.boneWeights[v].weight1;
					}

					if ( mesh.boneWeights[v].weight2 > boneW ){
						boneI = mesh.boneWeights[v].boneIndex2;
						boneW = mesh.boneWeights[v].weight2; 
					}

					if ( mesh.boneWeights[v].weight3 > boneW ){
						boneI = mesh.boneWeights[v].boneIndex3;
						boneW = mesh.boneWeights[v].weight3;
					}
	}
					uv3[v].w = boneI;
				}

			for  (int t = 0; t < tris.Length; t++ ){
				tris[t] += mesh.vertexCount*i;
			}

			UV0.AddRange( new List<Vector2>( mesh.uv ) );
			UV1.AddRange( uv1 );
			UV2.AddRange( uv2 );
			UV3.AddRange( new List<Vector4>( uv3 ) );
			vertices.AddRange( new List<Vector3>( mesh.vertices ) );
			normals.AddRange( new List<Vector3>( mesh.normals ) );
			tangents.AddRange( new List<Vector4>(mesh.tangents) );
			triangles.AddRange( new List<int>(tris) );
			boneWeights.AddRange( new List<BoneWeight>(mesh.boneWeights) );
			colors.AddRange( new List<Color>(mesh.colors) );

		}

		mesh.SetVertices( vertices );
		mesh.SetNormals( normals );
		mesh.SetColors( colors );
		mesh.triangles = triangles.ToArray();
		mesh.SetTangents( tangents );
		mesh.SetUVs( 0, UV0 );
		mesh.SetUVs( 1, UV1 );
		mesh.SetUVs( 2, UV2 );
		mesh.SetUVs( 3, UV3 );
		mesh.boneWeights = boneWeights.ToArray();
		mesh.bindposes = Mesh.bindposes;

		database.XFur_AddShadowMeshData( originalMesh, mesh, materialProfileIndex );

	}

#endif

		public void XFur_UpdateMeshData( bool forceUpdate = false ){

			if ( !database ){
				return;
			}
			
			if ( GetComponent<MeshFilter>() ){

				if ( !GetComponent<MeshFilter>().sharedMesh && originalMesh ){
					GetComponent<MeshFilter>().sharedMesh = originalMesh;
				}

				var mesh = Instantiate( GetComponent<MeshFilter>().sharedMesh );

				if ( !originalMesh )
					originalMesh = GetComponent<MeshFilter>().sharedMesh;

				var tris = new List<int>();

				foreach( XFur_MaterialProperties mats in materialProfiles ){
					if ( mats.furmatType == 2 || materialProfiles.Length == 1 ){
						tris.AddRange(mesh.GetTriangles(mats.furmatIndex));
					}
				}

				if ( database.XFur_ContainsMeshData(originalMesh) == -1 || forceUpdate ){
					var uv2 = new Vector4[mesh.vertexCount];
					var uv3 = new Vector4[mesh.vertexCount];
					int index = 0;
					var furV = new List<int>();
					for ( int v = 0; v < mesh.vertexCount; v++ ){
						
						uv2[v] = mesh.vertices[v];
						if ( tris.Contains(v) ){
							uv2[v].w = index;
							furV.Add(v);
							index++;
						}
						else{
							uv2[v].w = -1;
						}
						uv3[v] = mesh.normals[v];

						

					}

					
 
					mesh.SetUVs( 2, new List<Vector4>(uv2) );
					mesh.SetUVs( 3, new List<Vector4>(uv3) );

					database.XFur_AddBaseMeshData( originalMesh, mesh, furV.ToArray(), internalVersion );
					GetComponent<MeshFilter>().sharedMesh = database.XFur_GetPatchedMesh( originalMesh )==null?originalMesh:database.XFur_GetPatchedMesh( originalMesh );

					for ( int i = 0; i< materialProfiles.Length; i++ ){
						if ( materialProfiles[i].furmatType == 2 && materialProfiles[i].furmatShadowsMode == 1 )
							XFur_GenerateShadowMesh( materialProfiles[i] );
					}
				}
				else{
					GetComponent<MeshFilter>().sharedMesh = database.XFur_GetPatchedMesh( originalMesh )==null?originalMesh:database.XFur_GetPatchedMesh( originalMesh );
				}

			}
			else if ( GetComponent<SkinnedMeshRenderer>() ){

				if ( !GetComponent<SkinnedMeshRenderer>().sharedMesh && originalMesh ){
					GetComponent<SkinnedMeshRenderer>().sharedMesh = originalMesh;
				}
				
				var mesh = Instantiate( GetComponent<SkinnedMeshRenderer>().sharedMesh );

				if ( !originalMesh )
					originalMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;

				var tris = new List<int>();

				foreach( XFur_MaterialProperties mats in materialProfiles ){
					if ( mats.furmatType == 2 || materialProfiles.Length == 1 ){
						tris.AddRange(mesh.GetTriangles(mats.furmatIndex));
					}
				}

				if ( database.XFur_ContainsMeshData(originalMesh) == -1 || forceUpdate ){

					var uv2 = new Vector4[mesh.vertexCount];
					var uv3 = new Vector4[mesh.vertexCount];
					int index = 0;
					var furV = new List<int>();

					for ( int v = 0; v < mesh.vertexCount; v++ ){
						
						uv2[v] = mesh.vertices[v];
						if ( tris.Contains(v) ){
							uv2[v].w = index;
							furV.Add(v);
							index++;
						}
						else{
							uv2[v].w = -1;
						}
						uv3[v] = mesh.normals[v];
						
						var boneW = 0.0f;
						var boneI = 0;
						
						if ( mesh.boneWeights!=null && mesh.boneWeights.Length > v ){
							if ( mesh.boneWeights[v].weight0 > boneW ){
								boneI = mesh.boneWeights[v].boneIndex0;
								boneW = mesh.boneWeights[v].weight0;
							}

							if ( mesh.boneWeights[v].weight1 > boneW ){
								boneI = mesh.boneWeights[v].boneIndex1;
								boneW = mesh.boneWeights[v].weight1;
							}

							if ( mesh.boneWeights[v].weight2 > boneW ){
								boneI = mesh.boneWeights[v].boneIndex2;
								boneW = mesh.boneWeights[v].weight2; 
							}

							if ( mesh.boneWeights[v].weight3 > boneW ){
								boneI = mesh.boneWeights[v].boneIndex3;
								boneW = mesh.boneWeights[v].weight3;
							}
						}

						uv3[v].w = boneI;
					}



					mesh.SetUVs( 2, new List<Vector4>(uv2) );
					mesh.SetUVs( 3, new List<Vector4>(uv3) );

					Debug.Log("Mesh "+mesh.name+" has been patched to use with XFur. UV3 and UV4 channels have been generated" );
					database.XFur_AddBaseMeshData( originalMesh, mesh, furV.ToArray(), internalVersion );
					GetComponent<SkinnedMeshRenderer>().sharedMesh = database.XFur_GetPatchedMesh( originalMesh )==null?originalMesh:database.XFur_GetPatchedMesh( originalMesh );
				}
				else{
					GetComponent<SkinnedMeshRenderer>().sharedMesh = database.XFur_GetPatchedMesh( originalMesh )==null?originalMesh:database.XFur_GetPatchedMesh( originalMesh );
				}


				

			}
		}

	#endregion

#endregion

#region EXTERNAL COMMANDS

	public Texture2D XFur_CreateVertexData( string dataType ){

		switch( dataType.ToUpper() ){
			case "FUR_VERTICES":
			var index = database.XFur_ContainsMeshData(originalMesh);

			if ( GetComponent<MeshFilter>() ){
				
				//var mesh = GetComponent<MeshFilter>().sharedMesh;
				var vertices = database.meshData[index].furVertices;
				int side = Mathf.NextPowerOfTwo( (int)Mathf.Sqrt(vertices.Length) );
				Texture2D dat = new Texture2D( side, side, TextureFormat.ARGB32, false, true );
				var col = new Color32[side*side];				
				for ( int i = 0; i < vertices.Length; i++ ){
					col[i] = Color.black;
				}

				dat.SetPixels32(col);
				dat.Apply();
				return dat;
			}
			else if ( GetComponent<SkinnedMeshRenderer>() ){
				
				//var mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
				var vertices = database.meshData[index].furVertices;
				int side = Mathf.NextPowerOfTwo( (int)Mathf.Sqrt(vertices.Length) );
				Texture2D dat = new Texture2D( side, side, TextureFormat.ARGB32, false, true );
				var col = new Color[side*side];
				for ( int i = 0; i < vertices.Length; i++ ){
					col[i] = Color.black;
				}

				dat.SetPixels(col);
				dat.Apply();
				return dat;
			}
			return null;
		}

		return null;
	}


	public void XFur_UpdateNormals(){

		var index = database.XFur_ContainsMeshData(originalMesh);

		if ( !normalTexture ){
			

			if ( GetComponent<MeshFilter>() ){
				
				var mesh = GetComponent<MeshFilter>().sharedMesh;
				int side = Mathf.NextPowerOfTwo( (int)Mathf.Sqrt(mesh.vertexCount) );
				Texture2D norms = new Texture2D( side, side, TextureFormat.ARGB32, false );
				var col = new Color[side*side];
				for ( int i = 0; i < mesh.vertexCount; i++ ){
					col[i] = new Color(mesh.normals[i].x, mesh.normals[i].y, mesh.normals[i].z, 0 );
				}

				norms.SetPixels(col);
				norms.Apply();
				normalTexture = norms;
			}
			else if ( GetComponent<SkinnedMeshRenderer>() ){
				
				var mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
				int side = Mathf.NextPowerOfTwo( (int)Mathf.Sqrt(mesh.vertexCount) );
				Texture2D norms = new Texture2D( side, side );
				var col = new Color[side*side];
				for ( int i = 0; i < database.meshData[index].furVertices.Length; i++ ){
					col[i] = new Color(mesh.normals[database.meshData[index].furVertices[i]].x, mesh.normals[database.meshData[index].furVertices[i]].y, mesh.normals[database.meshData[index].furVertices[i]].z, 0 );
				}

				norms.SetPixels(col);
				norms.Apply();
				normalTexture = norms;
			}

		}
		else{
			if ( GetComponent<MeshFilter>() ){
				var mesh = GetComponent<MeshFilter>().sharedMesh;
				var col = new Color[normalTexture.width*normalTexture.height];
				var noms = mesh.normals;
				var vertices = database.meshData[index].furVertices;
				for ( int i = 0; i < vertices.Length; i++ ){
					var n = transform.rotation*noms[vertices[i]];
					col[i] = new Color( n.x, n.y, n.z, 0 );
				}

				normalTexture.SetPixels(col);
				normalTexture.Apply();
			}
			else if ( GetComponent<SkinnedMeshRenderer>() ){
				var mesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
				var col = new Color[normalTexture.width*normalTexture.height];
				var noms = mesh.normals;
				var vertices = database.meshData[index].furVertices;
				for ( int i = 0; i < vertices.Length; i++ ){
					var n = transform.rotation*noms[vertices[i]];
					col[i] = new Color( n.x, n.y, n.z, 0 );
				}
				normalTexture.SetPixels(col);
				normalTexture.Apply();
			}
		}
	}

	#endregion



#region INTERFACES

	public string Version{
		get{
			return internalVersion;
		}
	}

	public Mesh Mesh{
		get{
			if ( GetComponent<MeshFilter>() ){
				return GetComponent<MeshFilter>().sharedMesh; 
			}
			else{
				return GetComponent<SkinnedMeshRenderer>().sharedMesh;
			}
		}
	}

	public Mesh OriginalMesh{
		get{
			return originalMesh;
		}
	}


	public Texture2D NormalTexture{
		get{
			return normalTexture;
		}
	}

	public string[] FurMatNames{
		get{
			var names = new List<string>();
			for ( int i = 0; i < sharedMats.Length; i++ ){
				names.Add( i+". "+materialProfiles[i].originalMat.name );
			}
			return names.ToArray();
		}
	}
	
	
	public GUIContent[] FurMatGUIS{
		get{
			var names = new List<GUIContent>();
			for ( int i = 0; i < sharedMats.Length; i++ ){
				names.Add( new GUIContent( i+". "+materialProfiles[i].originalMat.name ) );
			}
			return names.ToArray();
		}
	}

	public int FurMaterials{
		get{
			var furs = 0;
			for ( int i = 0; i < materialProfiles.Length; i++ ){
				if  ( materialProfiles[i].originalMat.shader.name.Contains("XFur") && !materialProfiles[i].originalMat.shader.name.Contains("Legacy") ){
					furs++;
				}
			}
			return furs;
		}
	}

	public XFur_MaterialProperties[] FurProfiles{
		get{
			return materialProfiles;
		}
	}

	#endregion


	}


	[System.Serializable]
	public class XFur_MaterialProperties{
		///<summary> Original material reference</summary>
		public Material originalMat;
		///<summary> INTERNAL. DO NOT CHANGE. Used to detect material changes</summary>
		public Material buffOriginalMat = null;
		///<summary> DO NOT CHANGE. Material type. 0 = non fur material, 1 = legacy fur, 2 = fur material.</summary>
		public int furmatType;
		///<summary> DO NOT CHANGE. Fur Material Index</summary>
		public int furmatIndex = 0;
		///<summary> INTERNAL. Used for samples amount</summary>
		public int furmatSamples = 2;
		///<summary> Should this material use triplanar ¨Projection? 0 = no, 1 = yes</summary>
		public int furmatTriplanar;
		/// <summary>Forces all grooming data to be based on the secondary UV map instead of the primary UVs. Useful to avoid errors on mirrored / symmetric UV projections by using an alternative UV map stored on the secondary channel</summary>
		public int furmatForceUV2Grooming;
		///<summary> 0 = Disable self-collisions, 1 = Enabled</summary>
		public int furmatCollision = 1;
		///<summary> Fur shadows mode. 0 = Standard, 1 = Full (Do not change at runtime)</summary>
		public int furmatShadowsMode = 0;
		///<summary> INTERNAL. Read skin settigns from Material (0) or from Instance (1)</summary>
		public int furmatReadBaseSkin = 0;
		///<summary> INTERNAL. Read fur settigns from Material (0) or from Instance (1)</summary>
		public int furmatReadBaseFur = 0;
		///<summary> INTERNAL. Read fur noise map from Material (0) or from Instance (1)</summary>
		public int furmatReadFurNoise = 0;
		///<summary> Not yet implemented. Anisotropic offset value</summary>
		public float furmatAnisoOffset = 0.5f;
		///<summary> Triplanar Scale</summary>
		public float furmatTriplanarScale = 0.5f; //The triplanar scale used in the projections
		///<summary> Main Fur Material maps</summary>
		public Texture2D furmatBaseTex, furmatNormalmap, furmatGlossSpecular, furmatOcclusion, furmatFurColorMap, furmatFurNoiseMap;
		///<summary>Fur Data maps for Fur parameters and grooming</summary>
		public Texture2D furmatData0, furmatData1;
		///<summary>DO NOT SET. Used internally by XFur Painter</summary>
		public Texture2D furmatPainter;
		///<summary> Base specular color</summary>
		public Color furmatSpecular = new Color(0.2f,0.2f,0.2f,1);
		///<summary> Fur specular color</summary>
		public Color furmatFurSpecular = new Color(0.2f,0.2f,0.2f,1);
		///<summary> Base (skin) color</summary>
		public Color furmatBaseColor = Color.white;
		///<summary>Main fur color</summary>
		public Color furmatFurColorA = Color.white;
		///<summary>Secondary fur color</summary>
		public Color furmatFurColorB = Color.white;
		///<summary>Base (skin) smoothness</summary>
		public float furmatSmoothness = 0.25f;
		///<summary> Fur smoothness</summary>
		public float furmatFurSmoothness = 0.25f;
		///<summary> Fur Occlusion</summary>
		public float furmatFurOcclusion = 0.5f;
		///<summary> Fur Cutoff value</summary>
		public float furmatFurCutoff = 0.25f;
		///<summary>Global fur length</summary>
		public float furmatFurLength = 0.25f;
		///<summary>Global fur thickness</summary>
		public float furmatFurThickness = 0.25f;
		///<summary>Main UV Scaling</summary>
		public float furmatUV1 = 1;
		///<summary>First Fur UV Scaling (affects triplanar)</summary>
		public float furmatFurUV1 = 2;
		///<summary>Secondary Fur UV Scaling (affects triplanar)</summary>
		public float furmatFurUV2 = 4;
		///<summary>DO NOT CHANGE.</summary>
		public bool originalSynch;
		///<summary> Fur Rim Color</summary>
		public Color furmatFurRim = Color.gray;
		///<summary>Fur Rim Power</summary>
		public float furmatFurRimPower = 0.2f;
		///<summary> INDERNAL. DO NOT CHANGE</summary>
		public Renderer[] subRenders = new Renderer[0];
		///<summary>Global Fur Direction to be multiplied by the grooming texture</summary>
		public Vector4 furmatDirection = new Vector4(0.2f,0.2f,0.2f,0.2f);

		public void SynchToOriginalMat(){
			

			if ( originalMat == buffOriginalMat ){
				return;
			}
			else{
				buffOriginalMat = originalMat;
			}

			if ( !originalMat.shader.name.Contains("XFur") ){
				furmatType = 0;
				return;
			}
			else{
				furmatType = originalMat.shader.name.Contains("Legacy")?1:2;
			}
			if ( furmatType == 2 ){
				furmatBaseColor = originalMat.GetColor("_BaseColor");
				furmatFurColorA = originalMat.GetColor("_FurColorA");
				furmatFurColorB = originalMat.GetColor("_FurColorB");
				furmatFurRim = originalMat.GetColor("_RimColor");
				furmatFurSpecular = originalMat.GetColor("_FurSpecular");
				furmatSpecular = originalMat.GetColor("_BaseSpecular");

				furmatBaseTex = (Texture2D)originalMat.GetTexture("_BaseTex");
				furmatOcclusion = (Texture2D)originalMat.GetTexture("_OcclusionMap");
				furmatNormalmap = (Texture2D)originalMat.GetTexture("_Normalmap");
				furmatGlossSpecular = (Texture2D)originalMat.GetTexture("_GlossSpecular");
				furmatFurNoiseMap = (Texture2D)originalMat.GetTexture("_FurNoiseMap");
				furmatData0 = (Texture2D)originalMat.GetTexture("_FurData0");
				furmatData1 = (Texture2D)originalMat.GetTexture("_FurData1");
				furmatBaseTex = (Texture2D)originalMat.GetTexture("_BaseTex");
				furmatTriplanar = (int)originalMat.GetFloat("_TriplanarMode");
				furmatForceUV2Grooming = (int)originalMat.GetFloat("_UV2Grooming");
				furmatTriplanarScale = originalMat.GetFloat("_TriplanarScale");
				furmatFurRimPower = originalMat.GetFloat("_FurRimStrength");
				furmatFurLength = originalMat.GetFloat("_FurLength");
				furmatFurThickness = originalMat.GetFloat("_FurThin");
				furmatFurOcclusion = originalMat.GetFloat("_FurOcclusion");
				furmatFurCutoff = originalMat.GetFloat("_FurCutoff");
				furmatUV1 = originalMat.GetFloat("_UV0Scale1");
				furmatFurUV1 = originalMat.GetFloat("_UV1Scale1");
				furmatFurUV2 = originalMat.GetFloat("_UV1Scale2");
				furmatFurSmoothness = originalMat.GetFloat("_FurSmoothness");
				furmatSmoothness = originalMat.GetFloat("_BaseSmoothness");
				furmatDirection = originalMat.GetVector("_FurDirection");
			}

		}

	}
}
