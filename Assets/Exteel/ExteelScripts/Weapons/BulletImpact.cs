using UnityEngine;
using System.Collections;

public class BulletImpact : MonoBehaviour {

	// Use this for initialization
	void Start () {
		ParticleSystem ps = GetComponent<ParticleSystem>();
		//ps.Emit(1);
		Destroy(gameObject, 0.55f);
	}
}
