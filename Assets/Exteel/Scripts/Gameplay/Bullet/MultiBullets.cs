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
            if (isfollow){
                if (target == null){
                    Stop();
                    return;
                }
            }

            //Emit particles
            if (totalSpawnedBulletNum >= maxBulletNum){
                if (numParticlesAlive == 0) Stop();
            } else{
                if (Time.time - timeOfLastSpawn > interval){
                    transform.LookAt(cam.transform.forward * 9999);
                    timeOfLastSpawn = Time.time;
                    totalSpawnedBulletNum++;
                    bullet_ps.Emit(1);

                    //Show hit when the bullet is spawned
                    if (isfollow) ShowHitMsg(target);
                }
            }

            numParticlesAlive = bullet_ps.GetParticles(particles);

            //Set velocity for each particle , and play impact if hit
            if (isfollow){
                for (int i = 0; i < numParticlesAlive; i++){
                    if (Vector3.Distance(particles[i].position, target.position) <= bulletSpeed * Time.deltaTime){
                        PlayImpact(particles[i].position);
                        particles[i].remainingLifetime = 0;
                    } else{
                        particles[i].velocity = ((isTargetShield) ? (target.position - particles[i].position) : (target.position + MECH_MID_POINT - particles[i].position)).normalized * bulletSpeed;
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

        public override void Stop(){
            enabled = false;
            Destroy(gameObject, 2);
        }
    }
}