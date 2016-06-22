class RangedWeapon : Weapon {
//	GameManager gm;
//	private bool fireL = false;
//	private bool shootingL = false;
//
//	private bool fireR = false;
//	private bool shootingR = false;
//
//	private Transform shoulderL;
//	private Transform shoulderR;

//	private Transform shoulder;
//
//	private MechCombat mc;
//
//	private bool left;

//	void Start() {
//		gm = GameObject.Find("GameManager").GetComponent<GameManager>();
//		shoulderL = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.L");
//		shoulderR = transform.FindChild("CurrentMech/metarig/hips/spine/chest/shoulder.R");
//
//		shoulder = transform.parent.parent.parent.parent;
//		left = shoulder.name.EndsWith("L");
//
//		Debug.Log("shoulder: " + shoulder.name);
//		Debug.Log("left: " + left);
//
//		Range = Mathf.Infinity;
//		Damage = 25;
//
//		mc = root.GetComponent<MechCombat>();
//	}



//	override public void Fire() {
//		Debug.Log("Fire");
//	}

//	public void setLeft(bool l) {
//		left = l;
//	}

	// Update is called once per frame
//	void Update () {
//		if (gm.GameOver()) return;
//		Debug.Log("left is: " + left);
//		if (left) {
//			Debug.Log("L");
//			if (Input.GetKey(KeyCode.Mouse0)){
//				Debug.Log("FireL");
//				mc.CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, Range);
//				fireL = true;
//			} else {
//				fireL = false;
//			}
//		} else {
//			if (Input.GetKey(KeyCode.Mouse1)){
//				Debug.Log("FireR");
//				mc.CmdFireRaycast(camTransform.TransformPoint(0,0,0.5f), camTransform.forward, Range);
//				fireR = true;
//			} else {
//				fireR = false;
//			}
//		}
//	}
//
//	void LateUpdate() {
//		if (fireL && left) {
//			playShootAnimationL();
//			shootingL = true;
//		} else if (shootingL) {
//			stopShootAnimationL();
//			shootingL = false;
//		}
//
//		if (fireR && !left) {
//			playShootAnimationR();
//			shootingR = true;
//		} else if (shootingR) {
//			stopShootAnimationR();
//			shootingR = false;
//		}
//	}
//
//	void playShootAnimationL() {
//		shoulder.Rotate(0,90,0);
//	}
//
//	void stopShootAnimationL() {
//		shoulder.Rotate(0,-90,0);
//	}
//
//	void playShootAnimationR() {
//		shoulder.Rotate(0,-90,0);
//	}
//
//	void stopShootAnimationR() {
//		shoulder.Rotate(0,90,0);
//	}

//	[Command]
//	void CmdFireRaycast(Vector3 start, Vector3 direction){
//		if (Physics.Raycast (start, direction, out hit, Range, 1 << 8)){
//			Debug.Log ("Hit tag: " + hit.transform.tag);
//			Debug.Log("Hit name: " + hit.transform.name);
//			//			Debug.Log("Parent name: " + hit.transform.parent.name);
//			//			Debug.Log("Parent parent name: " + hit.transform.parent.parent.name);
//
//			if (hit.transform.tag == "Player"){
//				hit.transform.GetComponent<MechCombat>().OnHit(gameObject.GetComponent<NetworkIdentity>().netId.Value, Damage);
//			} else if (hit.transform.tag == "Drone"){
//
//			}
//		}
//	}
}
