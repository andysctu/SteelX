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
	
	private Slider healthBar;

	// Use this for initialization
	public override void OnStartLocalPlayer () {
		health = MaxHealth;
		healthText = GameObject.Find ("Health Text").GetComponent<Text> ();
		Slider[] slider = GameObject.Find ("Canvas").GetComponentsInChildren<Slider>();
		if (slider.Length > 0) {
			Debug.Log("Healthbar is non-empty");
			healthBar = slider[0];
		} else {
			Debug.Log("Healthbar is empty");
		}
		SetHealthText ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!isLocalPlayer) return;
		if (healthBar == null){
			Slider[] slider = GameObject.Find ("Canvas").GetComponentsInChildren<Slider>();
			if (slider.Length > 0) {
				Debug.Log("Healthbar is non-empty2");
				healthBar = slider[0];
			} else {
				Debug.Log("Healthbar is empty2");
			}
		}
		CheckCondition ();

		float currentPercent = healthBar.value;
		float targetPercent = health/(float)MaxHealth;
		float err = 0.1f;
		if (Mathf.Abs(currentPercent - targetPercent) > err) {
			currentPercent = currentPercent + (currentPercent > targetPercent ? -0.01f : 0.01f);
		}

		healthBar.value = currentPercent;
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
