using UnityEngine;

public class BulletImpact : MonoBehaviour {

	public AudioClip ImpactSound;

    private void Start()
    {
        Destroy(gameObject, 2);
    }
    public void PlayHitSound(Vector3 intersection)
    {
        if (ImpactSound != null)
            AudioSource.PlayClipAtPoint(ImpactSound, intersection);
    }
}
