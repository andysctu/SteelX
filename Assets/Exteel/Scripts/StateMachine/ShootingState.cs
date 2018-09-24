using UnityEngine;

public class ShootingState : MechStateMachineBehaviour {

	public int hand = 0;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
		base.Init(animator);
		if (mcbt == null)return;
        mcbt.OnWeaponStateCallBack<RangedWeapon>(hand, this, (int)RangedWeapon.StateCallBackType.AttackStateEnter);
        //mechIK.SetIK (true, 0, hand);
	}

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if(mcbt == null)return;
        mcbt.OnWeaponStateCallBack<RangedWeapon>(hand, this, (int)RangedWeapon.StateCallBackType.AttackStateUpdate);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        if (mcbt == null) return;
        //mechIK.SetIK (false, 0, hand);
        mcbt.OnWeaponStateCallBack<RangedWeapon>(hand, this, (int)RangedWeapon.StateCallBackType.AttackStateExit);
    }
}	
