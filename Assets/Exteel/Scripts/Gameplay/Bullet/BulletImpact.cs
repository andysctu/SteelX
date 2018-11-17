using UnityEngine;

namespace Weapons.Bullets
{
    public class BulletImpact : MonoBehaviour
    {
        [SerializeField] private AudioClip ImpactSound;
        private ParticleSystem bulletImpact;
        private AudioSource AudioSource;

        private void Awake(){
            RegisterToBulletCollector();
            InitComponents();
        }

        private void RegisterToBulletCollector(){
            GameObject BulletCollector = GameObject.FindGameObjectWithTag("Collector");
            transform.SetParent(BulletCollector.transform);
        }

        private void InitComponents(){
            bulletImpact = GetComponent<ParticleSystem>();
            InitAudioSource();
        }

        private void InitAudioSource(){
            AudioSource = gameObject.AddComponent<AudioSource>();

            AudioSource.spatialBlend = 1;
            AudioSource.dopplerLevel = 0;
            AudioSource.volume = 1;
            AudioSource.playOnAwake = false;
            AudioSource.minDistance = 50;
            AudioSource.maxDistance = 350;
        }

        public void Play(Vector3 intersection){
            transform.position = intersection;
            bulletImpact.Play();

            if (ImpactSound != null){
                AudioSource.PlayOneShot(ImpactSound);
            }
        }
    }
}