using UnityEngine;

public class Combat : Photon.MonoBehaviour {
    private int max_hp = 2000;
	public int MAX_HP { get { return max_hp ;} protected set { max_hp = value ;} }
    public const float MAX_FUEL = 2000;
    public int CurrentHP { get; protected set; }

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

	public int GetMaxHp(){
		return MAX_HP;
	}

    public bool IsHpFull() {
        return (CurrentHP >= MAX_HP);
    }
}