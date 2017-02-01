using UnityEngine;

public class Combat : Photon.MonoBehaviour {
	public int MaxHP;
	public int CurrentHP;

	protected GameManager gm;

	[PunRPC]
	public virtual void OnHit(int d, string shooter) {
	}

	protected void findGameManager() {
		if (gm == null) {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
	}
}