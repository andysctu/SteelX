using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckIsRendered : MonoBehaviour {

	SkinnedMeshRenderer theMeshRenderer;
	public Crosshair crosshair;//set in buildmech , same player for all mechs

	private bool isVisible = false;
	private float lastRequestTime;
	private float requestDeltaTime = 0.5f;
	// Use this for initialization
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
	
	// Update is called once per frame
	void Update () {
		if(crosshair == null){
			if (Time.time - lastRequestTime >= requestDeltaTime) {
				lastRequestTime = Time.time;
				GameObject theplayer = GameObject.Find (PhotonNetwork.playerName);
				if (theplayer != null)
					crosshair = theplayer.GetComponentInChildren<Camera> ().GetComponent<Crosshair> ();
				else {
					crosshair = null;
					return;
				}
			}else{
				return;
			}
		}

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
