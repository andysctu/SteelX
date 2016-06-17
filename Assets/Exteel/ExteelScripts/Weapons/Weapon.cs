using UnityEngine;
using UnityEngine.Networking;

abstract class Weapon : NetworkBehaviour {

	public int Damage;
	public float Range;
	protected Transform camTransform;

	abstract public void Fire();

	public void SetCam(Transform cam) {
		camTransform = cam;
	}
}
