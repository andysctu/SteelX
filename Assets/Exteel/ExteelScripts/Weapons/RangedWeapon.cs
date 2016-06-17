using UnityEngine;
using UnityEngine.Networking;

class RangedWeapon : Weapon {
	GameManager gm;
	private bool fireL = false;
	private bool shootingL = false;

	private bool fireR = false;
	private bool shootingR = false;

	private Transform shoulderL;
	private Transform shoulderR;
	private RaycastHit hit;

	void Start() {
		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		shoulderL = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.L");
		shoulderR = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.R");

		Range = Mathf.Infinity;
		Damage = 25;
	}

	override public void Fire() {
		Debug.Log("Fire");
	}

	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer || gm.GameOver()) return;
		if (Input.GetKey(KeyCode.Mouse0)){
			CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward);
			fireL = true;
		} else {
			fireL = false;
		}

		if (Input.GetKey(KeyCode.Mouse1)){
			CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward);
			fireR = true;
		} else {
			fireR = false;
		}
	}

	void LateUpdate() {
		if (fireL) {
			playShootAnimationL();
			shootingL = true;
		} else if (shootingL) {
			stopShootAnimationL();
			shootingL = false;
		}

		if (fireR) {
			playShootAnimationR();
			shootingR = true;
		} else if (shootingR) {
			stopShootAnimationR();
			shootingR = false;
		}
	}

	void playShootAnimationL() {
		shoulderL.Rotate(0,90,0);
	}

	void stopShootAnimationL() {
		shoulderL.Rotate(0,-90,0);
	}

	void playShootAnimationR() {
		shoulderR.Rotate(0,-90,0);
	}

	void stopShootAnimationR() {
		shoulderR.Rotate(0,90,0);
	}

	[Command]
	void CmdFireRaycast(Vector3 start, Vector3 direction){
		if (Physics.Raycast (start, direction, out hit, Range, 1 << 8)){
			Debug.Log ("Hit tag: " + hit.transform.tag);
			Debug.Log("Hit name: " + hit.transform.name);
			//			Debug.Log("Parent name: " + hit.transform.parent.name);
			//			Debug.Log("Parent parent name: " + hit.transform.parent.parent.name);

			if (hit.transform.tag == "Player"){
				hit.transform.GetComponent<MechCombat>().OnHit(gameObject.GetComponent<NetworkIdentity>().netId.Value, Damage);
			} else if (hit.transform.tag == "Drone"){

			}
		}
	}
}
