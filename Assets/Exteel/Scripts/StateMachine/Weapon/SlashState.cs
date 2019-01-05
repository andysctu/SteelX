﻿using UnityEngine;
using Weapons;

namespace StateMachine.Attack
{
    public class SlashState : MechStateMachineBehaviour
    {
        private bool inAir = false, detectedGrounded;
        private int hand;
        private float threshold = 1;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);

            animator.SetFloat("slashTime", 0);
            animator.SetBool("CanExit", false);

            hand = (stateInfo.IsTag("L")) ? 0 : 1;
            mcbt.OnWeaponStateCallBack<Sword>(hand, this, (int) MeleeWeapon.StateCallBackType.AttackStateEnter); //threshold is set in this

            if (cc == null || !cc.enabled) return;

            inAir = mctrl.IsJumping;
            detectedGrounded = false;

            animator.SetBool(animatorVars.OnMeleeHash, true);
            animator.SetBool(animatorVars.SlashHash, false);
            animator.SetBool(BoostHash, false);

            //Boost Effect
            if (inAir){
                //mctrl.Boost(true);
            } else{
                //mctrl.Boost(false);
            }
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            animator.SetFloat("slashTime", stateInfo.normalizedTime);

            if (stateInfo.normalizedTime > threshold && !animator.IsInTransition(0)){
                animator.SetBool("CanExit", true);
            }

            if (cc == null || !cc.enabled) return;

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
                if (mctrl.Grounded) {
                    detectedGrounded = true;
                    animator.SetBool(JumpHash, false);
                    animator.SetBool(GroundedHash, true);
                    //mctrl.Boost(false);
                }
            }
        }

        override public void OnStateMachineExit(Animator animator, int stateMachinePathHash){
            mcbt.OnWeaponStateCallBack<Sword>(hand, this, (int) MeleeWeapon.StateCallBackType.AttackStateMachineExit);

            if (cc == null || !cc.enabled) return;
            mcbt.CanMeleeAttack = true;
            mcbt.SetMeleePlaying(false);

            animator.SetBool(animatorVars.OnMeleeHash, false);
            animator.SetBool(animatorVars.SlashHash, false);
            animator.SetBool(animatorVars.FinalSlashHash, false);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            animator.SetFloat("slashTime", 0);
            animator.SetBool("CanExit", false);
            mcbt.OnWeaponStateCallBack<Sword>(hand, this, (int) MeleeWeapon.StateCallBackType.AttackStateExit);

            if (cc == null || !cc.enabled) return;

            //mctrl.SetCanVerticalBoost(false);

            if (inAir){
                //exiting from jump melee attack
                animator.SetBool(animatorVars.OnMeleeHash, false);
                animator.SetBool(animatorVars.SlashHash, false);
            }
        }

        public void SetThreshold(float threshold){
            this.threshold = threshold;
        }

        public bool IsInAir(){
            return inAir;
        }
    }
}