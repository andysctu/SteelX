using UnityEngine;

public class Rifle : RangedWeapon {
    public override void OnTargetEffect(GameObject target, bool isShield) {
        throw new System.NotImplementedException();
    }

    protected override void LoadSoundClips() {
        
    }
}