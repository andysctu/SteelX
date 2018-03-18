using UnityEngine;
using System.Collections;

public class BulletImpact : MonoBehaviour {

	public AudioClip ImpactSound;

	// Use this for initialization
	void Start () {
		//print ("bulletimpact is called.");
		//ParticleSystem ps = GetComponent<ParticleSystem>();
		//ps.Emit(1);
		if (ImpactSound != null)
			AudioSource.PlayClipAtPoint (ImpactSound, transform.position);
		Destroy(gameObject, 0.50f);
	}
}
