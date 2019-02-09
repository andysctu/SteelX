using UnityEngine;
using Utility;

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
            AudioSource = AudioSourceBuilder.Build(gameObject);
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