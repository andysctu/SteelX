using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckIsRendered : MonoBehaviour {

	SkinnedMeshRenderer theMeshRenderer;
	private Crosshair crosshair;//same player's crosshair for all mechs

	private bool isVisible = false;
	private float lastRequestTime;
	private float requestDeltaTime = 0.5f;

	void Start () {
		if(GetComponent<PhotonView>().isMine && gameObject.tag!="Drone"){//not get msg from myself
			enabled = false;
		}
		//find core
		if (gameObject.tag != "Drone") {
			SkinnedMeshRenderer[] meshRenderers = transform.Find ("CurrentMech").GetComponentsInChildren<SkinnedMeshRenderer> ();
			theMeshRenderer = meshRenderers [0];
		}else{
			SkinnedMeshRenderer meshRenderer = GetComponentInChildren<SkinnedMeshRenderer> ();
			theMeshRenderer = meshRenderer;
		}
	}
	
    public void SetCrosshair(Crosshair crosshair) {
        this.crosshair = crosshair;
    }

	// Update is called once per frame
	void Update () {
		if(crosshair == null){//TODO : improve this
			if (Time.time - lastRequestTime >= requestDeltaTime) {
				lastRequestTime = Time.time;
                Camera[] cameras = (Camera[])GameObject.FindObjectsOfType<Camera>();
                foreach(Camera cam in cameras) {
                    if(cam.transform.root.name == "PlayerCam") {
                        crosshair = cam.GetComponent<Crosshair>();
                    }
                }
			}
		}else{
			if(theMeshRenderer.isVisible){
				if(!isVisible){
					isVisible = true;
					crosshair.Targets.Add (transform.root.gameObject);
				}
			}else{
				if(isVisible){
					isVisible = false;
					crosshair.Targets.Remove (transform.root.gameObject);
				}
			}
		}
	}
}
