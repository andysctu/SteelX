using UnityEngine;
using System.Collections;

public class HitInfo {
	public RaycastHit raycastHit;
	public float damage;
}

public class Drone : MonoBehaviour {
	
	public float Health = 100f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (Health <= 0) {
			Destroy (gameObject);
		}
	}

//	void OnHit(HitInfo hitInfo){
//		Health -= hitInfo.damage;
//		AudioSource.PlayClipAtPoint (gameObject.GetComponent<AudioSource> ().clip, transform.position);
//	}
}
