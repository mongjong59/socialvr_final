using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XFurStudio;

[ExecuteInEditMode]
public class XFur_FXVolume : MonoBehaviour {

	public enum FXToApply{ Blood, Snow, Water };

	[HideInInspector]public BoxCollider fxCollider;

	public FXToApply effectToApply;
	[Range(0,1)]public float effectIntensity = 1.0f;
	public Vector3 effectSize = new Vector3(1f,1f,1f);
	private Mesh m ;

	private void OnEnable() {
		if (!fxCollider){
			fxCollider = gameObject.AddComponent<BoxCollider>();
			fxCollider.isTrigger = true;
			fxCollider.hideFlags = HideFlags.HideInInspector;
		}
		else{
			fxCollider.hideFlags = HideFlags.HideInInspector;
		}
		
	}

	// Use this for initialization
	void Start () {
		m = new Mesh();
	}

	void OnDrawGizmos(){
		Gizmos.color = new Color(1,0,1,0.45f);

		Gizmos.DrawCube( transform.position, effectSize );
	}
	
	void Update(){
		fxCollider.size = effectSize;
	}

	private void OnTriggerStay(Collider other) {
		XFur_System targetFur;
		if ( targetFur = other.transform.root.GetComponentInChildren<XFur_System>() ){
			if (targetFur){
				var fV = targetFur.database.meshData[targetFur.database.XFur_ContainsMeshData(targetFur.OriginalMesh)].furVertices;
				if ( targetFur.GetComponent<SkinnedMeshRenderer>() ){
					targetFur.GetComponent<SkinnedMeshRenderer>().BakeMesh(m);
				}
				else{
					m = targetFur.Mesh;
				}

				var verts = m.vertices;
				List<int> vertexIndex = new List<int>();

				for ( int i = 0; i < fV.Length; i++ ){
					if ( ContainsPoint( targetFur.transform.TransformPoint(verts[fV[i]]) ) ){
						vertexIndex.Add(fV[i]);
					}
				}

				targetFur.fxModule.ApplyEffect( (int)effectToApply, vertexIndex.ToArray(), effectIntensity );
							
			}
		}
	}

	

	public bool ContainsPoint( Vector3 point ){
		var tPoint = transform.InverseTransformPoint(point);

		if ( tPoint.x < effectSize.x*0.5f && tPoint.x > effectSize.x*-0.5f  ){
			if ( tPoint.y < effectSize.y*0.5f && tPoint.y > effectSize.y*-0.5f  ){
				if ( tPoint.z < effectSize.z*0.5f && tPoint.z > effectSize.z*-0.5f  ){
					return true;
				}
			}
		}

		return false;
	}


}
