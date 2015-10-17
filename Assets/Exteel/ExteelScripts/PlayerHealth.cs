using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour {

	[SyncVar (hook = "OnHealthChanged")] private int health = 100;
	private Text healthText;
	private bool shouldDie = false;
	public bool isDead = false;

	public delegate void DieDelegate();
	public event DieDelegate EventDie;

	public delegate void RespawnDelegate();
	public event RespawnDelegate EventRespawn;

	// Use this for initialization
	public override void OnStartLocalPlayer () {
		healthText = GameObject.Find ("Health Text").GetComponent<Text> ();
		SetHealthText ();
	}
	
	// Update is called once per frame
	void Update () {
		CheckCondition ();
	}

	void CheckCondition(){
		if (health <= 0 && !shouldDie && !isDead) {
			shouldDie = true;
		}

		if (health <= 0 && shouldDie) {
			if (EventDie != null) {
				EventDie();
			}

			shouldDie = false;
		}

		if (health > 0 && isDead) {
			if (EventRespawn != null){
				EventRespawn();
			}	
			isDead = false;
		}
	}

	public void ResetHealth(){
		health = 100;
	}

	void SetHealthText(){
		if (isLocalPlayer) {
			healthText.text = "Health " + health.ToString();
		}
	}

	public void OnHit(int damage){
		health -= damage;
	}

	void OnHealthChanged(int hlth){
		health = hlth;
		SetHealthText ();
	}
}
