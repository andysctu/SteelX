using UnityEngine;

public class Combat : Photon.MonoBehaviour {
	public int MaxHP;
	public int CurrentHP;

	[PunRPC]
	public virtual void OnHit(int d, string shooter) {
	}
}