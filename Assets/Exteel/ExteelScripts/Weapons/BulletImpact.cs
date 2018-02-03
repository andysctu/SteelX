using UnityEngine;
using System.Collections;

public class BulletImpact : MonoBehaviour {

	[SerializeField] private AudioClip ImpactSound;

	// Use this for initialization
	void Start () {
		//print ("bulletimpact is called.");
		ParticleSystem ps = GetComponent<ParticleSystem>();
		//ps.Emit(1);
		Destroy(gameObject, 0.55f);
	}
}
