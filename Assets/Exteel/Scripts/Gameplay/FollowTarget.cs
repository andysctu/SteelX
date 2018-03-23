using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

	//this script handles particles, not gameObject
public class FollowTarget : MonoBehaviour
{
	[SerializeField]ParticleSystem p;
	[SerializeField]int particlesNum;//max num
	[SerializeField]float speed;
	[SerializeField]LayerMask layerMask = 8, Terrain = 10;
	ParticleSystem.Particle [] particles;

	public Transform Target;
	public ParticleSystem bulletImpact;
	public HUD HUD;
	public Camera cam;
	public string ShooterName;
	public bool isTargetShield;
	private bool ImpactIsPlayed = false;
	private bool hasSlowdown = false;
	private int numParticlesAlive;

	void Start () {
		p.Play ();
		particles = new ParticleSystem.Particle [particlesNum];
		//Destroy (gameObject, 2f);
	}

	void Update () {

		if (Target != null) {
			//current alive particles num
			numParticlesAlive = p.GetParticles(particles);

			for (int i = 0; i < numParticlesAlive; i++) {
				if (Vector3.Distance (particles [i].position, Target.position) <= speed * Time.deltaTime) {
					PlayImpact (particles[i].position);
					particles [i].remainingLifetime = 0;
				} else {
					particles [i].velocity = (Target.position - particles [i].position).normalized * speed;
				}
			}

			p.SetParticles (particles, numParticlesAlive);
		}else{
			//do nothing
		}
	}

	void PlayImpact(Vector3 intersection){
		if (ImpactIsPlayed)
			return;
		
		print ("call play impact");

		bulletImpact.transform.position = intersection;

		bulletImpact.Play();
		ImpactIsPlayed = true;

	}
}
