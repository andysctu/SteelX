public abstract class Weapon {

	public int Damage;
	public float Range;
	public float Rate;
	public float radius=0;
	public string Animation;
	public int bulletNum=1; //for animation
	public float Weight;

	public bool isSlowDown = false;
	public bool isTwoHanded=false;
//	protected Transform camTransform;
//	protected GameObject root;

//	abstract public void Fire();

//	public void SetCam(Transform cam) {
//		camTransform = cam;
//	}
//
//	public void SetRoot(GameObject r) {
//		root = r;
//	}
}
