/*
XFur Studio™ - XFur Painter™ 2.0
XFur Studio and XFur Painter are trademarks of Jorge Pinal Negrete. Copyright© 2015-2018, Jorge Pinal Negrete. All Rights Reserved.
*/

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace XFurStudio{

	public class XFur_Painter2 : MonoBehaviour {

		[System.Serializable]
		public struct XFurPainter_PainterData{
			public List<Texture2D> undoSpaces;
			public Texture2D painterTex;
			public Texture2D furData0;
			public Texture2D furData1;

			public Texture2D originalFDat0, originalFDat1;

			public Material originalMat;
		}

		private float distance = 1.0f;
		[Range(256,2048)]public int textureSize = 512;
		public bool startWithFullFur;
		public KeyCode movePivotKey = KeyCode.Mouse2;
		public KeyCode rotationKey = KeyCode.Mouse2;
		private float orbitX,orbitY;
		private Vector3 origin;
		private XFurPainter_PainterData[] perMaterialData;
		[HideInInspector]public GUISkin customSkin;
		[HideInInspector]public Texture2D icon0N,icon0A,icon1N,icon1A,icon2N,icon2A,icon3N,icon3A,icon4N,icon4A,icon5N,icon5A,icon6N,icon6A,icon7N,icon7A,icon8N,icon8A;

				

		//INTERNAL
		private XFur_System target;
		private int brushSize = 16;
		private float brushStrength = 5.0f;
		private int tool = 0;
		private string toolTip;
		private bool isPainting;
		private float tStep;
		private bool ready;


		// Use this for initialization
		IEnumerator Start () {

			yield return new WaitForSeconds(1.0f);
			target = GameObject.FindObjectOfType<XFur_System>();
			if ( target ){
				transform.forward = -target.transform.root.forward;
				orbitX = transform.eulerAngles.y;
				origin = target.transform.position;
				distance = target.GetComponent<Renderer>().bounds.extents.magnitude+1;


				if ( !target.GetComponent<MeshCollider>() ){
					
					var tMesh = new Mesh();
					
					if ( target.GetComponent<SkinnedMeshRenderer>() ){
						target.GetComponent<SkinnedMeshRenderer>().BakeMesh(tMesh);
					}

					target.gameObject.AddComponent<MeshCollider>();

					target.GetComponent<MeshCollider>().sharedMesh = target.GetComponent<SkinnedMeshRenderer>()?tMesh:target.Mesh;
				}
			
				perMaterialData = new XFurPainter_PainterData[target.materialProfiles.Length];

				for ( int i = 0; i < perMaterialData.Length; i++ ){
					if ( target.materialProfiles[i].furmatType == 2 )
						PrepareMaterial( i );
				}
			}

			ready = true;	
			yield break;	
		}


		void PrepareMaterial( int index ){
			
			var furs = target.FurMaterials;
			#if UNITY_2018_1_OR_NEWER
			furs = 0;
			#endif
			Texture2D data0 = null, data1 = null;

			perMaterialData[index].undoSpaces = new List<Texture2D>();

			if ( furs > 1 || target.materialProfiles[index].furmatReadBaseFur == 0 ){
				data0 = (Texture2D)target.materialProfiles[index].originalMat.GetTexture("_FurData0");
				data1 = (Texture2D)target.materialProfiles[index].originalMat.GetTexture("_FurData1");
				perMaterialData[index].originalFDat0 = data0;
				perMaterialData[index].originalFDat1 = data1;
				perMaterialData[index].originalMat = target.materialProfiles[index].originalMat;
				
			}
			else{
				data0 = (Texture2D)target.materialProfiles[index].furmatData0;
				data1 = (Texture2D)target.materialProfiles[index].furmatData1;
				perMaterialData[index].originalMat = target.GetComponent<Renderer>().sharedMaterials[index];
			}

			perMaterialData[index].painterTex = new Texture2D(256,256);

			perMaterialData[index].painterTex.SetPixels(new Color[256*256]);
			perMaterialData[index].painterTex.Apply();

			if ( data0 ){
				textureSize = data0.width;
				perMaterialData[index].furData0 = new Texture2D(data0.width,data0.height, TextureFormat.ARGB32, true, true );
				perMaterialData[index].furData0.SetPixels(data0.GetPixels());
				perMaterialData[index].furData0.Apply();
			}
			
			if ( data1 ){
				textureSize = data1.width;
				perMaterialData[index].furData1 = new Texture2D(data1.width,data1.height, TextureFormat.ARGB32, true, true );
				perMaterialData[index].furData1.SetPixels(data1.GetPixels());
				perMaterialData[index].furData1.Apply();
			}

			Color[] furColors = new Color[textureSize*textureSize];
			Color[] groomColors = new Color[textureSize*textureSize];

			for( int i = 0; i < furColors.Length; i++ ){
				furColors[i] = new Color( startWithFullFur?1:0 , 1, 1, 1 );
				groomColors[i] = new Color(0.5f,0.5f,0.5f,1);
			}

			if ( !data0 ){
				perMaterialData[index].furData0 = new Texture2D(textureSize,textureSize, TextureFormat.ARGB32, true, true );
				perMaterialData[index].furData0.SetPixels(furColors);
				perMaterialData[index].furData0.Apply();
			}
			

			
			if ( !data1 ){
				perMaterialData[index].furData1 = new Texture2D(textureSize,textureSize, TextureFormat.ARGB32, true, true );
				perMaterialData[index].furData1.SetPixels(groomColors);
				perMaterialData[index].furData1.Apply();
			}	
			
			if ( furs > 1  || target.materialProfiles[index].furmatReadBaseFur == 0  ){
				target.materialProfiles[index].furmatData0 = perMaterialData[index].furData0;
				target.materialProfiles[index].furmatData1 = perMaterialData[index].furData1;
				target.materialProfiles[index].originalMat.SetTexture("_FurData0", perMaterialData[index].furData0 );
				target.materialProfiles[index].originalMat.SetTexture("_FurData1", perMaterialData[index].furData1 );
				target.GetComponent<Renderer>().sharedMaterials[index].SetTexture("_FurPainter", perMaterialData[index].painterTex );
			}
			else{
				target.GetComponent<Renderer>().sharedMaterials[index].SetTexture("_FurPainter", perMaterialData[index].painterTex );
				target.materialProfiles[index].furmatData0 = perMaterialData[index].furData0;
				target.materialProfiles[index].furmatData1 = perMaterialData[index].furData1;
			}


			perMaterialData[index].undoSpaces.Add(new Texture2D(textureSize,textureSize) );
			perMaterialData[index].undoSpaces[0] = Instantiate(perMaterialData[index].furData0);

		}


		void OnGUI(){
			
			float scale = Screen.height/600.0f;
			GUIStyle italicSmall = new GUIStyle();
			GUIStyle bold = new GUIStyle();
			GUIStyle buttonStyle = new GUIStyle();
			italicSmall.normal.textColor = Color.white;
			italicSmall.fontSize = (int)(9*scale);
			italicSmall.fontStyle = FontStyle.Italic;
			bold.normal.textColor = Color.white;
			bold.fontSize = (int)(14*scale);
			bold.fontStyle = FontStyle.Bold;
			GUI.skin = customSkin;
			GUILayout.BeginArea( new Rect( 0, Screen.height-32*scale, Screen.width, 32*scale ) );
			GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
			GUILayout.Label( "XFur Painter™ 2.0. Copyright© 2018, Jorge Pinal Negrete", italicSmall );
			GUILayout.FlexibleSpace();GUILayout.EndHorizontal();
			GUILayout.EndArea();

			italicSmall.fontSize = (int)(14*scale);

			GUILayout.BeginArea( new Rect( 32*scale, 50*scale, 200*scale, 30*scale ) );
			GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();GUILayout.Label( "Fur Appearance Tools", bold );GUILayout.FlexibleSpace();GUILayout.EndHorizontal();
			GUILayout.EndArea();

			GUILayout.BeginArea( new Rect( 32*scale, 128*scale, 200*scale, 30*scale ) );
			GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();GUILayout.Label( "Fur Grooming Tools", bold );GUILayout.FlexibleSpace();GUILayout.EndHorizontal();
			GUILayout.EndArea();

			GUILayout.BeginArea( new Rect( 32*scale, 210*scale, 300*scale, 90*scale ) );
			GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();GUILayout.Label( "Brush Settings", bold );GUILayout.Space(100);GUILayout.FlexibleSpace();GUILayout.EndHorizontal();
			GUILayout.Space(8);
			bold.fontSize = (int)(12*scale);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Brush Size", bold, GUILayout.Width(90*scale) );  GUILayout.Space(12);
			brushSize = (int)GUILayout.HorizontalSlider( brushSize, 1*textureSize/256, 32*textureSize/256, GUILayout.Width(100*scale), GUILayout.MaxHeight(16*scale) );
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Brush Strength", bold, GUILayout.Width(90*scale) ); GUILayout.Space(12);
			brushStrength = GUILayout.HorizontalSlider( brushStrength, 1, 8, GUILayout.Width(100*scale), GUILayout.MaxHeight(16*scale) );
			GUILayout.EndHorizontal();

			#if UNITY_EDITOR
			GUILayout.Space(12);
			if ( GUILayout.Button( "Export Fur Data", GUILayout.Width(200*scale) ) ){
				var path = EditorUtility.SaveFilePanelInProject("Export Textures","New Fur Mask.png","png", "Export the current fur mask" );
				if (path.Length != 0){
					for ( int i = 0; i < perMaterialData.Length; i++ ){
						if ( perMaterialData[i].furData0 && perMaterialData[i].furData1 ){
							var pngData = perMaterialData[i].furData0.EncodeToPNG();
							if (pngData != null){
								File.WriteAllBytes(path.Replace(".png","_"+target.materialProfiles[i].originalMat.name+"_Mask.png"), pngData);
								AssetDatabase.Refresh();
								var a = (TextureImporter) AssetImporter.GetAtPath(path.Replace(".png","_"+target.materialProfiles[i].originalMat.name+"_Mask.png"));
                                #if UNITY_5_6_OR_NEWER
								a.sRGBTexture = false;
								#else
								a.linearTexture = true;
								#endif
								a.isReadable = true;
                                a.SaveAndReimport();
                                AssetDatabase.Refresh();
							}

							pngData = perMaterialData[i].furData1.EncodeToPNG();
							
							if (pngData != null){
								File.WriteAllBytes(path.Replace(".png","_"+target.materialProfiles[i].originalMat.name+"_Groom.png"), pngData);
								AssetDatabase.Refresh();
								var a = (TextureImporter) AssetImporter.GetAtPath(path.Replace(".png","_"+target.materialProfiles[i].originalMat.name+"_Groom.png"));
                                #if UNITY_5_6_OR_NEWER
								a.sRGBTexture = false;
								#else
								a.linearTexture = true;
								#endif
								a.isReadable = true;
                                a.SaveAndReimport();
                                AssetDatabase.Refresh();
								
							}
						}
					}
				}
			}
			#endif

			bold.fontSize = (int)(14*scale);
			GUILayout.EndArea();


			GUI.BeginGroup( new Rect( 32*scale, 32*scale, 200*scale, 280*scale) );			

			if ( GUI.Button( new Rect( 8*scale, 40*scale, 40*scale, 40*scale ), tool==0?icon0A:icon0N, buttonStyle ) ){
				tool = 0;
			}

			if ( GUI.Button( new Rect( 56*scale, 40*scale, 40*scale, 40*scale ), tool==1?icon1A:icon1N, buttonStyle ) ){
				tool = 1;
			}

			if ( GUI.Button( new Rect( 104*scale, 40*scale, 40*scale, 40*scale ), tool==2?icon3A:icon3N, buttonStyle ) ){
				tool = 2;
			}

			if ( GUI.Button( new Rect( 152*scale, 40*scale, 40*scale, 40*scale ), tool==3?icon2A:icon2N, buttonStyle ) ){
				tool = 3;
			}

			if ( GUI.Button( new Rect( 8*scale, 118*scale, 40*scale, 40*scale ), tool==4?icon4A:icon4N, buttonStyle ) ){
				tool = 4;
			}

			if ( GUI.Button( new Rect( 56*scale, 118*scale, 40*scale, 40*scale ), tool==5?icon5A:icon5N, buttonStyle ) ){
				tool = 5;
			}
			

			GUI.EndGroup();

			switch( tool ){
				default:
				toolTip = "";
				break;
				
				case 0 :
				toolTip = "Use the numeric keys 1-6 to select a tool\nLeft Shift+Mouse Wheel = Brush Size\nLeft Control+Mouse Wheel = Brush Strength\n\nSelected Tool : 1. Shaving Tool.\nLeft Click = Add Fur\nRight Click = Shave Fur";
				break;

				case 1 :
				toolTip = "Use the numeric keys 1-6 to select a tool\nLeft Shift+Mouse Wheel = Brush Size\nLeft Control+Mouse Wheel = Brush Strength\n\nSelected Tool : 2. Trimming Tool.\nLeft Click = Longer Fur\nRight Click = Shorter Fur";
				break;

				case 2 :
				toolTip = "Use the numeric keys 1-6 to select a tool\nLeft Shift+Mouse Wheel = Brush Size\nLeft Control+Mouse Wheel = Brush Strength\n\nSelected Tool : 4. Shadowing Tool.\nLeft Click = Darken Fur\nRight Click = Bright Fur";
				break;

				case 3 :
					toolTip = "Use the numeric keys 1-6 to select a tool\nLeft Shift+Mouse Wheel = Brush Size\nLeft Control+Mouse Wheel = Brush Strength\n\nSelected Tool : 3. Thinning tool.\nLeft Click = Thinner Fur\nRight Click = Thicker Fur";
				break;

				case 4 :
				toolTip = "Use the numeric keys 1-6 to select a tool\nLeft Shift+Mouse Wheel = Brush Size\nLeft Control+Mouse Wheel = Brush Strength\n\nSelected Tool : 5. Grooming Tool.\nLeft Click+Drag to groom the fur\nRight Click to remove any grooming";
				break;

				case 5 :
				toolTip = "Use the numeric keys 1-6 to select a tool\nLeft Shift+Mouse Wheel = Brush Size\nLeft Control+Mouse Wheel = Brush Strength\n\nSelected Tool : 6. Fur Fixer.\nLeft Click = Add Fur Stiffness\nRight Click = Reduce Fur Stiffness";
				break;

			}

			GUI.BeginGroup( new Rect( 32*scale, Screen.height-130*scale, 256*scale, 110*scale ) );
			GUI.TextArea( new Rect(0,0, 256*scale, 110*scale ), toolTip, italicSmall ); 
			GUI.EndGroup();
			
			

		}
		
		// Update is called once per frame
		void LateUpdate () {
			if (!ready){
				return;
			}
			MouseOrbit();

			PaintMesh();

			//targetMaterial.SetTexture("_FurData" , perMaterialData[index].furData0 );
		}


		void PaintMesh(){

			if ( target ){
				foreach( Material m in target.GetComponent<Renderer>().sharedMaterials ){
					m.EnableKeyword( "XFUR_PAINTER" );
				}
			}
			
			var secondUVS = 0;

			if ( target.materialProfiles[target.materialProfileIndex].furmatForceUV2Grooming == 1 ){
				secondUVS = tool >= 4? 1:0;
				target.GetComponent<Renderer>().sharedMaterials[target.materialProfileIndex].SetFloat("_UV2Painting", secondUVS );
			}

			if ( Input.GetKeyDown("1") ){
				tool = 0;
			}

			if ( Input.GetKeyDown("2") ){
				tool = 1;
			}

			if ( Input.GetKeyDown("3") ){
				tool = 2;
			}

			if ( Input.GetKeyDown("4") ){
				tool = 3;
			}

			if ( Input.GetKeyDown("5") ){
				tool = 4;
			}

			if ( Input.GetKeyDown("6") ){
				tool = 5;
			}

			if ( Input.GetKey(KeyCode.LeftShift) ){
				brushSize = (int)Mathf.Clamp( brushSize+Input.GetAxis("Mouse ScrollWheel")*32, textureSize/256, 32*textureSize/256 );
			}


			if ( Input.GetKey(KeyCode.LeftControl) ){
				brushStrength = Mathf.Clamp( brushStrength+Input.GetAxis("Mouse ScrollWheel")*12, 1, 8 );
			}


			if ( Input.GetMouseButton(0) ){
				isPainting = true;
				RaycastHit hit;

				if ( Physics.Raycast( Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 16 ) ){
					var t = (secondUVS==0?hit.textureCoord:hit.textureCoord2)*textureSize;
					if ( tool < 4 ){
						Color[] colors = new Color[brushSize*brushSize];
						Color[] ogColors = perMaterialData[target.materialProfileIndex].furData0.GetPixels( (int)Mathf.Clamp( t.x-(brushSize*0.5f), 0, textureSize-brushSize ), (int)Mathf.Clamp( t.y-(brushSize*0.5f), 0, textureSize-brushSize ), brushSize, brushSize );
						int tempX = 0;
						int tempY = 0;

						for ( int i = 0; i < colors.Length; i++ ){
							tempX ++;
							if ( tempX >= brushSize ){
								tempY ++;
								tempX = 0;
							}
							float ratio = Vector2.Distance(new Vector2(tempX,tempY), new Vector2(brushSize*0.5f,brushSize*0.5f));
							float maxStrength = ratio>brushSize*0.5f?0:(1-(ratio/(brushSize*0.5f)));


							switch( tool ){
								case 0:
								colors[i] = Vector4.Min( (ogColors[i]+new Color(1*maxStrength,0,0,0)), Vector4.one );
								break;

								case 1:
								colors[i] = Vector4.Min( (ogColors[i]+new Color(0,maxStrength*brushStrength*Time.deltaTime*brushStrength, 0, 0)), Vector4.one );
								break;

								case 2:
								colors[i] = Vector4.Min( (ogColors[i]+new Color(0, 0, maxStrength*brushStrength*Time.deltaTime*brushStrength, 0)), Vector4.one );
								break;

								case 3:
								colors[i] = Vector4.Min( (ogColors[i]+new Color( 0, 0, 0, maxStrength*brushStrength*Time.deltaTime*brushStrength)), Vector4.one );
								break;
							}
						} 

						perMaterialData[target.materialProfileIndex].furData0.SetPixels( (int)Mathf.Clamp( t.x-(brushSize*0.5f), 0, textureSize-brushSize ), (int)Mathf.Clamp( t.y-(brushSize*0.5f), 0, textureSize-brushSize ), brushSize, brushSize, colors );
						perMaterialData[target.materialProfileIndex].furData0.Apply();
					}
					else{
						Color[] colors = new Color[brushSize*brushSize];
						Color[] ogColors = perMaterialData[target.materialProfileIndex].furData1.GetPixels( (int)Mathf.Clamp( t.x-(brushSize*0.5f), 0, textureSize-brushSize ), (int)Mathf.Clamp( t.y-(brushSize*0.5f), 0, textureSize-brushSize ), brushSize, brushSize );
						int tempX = 0;
						int tempY = 0;

						
						var skinned = target.GetComponent<SkinnedMeshRenderer>();
						var tUP = transform.up;
						var tRT = transform.right;
						var tFW = !skinned?target.transform.forward:target.GetComponent<SkinnedMeshRenderer>().rootBone.forward;
						var targetUp = !skinned?target.transform.up:target.GetComponent<SkinnedMeshRenderer>().rootBone.up;
						var targetRight = !skinned?target.transform.right:target.GetComponent<SkinnedMeshRenderer>().rootBone.right;

						var dotX = Vector3.Dot(tUP, targetRight )*Input.GetAxis("Mouse Y")*Time.deltaTime*3*brushStrength + Vector3.Dot(tRT, targetRight)*Input.GetAxis("Mouse X")*Time.deltaTime*3*brushStrength;
						var dotY = Vector3.Dot(tUP, targetUp )*Input.GetAxis("Mouse Y")*Time.deltaTime*3*brushStrength;
						var dotZ = Vector3.Dot(tRT, tFW)*Input.GetAxis("Mouse X")*Time.deltaTime*3*brushStrength + Vector3.Dot( tUP, tFW )*Input.GetAxis("Mouse Y")*Time.deltaTime*3*brushStrength;

						for ( int i = 0; i < colors.Length; i++ ){

							tempX ++;
							if ( tempX >= brushSize ){
								tempY ++;
								tempX = 0;
							}

							float ratio = Vector2.Distance(new Vector2(tempX,tempY), new Vector2(brushSize*0.5f,brushSize*0.5f));
							float maxStrength = ratio>brushSize*0.5f?0:(1-(ratio/(brushSize*0.5f)));

							
							if ( tool == 4 ){
								colors[i] = ogColors[i]+new Color( dotX*maxStrength, dotY*maxStrength, dotZ*maxStrength, ogColors[i].a );
							}

							if ( tool == 5 ){
								colors[i] = Vector4.Max( (ogColors[i]-new Color( 0, 0, 0, maxStrength*brushStrength*Time.deltaTime*brushStrength)), Vector4.zero );
							}


						} 

						perMaterialData[target.materialProfileIndex].furData1.SetPixels( (int)Mathf.Clamp( t.x-(brushSize*0.5f), 0, textureSize-brushSize ), (int)Mathf.Clamp( t.y-(brushSize*0.5f), 0, textureSize-brushSize ), brushSize, brushSize, colors );
						perMaterialData[target.materialProfileIndex].furData1.Apply();
					}
				
				}

			

		}


		if ( Input.GetMouseButton(1) ){
			isPainting = true;
			RaycastHit hit;

			if ( Physics.Raycast( Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 16 ) ){
				if ( tool < 4 ){	
					var t = (secondUVS==0?hit.textureCoord:hit.textureCoord2)*textureSize;
					Color[] colors = new Color[brushSize*brushSize];
					Color[] ogColors = perMaterialData[target.materialProfileIndex].furData0.GetPixels( (int)Mathf.Clamp( t.x-(brushSize*0.5f), 0, textureSize-brushSize ), (int)Mathf.Clamp( t.y-(brushSize*0.5f), 0, textureSize-brushSize ), brushSize, brushSize );
					int tempX = 0;
					int tempY = 0;

					for ( int i = 0; i < colors.Length; i++ ){
						tempX ++;
						if ( tempX >= brushSize ){
							tempY ++;
							tempX = 0;
						}
						float ratio = Vector2.Distance(new Vector2(tempX,tempY), new Vector2(brushSize*0.5f,brushSize*0.5f));
						float maxStrength = ratio>brushSize*0.5f?0:(1-(ratio/(brushSize*0.5f)));


						switch( tool ){
							case 0:
							colors[i] = Vector4.Max( (ogColors[i]-new Color(1*maxStrength,0,0,0)), new Vector4(0,0.1f,0,0.05f) );
							break;

							case 1:
							colors[i] = Vector4.Max( (ogColors[i]-new Color(0,maxStrength*brushStrength*Time.deltaTime*brushStrength, 0, 0)), new Vector4(0,0.1f,0,0.05f) );
							break;

							case 2:
							colors[i] = Vector4.Max( (ogColors[i]-new Color(0, 0, maxStrength*brushStrength*Time.deltaTime*brushStrength, 0)), new Vector4(0,0.1f,0,0.05f) );
							break;

							case 3:
							colors[i] = Vector4.Max( (ogColors[i]-new Color( 0, 0, 0, maxStrength*brushStrength*Time.deltaTime*brushStrength)), new Vector4(0,0.05f,0,0.05f) );
							break;
							
						}
					} 

					perMaterialData[target.materialProfileIndex].furData0.SetPixels( (int)Mathf.Clamp( t.x-(brushSize*0.5f), 0, textureSize-brushSize ), (int)Mathf.Clamp( t.y-(brushSize*0.5f), 0, textureSize-brushSize ), brushSize, brushSize, colors );
					perMaterialData[target.materialProfileIndex].furData0.Apply();
				}
				else {
					var t = (secondUVS==0?hit.textureCoord:hit.textureCoord2)*textureSize;
						Color[] colors = new Color[brushSize*brushSize];
						Color[] ogColors = perMaterialData[target.materialProfileIndex].furData1.GetPixels( (int)Mathf.Clamp( t.x-(brushSize*0.5f), 0, textureSize-brushSize ), (int)Mathf.Clamp( t.y-(brushSize*0.5f), 0, textureSize-brushSize ), brushSize, brushSize );
						int tempX = 0;
						int tempY = 0;

						for ( int i = 0; i < colors.Length; i++ ){
							tempX ++;
							if ( tempX >= brushSize ){
								tempY ++;
								tempX = 0;
							}
						
						float ratio = Vector2.Distance(new Vector2(tempX,tempY), new Vector2(brushSize*0.5f,brushSize*0.5f));
						float maxStrength = ratio>brushSize*0.5f?0:(1-(ratio/(brushSize*0.5f)));

						if ( tool == 4 ){
							colors[i] = Vector4.Lerp( ogColors[i], new Vector4(0.5f,0.5f,0.5f,ogColors[i].a), 0.5f*brushStrength*Time.deltaTime );
						}

						if ( tool == 5 ){
							colors[i] = Vector4.Min( (ogColors[i]+new Color( 0, 0, 0, maxStrength*brushStrength*Time.deltaTime*brushStrength)), Vector4.one );
						}


						} 

						perMaterialData[target.materialProfileIndex].furData1.SetPixels( (int)Mathf.Clamp( t.x-(brushSize*0.5f), 0, textureSize-brushSize ), (int)Mathf.Clamp( t.y-(brushSize*0.5f), 0, textureSize-brushSize ), brushSize, brushSize, colors );
						perMaterialData[target.materialProfileIndex].furData1.Apply();
					
				}
			
			}

			
		}


		if ( Input.GetMouseButtonUp(0)||Input.GetMouseButtonUp(1) ){
			isPainting = false;

			perMaterialData[target.materialProfileIndex].undoSpaces.Add( new Texture2D(textureSize,textureSize) );

			perMaterialData[target.materialProfileIndex].undoSpaces[perMaterialData[target.materialProfileIndex].undoSpaces.Count-1] = Instantiate(perMaterialData[target.materialProfileIndex].furData0);

			if ( perMaterialData[target.materialProfileIndex].undoSpaces.Count > 10 ){
				DestroyImmediate(perMaterialData[target.materialProfileIndex].undoSpaces[0]);
				perMaterialData[target.materialProfileIndex].undoSpaces.RemoveAt(0);
			}
		}


		if ( Input.GetKeyDown(KeyCode.R) ){
			if ( !isPainting )
			if ( perMaterialData[target.materialProfileIndex].undoSpaces.Count > 1 ){
				perMaterialData[target.materialProfileIndex].furData0.SetPixels( perMaterialData[target.materialProfileIndex].undoSpaces[perMaterialData[target.materialProfileIndex].undoSpaces.Count-2].GetPixels() );
				perMaterialData[target.materialProfileIndex].furData0.Apply();
				perMaterialData[target.materialProfileIndex].undoSpaces.RemoveAt(perMaterialData[target.materialProfileIndex].undoSpaces.Count-1);
			}
		}

			if ( Time.timeSinceLevelLoad > tStep ){
			RaycastHit hit2;

			if ( Physics.Raycast( Camera.main.ScreenPointToRay(Input.mousePosition), out hit2, 16 ) ){
				var t = (secondUVS==0?hit2.textureCoord:hit2.textureCoord2)*256;
				var bSize = brushSize/(textureSize/256);
				Color[] colors = new Color[bSize*bSize];
				
				int tempX = 0;
				int tempY = 0;

				for ( int i = 0; i < colors.Length; i++ ){
					tempX ++;
					if ( tempX >= bSize ){
						tempY ++;
						tempX = 0;
					}
					float ratio = Vector2.Distance(new Vector2(tempX,tempY), new Vector2(bSize*0.5f,bSize*0.5f));
					float maxStrength = ratio>bSize*0.5f?0:(1-(ratio/(bSize*0.5f)));

					colors[i] = (Color.cyan*Mathf.Clamp01(maxStrength*brushStrength))*0.2f;
				} 
				
				

				perMaterialData[target.materialProfileIndex].painterTex.SetPixels(new Color[256*256]);

				perMaterialData[target.materialProfileIndex].painterTex.SetPixels( (int)Mathf.Clamp( t.x-(bSize*0.5f), 0, 256-bSize ), (int)Mathf.Clamp( t.y-(bSize*0.5f), 0, 256-bSize ), bSize, bSize, colors );

				perMaterialData[target.materialProfileIndex].painterTex.Apply();
			}

			tStep = Time.timeSinceLevelLoad+0.1f;
		}
		
		}


		void MouseOrbit(){

			if ( !Input.GetKey(KeyCode.LeftControl)&&!Input.GetKey(KeyCode.LeftShift) )
				distance = Mathf.Clamp( distance - Input.GetAxis("Mouse ScrollWheel")*120*Time.deltaTime, 0.15f, 8 );
		
			

			if ( !Input.GetKey(KeyCode.LeftShift)&&Input.GetKey(rotationKey) ){
				orbitX += Input.GetAxis("Mouse X") * 150 * distance * 0.02f;
				orbitY -= Input.GetAxis("Mouse Y") * 150 * 0.02f;
			}
			else if ( Input.GetKey(KeyCode.LeftShift) && Input.GetKey(movePivotKey) ){
				origin -= transform.rotation*new Vector3(Input.GetAxis("Mouse X")*10*Time.deltaTime, Input.GetAxis("Mouse Y")*10*Time.deltaTime, 0 );
			}

			orbitY = ClampAngle( orbitY, -90, 90);
		
			Quaternion rotation = Quaternion.Euler(orbitY, orbitX, 0);
			Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
			Vector3 position = rotation * negDistance + origin;
		
			transform.rotation = rotation;
			transform.position = position;
		}

		public static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360F)
				angle += 360F;
			if (angle > 360F)
				angle -= 360F;
			return Mathf.Clamp(angle, min, max);
		}


		private void OnDestroy() {
			
			foreach( XFurPainter_PainterData m in perMaterialData ){
				if ( m.originalMat ){
					m.originalMat.DisableKeyword( "XFUR_PAINTER" );
					if ( m.originalFDat0 ){
						m.originalMat.SetTexture("_FurData0", m.originalFDat0 );
					}
					if ( m.originalFDat1 ){
						m.originalMat.SetTexture("_FurData1", m.originalFDat1 );
					}
				}
			}
			
			if ( target ){
				foreach( Material m in target.GetComponent<Renderer>().sharedMaterials ){
					m.DisableKeyword( "XFUR_PAINTER" );
				}

				for ( int i = 0; i < target.materialProfiles.Length; i++ ){
					if ( perMaterialData[i].originalFDat0 ){
						target.materialProfiles[i].originalMat.SetTexture("_FurData0", perMaterialData[i].originalFDat0 );
					}
					if ( perMaterialData[i].originalFDat1 ){
						target.materialProfiles[i].originalMat.SetTexture("_FurData1", perMaterialData[i].originalFDat1 );
					}
				}

				
			}
		}


	}

}