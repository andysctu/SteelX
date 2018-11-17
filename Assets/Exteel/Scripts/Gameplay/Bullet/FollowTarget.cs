using UnityEngine;

//this script handles particles, not gameObject
public class FollowTarget : MonoBehaviour {
    [SerializeField] private ParticleSystem p;
    [SerializeField] private int particlesNum;//max num
    [SerializeField] private float speed;
    [SerializeField] private LayerMask layerMask = 8, Terrain = 10;
    private ParticleSystem.Particle[] particles;

    public Transform Target;
    public ParticleSystem bulletImpact;
    public HUD HUD;
    public Camera cam;
    public string ShooterName;
    public bool isTargetShield;
    private bool ImpactIsPlayed = false;
    private bool hasSlowdown = false;
    private int numParticlesAlive;

    private void Start() {
        p.Play();
        particles = new ParticleSystem.Particle[particlesNum];
        //Destroy (gameObject, 2f);
    }

    private void Update() {
        if (Target != null) {
            //current alive particles num
            numParticlesAlive = p.GetParticles(particles);

            for (int i = 0; i < numParticlesAlive; i++) {
                if (Vector3.Distance(particles[i].position, Target.position) <= speed * Time.deltaTime) {
                    PlayImpact(particles[i].position);
                    particles[i].remainingLifetime = 0;
                } else {
                    particles[i].velocity = (Target.position - particles[i].position).normalized * speed;
                }
            }

            p.SetParticles(particles, numParticlesAlive);
        } else {
            //do nothing
        }
    }

    private void PlayImpact(Vector3 intersection) {
        if (ImpactIsPlayed)
            return;

        print("call play impact");

        bulletImpact.transform.position = intersection;

        bulletImpact.Play();
        ImpactIsPlayed = true;
    }
}