using UnityEngine;

public class Rocket : RangedWeapon {
    private AudioClip shotSound, reloadSound;
    private Bullet bullet;

    private float animationStartTime;

    public Rocket() {
        allowBothWeaponUsing = false;
    }

    protected override void InitAttackType() {
        attackType = AttackType.Ranged;
    }

    protected override void LoadSoundClips() {
        shotSound = ((RocketData)data).shotSound;
        reloadSound = ((RocketData)data).reload_sound;
    }

    protected override void InitAtkAnimHash() {
        AtkAnimHash = Animator.StringToHash("AtkL") ;
    }

    public override void HandleCombat() {
        base.HandleCombat();
    }

    public override void OnTargetEffect(GameObject target, Weapon targetWeapon, bool isShield) {
        if (data.slowDown && !isShield) {
            //target.GetComponent<MechController>().SlowDown();
        }
    }

    protected override void UpdateAnimationSpeed() {
    }

    private void UpdateBulletEffect(ParticleSystem Bullet_ps) {
    }

    protected override void UpdateMuzzleEffect() {
    }

    protected override void UpdateMechArmState() {
        MechAnimator.Play("Rocket", 1);
        MechAnimator.Play("Rocket", 2);
    }

    public override void HandleAnimation() {
        if (isFiring) {
            if (Time.time - startShootTime >= 0.1f) { //0.1f : animation min time
                if (isAtkAnimationPlaying) {
                    isAtkAnimationPlaying = false;
                    MechAnimator.SetBool(AtkAnimHash, false);
                }
            } else {
                if (!isAtkAnimationPlaying) {
                    MechAnimator.SetBool(AtkAnimHash, true);
                    isAtkAnimationPlaying = true;
                }
            }
        } else {
            if (isAtkAnimationPlaying) {
                MechAnimator.SetBool(AtkAnimHash, false);
                isAtkAnimationPlaying = false;
            }
        }
    }

    protected override void FireRaycast(Vector3 start, Vector3 direction, int hand) {
        //does Rocket follow target ?
        //Transform target = ((hand == 0) ? Crosshair.getCurrentTargetL() : Crosshair.getCurrentTargetR());//target might be shield collider

        Transform target = null;

        if (target != null) {
            PhotonView targetpv = target.transform.root.GetComponent<PhotonView>();

            if (target.tag != "Shield") {
                photonView.RPC("Shoot", PhotonTargets.All, weapPos, direction, targetpv.owner, targetpv.viewID, -1);
            } else {//check what hand is it
                ShieldActionReceiver ShieldActionReceiver = target.parent.GetComponent<ShieldActionReceiver>();
                int target_ShieldPos = ShieldActionReceiver.GetPos();

                photonView.RPC("Shoot", PhotonTargets.All, weapPos, direction, targetpv.owner, targetpv.viewID, target_ShieldPos);
            }
        } else {
            photonView.RPC("Shoot", PhotonTargets.All, weapPos, direction, null, -1, -1);
        }
    }

    public override void Shoot(Vector3 direction, PhotonPlayer TargetPlayer, int target_pvID, int targetWeapPos) {
        MechAnimator.SetBool(AtkAnimHash, true);
        WeaponAnimator.SetTrigger("Atk");

        isFiring = true;
        startShootTime = Time.time;

        PhotonView targetpv = null;
        GameObject Target = null;

        //Get the target
        if (TargetPlayer == null) {
            targetpv = PhotonView.Find(target_pvID);
            if (targetpv != null) Target = targetpv.gameObject;
        } else {
            Target = (GameObject)TargetPlayer.TagObject;
        }

        if (Target != null) {
            Combat targetCbt = Target.GetComponent<Combat>();
            targetCbt.OnHit(data.damage, photonView.owner, photonView.viewID, weapPos, targetWeapPos);

            if(PhotonNetwork.isMasterClient)DisplayBullet(direction, Target, (targetWeapPos == -1) ? null : targetCbt.GetWeapon(targetWeapPos));

            Cbt.IncreaseSP(data.SPincreaseAmount);
        } else {
            if (PhotonNetwork.isMasterClient)DisplayBullet(direction, null, null);
        }
    }

    protected override void DisplayBullet(Vector3 direction, GameObject Target, Weapon targetWeapon) {//Call by master client
        bullet = PhotonNetwork.Instantiate(BulletPrefab.name, Effect_End.position, Quaternion.identity, 0).GetComponent<Bullet>();

        bullet.InitBulletTrace(MechCam, photonView);
        bullet.SetTarget((Target == null) ? null : Target.transform, targetWeapon);
        bullet.SetDirection(direction);
    }

    public override void OnSkillAction(bool b) {
        base.OnSkillAction(b);
        if (b) {//Stop effects playing when entering
            Muzzle.Stop();
            AudioSource.Stop();
        }
    }

    public override void OnSwitchedWeaponAction(bool b) {
        base.OnSwitchedWeaponAction(b);

        if (!b) {
            Muzzle.Stop();
            AudioSource.Stop();
        }
    }

    public override void OnStateCallBack(int type, MechStateMachineBehaviour state) {
        switch ((StateCallBackType)type) {
            case StateCallBackType.AttackStateEnter:
            //MechAnimator.Update(0);
            Muzzle.Play();
            AudioSource.PlayOneShot(shotSound);
            if (photonView.isMine) Crosshair.CallShakingEffect(hand);
            break;
            case StateCallBackType.AttackStateUpdate:
            //if (Time.time - animationStartTime >= 0.05f) {
                //bullet.transform.position = Effect_End.position;
                //bullet.Play();
                //bullet = null;
                //Muzzle.Play();
                //AudioSource.PlayOneShot(shotSound);
                //if (photonView.isMine) Crosshair.CallShakingEffect(hand);
            //}
            break;
            case StateCallBackType.AttackStateExit:
            WeaponAnimator.SetTrigger("Reload");
            if (reloadSound != null) AudioSource.PlayOneShot(reloadSound);
            break;
            default:
            Debug.Log("should not go here");
            break;
        }
    }
}