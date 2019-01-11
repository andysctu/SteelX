using UnityEngine;

public class SingleBullet : Bullet {
    private Rigidbody Rigidbody;

    private const float BULLETSPEED = 300;
    private bool calledStop = false;//make sure not trigger play impact again

    protected override void Awake() {
        base.Awake();

        InitComponents();
    }

    private void InitComponents() {
        AttachRigidbody();
    }

    private void AttachRigidbody() {
        Rigidbody = gameObject.AddComponent<Rigidbody>();  
        Rigidbody.useGravity = false;
    }

    public override void Play() {
        if (isfollow) {
            if(target == null)return;
            transform.LookAt(target);
            bullet_ps.Play();
        } else {
            transform.LookAt(startDirection * 9999);
            Rigidbody.velocity = startDirection * BULLETSPEED;
            bullet_ps.Play();
        }
    }

    public override void Stop() {
        calledStop = true;

        bullet_ps.Stop(true);
        Destroy(gameObject);
        enabled = false;
    }

    protected override void LateUpdate() {
        if (isfollow) {
            if (target == null) { Stop(); return; }

            if (Vector3.Distance(transform.position, target.position) <= BULLETSPEED * Time.deltaTime) {
                PlayImpact(transform.position);                
            } else {
                Rigidbody.velocity = ((isTargetShield) ? (target.position - transform.position) : (target.position + MECH_MID_POINT - transform.position)).normalized * BULLETSPEED;
            }
        }
    }

    protected override void OnParticleCollision(GameObject other) {
        int numCollisionEvents = bullet_ps.GetCollisionEvents(other, collisionEvents);
        int i = 0;
        while (i < numCollisionEvents) {
            Vector3 collisionHitLoc = collisionEvents[i].intersection;
            PlayImpact(collisionHitLoc);
            i++;
        }
    }

    protected override void PlayImpact(Vector3 impactPoint) {
        if(calledStop)return;

        Stop();

        if (target == null) {
            bulletImpact.Play(impactPoint);
            return;
        } else {
            ShowHitMsg(target);
        }

        if (!isTargetShield) {
            bulletImpact.Play(impactPoint);
        } else {
            if (isTargetShield) targetWeapon.PlayOnHitEffect();
            else Debug.LogError("targetWeapon is null");
        }
    }
}