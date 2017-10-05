using UnityEngine;
using System.Collections;

public class MeleeCombat : MonoBehaviour {
	[SerializeField] Animator animator;
	private const int LEFT_HAND = 0;

	public void Slash(int i) {
		Debug.Log(i);
	}

	public void StopSlash(int handPosition) {
		animator.SetBool("Slash" + (handPosition == LEFT_HAND ? "L" : "R"), false);
	}
}
