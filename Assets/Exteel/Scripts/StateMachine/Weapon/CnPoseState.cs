using UnityEngine;
using Weapons;

namespace StateMachine.Attack
{
    public class CnPoseState : MechStateMachineBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);
            if (mcbt == null) return;
            mcbt.OnWeaponStateCallBack<Cannon>(0, this, (int) RangedWeapon.StateCallBackType.PoseStateEnter);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (mcbt == null) return;
            mcbt.OnWeaponStateCallBack<Cannon>(0, this, (int) RangedWeapon.StateCallBackType.PoseStateExit);
            animator.SetBool("CnPose", false);
        }
    }
}