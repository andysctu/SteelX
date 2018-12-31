using UnityEngine;
using Weapons;

namespace StateMachine.Attack
{
    public class SmashState : MechStateMachineBehaviour
    {
        private bool inAir = false, detectedGrounded;
        private int hand;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);

            hand = (stateInfo.IsTag("L")) ? 0 : 1;
            mcbt.OnWeaponStateCallBack<Spear>(hand, this, (int) MeleeWeapon.StateCallBackType.AttackStateEnter); //threshold is set in this

            if (cc == null || !cc.enabled) return;

            animator.SetBool(animatorVars.OnMeleeHash, true);
            animator.SetBool(BoostHash, false);
            inAir = animator.GetBool(JumpHash);

            detectedGrounded = false;

            mcbt.CanMeleeAttack = !animator.GetBool(JumpHash);
            mcbt.SetMeleePlaying(true);
            mctrl.ResetCurBoostingSpeed();

            if (inAir){
                //mctrl.Boost(true);
            } else{
                //mctrl.Boost(false);
            }
        }

        // OnStateMachineExit is called when exiting a statemachine via its Exit Node
        override public void OnStateMachineExit(Animator animator, int stateMachinePathHash){
            mcbt.OnWeaponStateCallBack<Spear>(hand, this, (int) MeleeWeapon.StateCallBackType.AttackStateMachineExit);

            if (cc == null || !cc.enabled) return;
            mcbt.CanMeleeAttack = true;
            mcbt.SetMeleePlaying(false);

            animator.SetBool(animatorVars.OnMeleeHash, false);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            if (cc == null || !cc.enabled) return;
            mcbt.CanMeleeAttack = !animator.GetBool(JumpHash);

            bool b = (inAir && !mcbt.IsMeleePlaying());

            if (b){
                //mctrl.JumpMoveInAir();
            }

            mctrl.CallLockMechRot(!animator.IsInTransition(0));

            if (stateInfo.normalizedTime > 0.5f && !detectedGrounded){
                if (b){
                    //mctrl.Boost(false);
                }

                mcbt.CanMeleeAttack = !animator.GetBool(JumpHash);
                //if (mctrl.CheckIsGrounded()) {
                //    detectedGrounded = true;
                //    //mctrl.grounded = true;
                //    mctrl.SetCanVerticalBoost(false);
                //    animator.SetBool(JumpHash, false);
                //    animator.SetBool(GroundedHash, true);
                //    //mctrl.Boost(false);
                //}
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            mcbt.OnWeaponStateCallBack<Spear>(hand, this, (int) MeleeWeapon.StateCallBackType.AttackStateExit);

            if (cc == null || !cc.enabled) return;

            //mctrl.SetCanVerticalBoost(false);

            if (inAir){
                //exiting from jump melee attack
                animator.SetBool(animatorVars.OnMeleeHash, false);
                mcbt.SetMeleePlaying(false);
            } else{
                mcbt.CanMeleeAttack = true; //sometimes OnstateMachineExit does not ensure canslash set to true ( called before update )
            }

            mctrl.CallLockMechRot(false);
        }

        public bool IsInAir(){
            return inAir;
        }
    }
}