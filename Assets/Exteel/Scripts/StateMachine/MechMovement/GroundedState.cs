using UnityEngine;

public class GroundedState : MechStateMachineBehaviour {
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.Init(animator);

        if ((!animatorVars.RootPv.isMine && !PhotonNetwork.isMasterClient) || !cc.enabled) return;

        animator.SetBool(GroundedHash, true);
        animator.SetBool(animatorVars.OnMeleeHash, false);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        mctrl.EnableBoostFlame(animator.GetBool("Boost"));

        if ((!animatorVars.RootPv.isMine && !PhotonNetwork.isMasterClient) || !cc.enabled) return;

        animator.SetFloat(SpeedHash, Mathf.Lerp(animator.GetFloat(SpeedHash), mctrl.Speed, Time.deltaTime * 15));
        animator.SetFloat(DirectionHash, Mathf.Lerp(animator.GetFloat(DirectionHash), mctrl.Direction, Time.deltaTime * 15));

        if (mctrl.IsJumping){//TODO : condition : you can't jump if playing melee attack
            Debug.Log("set to jump");
            animator.SetBool(GroundedHash, false);
            animator.SetBool(JumpHash, true);
            return;
        }

        if (!mctrl.Grounded) {//check not jumping but is falling
            animator.SetBool(GroundedHash, false);
            return;
        }

        if (mctrl.IsBoosting){
            animator.SetBool(BoostHash,true);
        }
    }
}