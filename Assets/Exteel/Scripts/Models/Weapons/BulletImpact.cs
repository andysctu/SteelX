﻿using UnityEngine;

public class BulletImpact : MonoBehaviour {

    public AudioClip ImpactSound;

    private void Start() {
        PlayHitSound(transform.position);
        Destroy(gameObject, 1.5f);
    }
    void PlayHitSound(Vector3 intersection) {
        if (ImpactSound != null)
            AudioSource.PlayClipAtPoint(ImpactSound, intersection);
    }
}