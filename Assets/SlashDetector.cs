using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashDetector : MonoBehaviour {

	private Transform Target;
	void Start(){
		Target = null;
	}
	void OnTriggerEnter(Collider target){
		if (target.gameObject != gameObject && (target.tag == "Drone" || target.tag == "Player" )) {
			Target = target.transform;
		}
	}
	 
	void OnTriggerExit(Collider target){
		if(target.gameObject != gameObject&&(target.tag == "Drone" || target.tag == "Player" ) ){
			Target = null;
		}	
	}
	public Transform getCurrentTarget(){
		return Target;
	} 
}
