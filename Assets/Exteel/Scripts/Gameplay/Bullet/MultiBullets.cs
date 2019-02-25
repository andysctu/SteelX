using UnityEngine;

namespace Weapons.Bullets
{
    public class MultiBullets : Bullet
    {
        //This script use emit(1) to emit bullets such that the rotation is the same with parent and is in world space 

        private ParticleSystem.Particle[] particles;

        //Bullet variables
        private float interval = 1, timeOfLastSpawn;
        private int maxBulletNum, numParticlesAlive, totalSpawnedBulletNum;

        public void SetParticleSystem(int maxBulletNum, float interval){
            this.interval = interval;
            this.maxBulletNum = maxBulletNum;
            particles = new ParticleSystem.Particle[maxBulletNum];
        }

        protected override void LateUpdate(){
            if (Isfollow){
                if (Target == null){
                    StopBulletEffect();
                    return;
                }
            }

            //Emit particles
            if (totalSpawnedBulletNum >= maxBulletNum){
                if (numParticlesAlive == 0) StopBulletEffect();
            } else{
                if (Time.time - timeOfLastSpawn > interval){
                    transform.LookAt(cam.transform.forward * 9999);
                    timeOfLastSpawn = Time.time;
                    totalSpawnedBulletNum++;
                    bullet_ps.Emit(1);

                    //Show hit when the bullet is spawned
                    //if (isfollow) ShowHitMsg(Target);
                }
            }

            numParticlesAlive = bullet_ps.GetParticles(particles);

            //Set velocity for each particle , and play impact if hit
            if (Isfollow){
                for (int i = 0; i < numParticlesAlive; i++){
                    if (Vector3.Distance(particles[i].position, Target.GetPosition()) <= bulletSpeed * Time.deltaTime){
                        PlayImpact(particles[i].position);
                        particles[i].remainingLifetime = 0;
                    } else{
                        particles[i].velocity = (Target.GetPosition() - particles[i].position).normalized * bulletSpeed;
                    }
                }
            }

            //Apply changes
            bullet_ps.SetParticles(particles, numParticlesAlive);
        }

        public override void Play(){
            totalSpawnedBulletNum = 0;
            enabled = true;
        }

        public override void StopBulletEffect(){
            enabled = false;
            Destroy(gameObject, 2);
        }
    }
}