﻿using UnityEngine;
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
	
	private Slider healthBar;

	// Use this for initialization
	public override void OnStartLocalPlayer () {
		Debug.Log ("Running");
		health = MaxHealth;
		Debug.Log ("Running.");
		healthText = GameObject.Find ("Health Text").GetComponent<Text> ();
		//healthBar = GameObject.Find ("HealthBar");
		Debug.Log ("Running..");
		SetHealthText ();
	}
	
	// Update is called once per frame
	void Update () {
		CheckCondition ();

//		float currentPercent = healthBar.value;
//		float targetPercent = health/(float)MaxHealth;
//		float err = 0.1f;
//		if (Mathf.Abs(currentPercent - targetPercent) > err) {
//			currentPercent = currentPercent + (currentPercent > targetPercent ? -0.05f : 0.05f);
//		}
//
//		healthBar.value = currentPercent;
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
