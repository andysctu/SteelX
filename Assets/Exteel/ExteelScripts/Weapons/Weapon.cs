﻿using UnityEngine;
using UnityEngine.Networking;

abstract class Weapon : NetworkBehaviour {

	public int Damage;
	public float Range;
	protected Transform camTransform;
	protected GameObject root;

	abstract public void Fire();

	public void SetCam(Transform cam) {
		camTransform = cam;
	}

	public void SetRoot(GameObject r) {
		root = r;
	}
}
