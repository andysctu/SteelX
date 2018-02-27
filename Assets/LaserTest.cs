using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTest : MonoBehaviour {

	[SerializeField]LineRenderer lineRenderer;
	// Use this for initialization
	void Start () {
		lineRenderer.SetPosition (0, Vector3.zero);
		lineRenderer.SetPosition (1, new Vector3 (10, 10, 10));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
