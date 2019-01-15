using StateMachine;
using UnityEngine;
using Weapons;
using Weapons.Bullets;

public class Rectifier : RangedWeapon
{
    private AudioClip _shotSound;
    private Bullet _bullet;

    public override void Init(WeaponData data, int hand, Transform handTransform, Combat Cbt, Animator Animator){
        base.Init(data, hand, handTransform, Cbt, Animator);

        InstantiateBullet();
    }

    protected override void AddAudioSource(GameObject weapon){
        WeaponAudioSource = weapon.AddComponent<AudioSource>();

        //Init AudioSource
        WeaponAudioSource.spatialBlend = 1;
        WeaponAudioSource.dopplerLevel = 0;
        WeaponAudioSource.volume = 1f;
        WeaponAudioSource.loop = true;
        WeaponAudioSource.playOnAwake = false;
        WeaponAudioSource.minDistance = 20;
        WeaponAudioSource.maxDistance = 250;
    }

    private void InstantiateBullet(){
        _bullet = Object.Instantiate(BulletPrefab).GetComponent<Bullet>();
        _bullet.transform.SetParent(EffectEnd);
        TransformExtension.SetLocalTransform(_bullet.transform);
    }

    public override void OnHitTargetAction(GameObject target, Weapon targetWeapon, bool isShield){
    }

    protected void UpdateAnimationSpeed(){
    }

    public override void HandleAnimation(){
        if (IsFiring){
            if (Time.time - startShootTime >= 1 / Rate){
                if (atkAnimationIsPlaying){
                    atkAnimationIsPlaying = false;
                    MechAnimator.SetBool(AtkAnimHash, false);
                    WeaponAudioSource.Stop();
                }
            } else{
                if (!atkAnimationIsPlaying){
                    MechAnimator.SetBool(AtkAnimHash, true);
                    atkAnimationIsPlaying = true;
                }
            }
        } else{
            if (atkAnimationIsPlaying){
                MechAnimator.SetBool(AtkAnimHash, false);
                atkAnimationIsPlaying = false;
            }
        }
    }

    protected override void FireRaycast(Vector3 start, Vector3 direction, int hand){
        Transform target = ((hand == 0) ? CrosshairController.GetCurrentTargetL() : CrosshairController.GetCurrentTargetR());

        if (target != null){
            PhotonView targetPv = target.transform.root.GetComponent<PhotonView>();

            if (target.tag != "Shield"){
                PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, targetPv.viewID, -1);
            } else{
                //check what hand is it
                ShieldActionReceiver shieldActionReceiver = target.parent.GetComponent<ShieldActionReceiver>();
                int targetShieldPos = shieldActionReceiver.GetPos();

                PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, targetPv.viewID, targetShieldPos);
            }
        } else{
            PlayerPv.RPC("Shoot", PhotonTargets.All, WeapPos, direction, -1, -1);
        }
    }

    public override void Shoot(Vector3 direction, int targetPvId, int targetWeapPos){
        MechAnimator.SetBool(AtkAnimHash, true);
        WeaponAnimator.SetTrigger("Atk");
        Muzzle.Play();

        IsFiring = true;
        startShootTime = Time.time;

        GameObject target = null;
        PhotonView targetPv = PhotonView.Find(targetPvId);
        if (targetPv != null) target = targetPv.gameObject;

        if (target != null){
            Combat targetCbt = target.GetComponent<Combat>();
            targetCbt.OnHit(data.damage, PlayerPv.viewID, WeapPos, targetWeapPos);

            DisplayBullet(direction, target, (targetWeapPos == -1) ? null : targetCbt.GetWeapon(targetWeapPos));

            Cbt.IncreaseSP(data.SpIncreaseAmount); //TODO : check this
        } else{
            DisplayBullet(direction, null, null);
        }

        WeaponAudioSource.Play();
    }

    protected override void DisplayBullet(Vector3 direction, GameObject target, Weapon targetWeapon){
        _bullet.InitBullet(MechCam, PlayerPv, direction, (target == null) ? null : target.transform, this, targetWeapon);
        _bullet.Play();
    }

    public override void OnSkillAction(bool enter){
        base.OnSkillAction(enter);
        if (enter){
            //Stop effects playing when entering
            Muzzle.Stop();
            WeaponAudioSource.Stop();
        }
    }

    protected override void LoadSoundClips(){
        _shotSound = ((RectifierData) data).shotSound;
        WeaponAudioSource.clip = _shotSound;
    }

    protected override void UpdateMechArmState(){
        MechAnimator.Play("ENG", 1);
        MechAnimator.Play("ENG", 2);
    }

    public override void OnStateCallBack(int type, MechStateMachineBehaviour state){
        switch (type){
            case (int) StateCallBackType.AttackStateEnter:
                break;
            case (int) StateCallBackType.AttackStateExit:
                if (_bullet != null) _bullet.Stop();
                Muzzle.Stop();
                WeaponAudioSource.Stop();
                break;
        }
    }
}
