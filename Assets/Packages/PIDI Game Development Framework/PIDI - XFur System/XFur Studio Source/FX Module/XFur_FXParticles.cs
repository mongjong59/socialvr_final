using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XFurStudio;


namespace XFurStudio{

	public class XFur_FXParticles:MonoBehaviour{
		
		public FXToApply effectToApply;
		[Range(0,1)]public float effectIntensity = 1.0f;
		public float effectRadius = 0.1f;
		private Mesh m ;

		private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

		void OnParticleCollision(GameObject other) {

			Mesh m = new Mesh();

			XFur_System targetFur;
			if ( targetFur = other.GetComponentInChildren<XFur_System>() ){
				GetComponent<ParticleSystem>().GetCollisionEvents( other, collisionEvents );

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
					foreach ( ParticleCollisionEvent e in collisionEvents )
						if ( Vector3.Distance( targetFur.transform.TransformPoint(verts[fV[i]]), e.intersection ) < effectRadius ){
							vertexIndex.Add(fV[i]);
						}

				}

				targetFur.fxModule.ApplyEffect( (int)effectToApply, vertexIndex.ToArray(), effectIntensity );
							

			}

		}
	}
}

