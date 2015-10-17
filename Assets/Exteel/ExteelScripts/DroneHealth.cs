using UnityEngine;
using System.Collections;

public class DroneHealth : MonoBehaviour {

	private int health = 100;

	public void OnHit(int dmg){
		health -= dmg;
		CheckHealth ();
	}

	void CheckHealth(){
		if (health <= 0) {
			Destroy (gameObject);
		}
	}
}
