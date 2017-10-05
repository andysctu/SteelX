using UnityEngine;

public class Combat : Photon.MonoBehaviour {
	public const int MAX_HP = 100;
	public const float MAX_FUEL = 100.0f;

	protected int currentHP;

	protected GameManager gm;

	[PunRPC]
	public virtual void OnHit(int d, string shooter) {}

	protected void findGameManager() {
		if (gm == null) {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
	}
}