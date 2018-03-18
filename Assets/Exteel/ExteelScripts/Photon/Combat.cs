using UnityEngine;

public class Combat : Photon.MonoBehaviour {
	public const int MAX_HP = 2000;
	public const float MAX_FUEL = 2000.0f;

	protected int currentHP;

	protected GameManager gm;

	[PunRPC]
	public virtual void OnHit(int d, int shooter_viewID, float slowdownDuration) {}

	protected void findGameManager() {
		if (gm == null) {
			gm = GameObject.Find("GameManager").GetComponent<GameManager>();
		}
	}

	public int CurrentHP() {
		return currentHP;
	}

	public int GetMaxHp(){
		return MAX_HP;
	}
}