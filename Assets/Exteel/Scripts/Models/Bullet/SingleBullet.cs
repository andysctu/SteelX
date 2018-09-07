using UnityEngine;

public class SingleBullet : Bullet {
    private Rigidbody Rigidbody;
    private Vector3 direction;

    private const float BULLETSPEED = 300;

    protected override void Awake() {
        base.Awake();

        InitComponents();
    }

    public void SetDirection(Vector3 direction) {
        this.direction = direction;
    }

    private void InitComponents() {
        AttachRigidbody();
    }

    private void AttachRigidbody() {
        Rigidbody = gameObject.AddComponent<Rigidbody>();        
    }

    public override void Play() {
        if (isfollow) {
            if(target == null)return;
            transform.LookAt(target);
            bullet_ps.Play();
        } else {
            transform.LookAt(direction * 9999);
            Rigidbody.velocity = direction * BULLETSPEED;
            bullet_ps.Play();
        }
    }

    public override void Stop() {
        bullet_ps.Stop();
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