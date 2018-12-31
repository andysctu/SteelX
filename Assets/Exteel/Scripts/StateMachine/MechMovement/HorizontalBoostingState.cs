using UnityEngine;

namespace StateMachine.MechMovement
{
    public class HorizontalBoostingState : MechStateMachineBehaviour
    {

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            base.Init(animator);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
            EffectController.UpdateBoostingDust();

            mctrl.EnableBoostFlame(animator.GetBool("Boost"));

            if ((mctrl.GetOwner() != null && !mctrl.GetOwner().IsLocal && !PhotonNetwork.isMasterClient) || !cc.enabled) return;

            animator.SetFloat(SpeedHash, Mathf.Lerp(animator.GetFloat(SpeedHash), mctrl.Speed, Time.deltaTime * 15));
            animator.SetFloat(DirectionHash, Mathf.Lerp(animator.GetFloat(DirectionHash), mctrl.Direction, Time.deltaTime * 15));

            if (!mctrl.Grounded){
                animator.SetBool(GroundedHash, false);
            }

            if (mctrl.IsJumping){
                animator.SetBool(JumpHash, true);
            }

            animator.SetBool(BoostHash, mctrl.IsBoosting);
        }
    }
}