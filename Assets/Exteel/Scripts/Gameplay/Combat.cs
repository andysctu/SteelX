using UnityEngine;

public class Combat : Photon.MonoBehaviour {
	public const int MAX_HP = 2000;
	public const float MAX_FUEL = 2000.0f;

	protected int currentHP;

	protected GameManager gm;

	[PunRPC]
	public virtual void OnHit(int d, int shooter_viewID, string weapon, bool isSlowDown) {}

	protected void findGameManager() {
		if (gm == null) {
            GameObject g = GameObject.Find("GameManager");
            if(g != null)
                gm = g.GetComponent<GameManager>();
            else {
                Debug.Log("Can't find GameManager. Ignore this if there isn't one");
            }
		}
	}

	public int CurrentHP() {
		return currentHP;
	}

	public int GetMaxHp(){
		return MAX_HP;
	}

    public bool IsHpFull() {
        return (currentHP >= MAX_HP);
    }
}