using UnityEngine;

public class Spear : MeleeWeapon {
    private AudioClip smashSound, smashOnHitSound;

    public override void AttackTarget(GameObject target, bool isShield) {
        throw new System.NotImplementedException();
    }

    public override void HandleAnimation() {
        throw new System.NotImplementedException();
    }

    public override void HandleCombat() {
        if (!Input.GetKeyDown(hand == LEFT_HAND ? KeyCode.Mouse0 : KeyCode.Mouse1) || IsOverHeat()) {
            return;
        }




    }

    protected override void LoadSoundClips() {
        smashSound = ((SpearData)data).smash_sound;
        smashOnHitSound = ((SpearData)data).smash_hit_sound;
    }


}