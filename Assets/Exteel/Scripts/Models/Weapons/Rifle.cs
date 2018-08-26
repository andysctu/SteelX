using UnityEngine;

public class Rifle : RangedWeapon {
    public override void AttackTarget(GameObject target, bool isShield) {
        throw new System.NotImplementedException();
    }

    protected override void LoadSoundClips() {
        
    }
}