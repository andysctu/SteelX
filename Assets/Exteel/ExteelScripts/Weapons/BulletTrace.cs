using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrace : MonoBehaviour {

	// Use this for initialization
	void Start () {
		ParticleSystem ps = GetComponent<ParticleSystem>();
//		ps.Emit(10);
		ps.Play();
//		Destroy(gameObject, 0.15f);
	}
}
