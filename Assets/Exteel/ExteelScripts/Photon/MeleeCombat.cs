using UnityEngine;
using System.Collections;

public class MeleeCombat : MonoBehaviour {
	[SerializeField] Animator animator;

	public void Slash(int i) {
		Debug.Log(i);
	}

	private void Stop() {
		animator.SetBool("Slash", false);
	}
}
