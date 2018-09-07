using UnityEngine;

public class Shotgun : RangedWeapon {
    public override void OnStateCallBack(int type, MechStateMachineBehaviour state) {
        throw new System.NotImplementedException();
    }

    public override void OnTargetEffect(GameObject target, Weapon targetWeapon, bool isShield) {
        throw new System.NotImplementedException();
    }

    protected override void UpdateAnimationSpeed() {
        throw new System.NotImplementedException();
    }

    protected override void DisplayBullet(Vector3 direction, GameObject Target, Weapon targetWeapon) {
        throw new System.NotImplementedException();
    }

    protected override void InitAttackType() {
        throw new System.NotImplementedException();
    }

    protected override void UpdateMuzzleEffect() {
        throw new System.NotImplementedException();
    }

    protected override void LoadSoundClips() {
        throw new System.NotImplementedException();
    }

    protected override void UpdateMechArmState() {
        throw new System.NotImplementedException();
    }
}