public abstract class Weapon : Part {

	public int Damage;
	public float Range;
	public float Rate;
	public float radius=0;
	public string Animation;
	public int bulletNum=1; //for animation

	public bool isSlowDown = false;
	public bool isTwoHanded=false;
}
