using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSHScollider : MonoBehaviour {

	public GameObject boxcollider;

	void Start(){

	}

	void LateUpdate () {

	}

	void OnDestroy(){
		if(boxcollider!=null){
			Destroy (boxcollider);
		}
	}
}
