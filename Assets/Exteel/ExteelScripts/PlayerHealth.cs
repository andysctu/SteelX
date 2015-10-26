using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour {

	public int MaxHealth = 100;
	[SyncVar (hook = "OnHealthChanged")] public int health;
	private Text healthText;
	private bool shouldDie = false;
	public bool isDead = false;

	public delegate void DieDelegate();
	public event DieDelegate EventDie;

	public delegate void RespawnDelegate();
	public event RespawnDelegate EventRespawn;

	// Use this for initialization
	public override void OnStartLocalPlayer () {
		Debug.Log ("Running");
		health = MaxHealth;
		Debug.Log ("Running.");
		healthText = GameObject.Find ("Health Text").GetComponent<Text> ();
		Debug.Log ("Running..");
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
		health = MaxHealth;
	}

	void SetHealthText(){
		if (isLocalPlayer) {
			if (healthText != null){
				healthText.text = "Health " + health.ToString();
			}
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
