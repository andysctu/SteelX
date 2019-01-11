using UnityEngine;

public class SlashState : MechStateMachineBehaviour {
    private bool inAir = false, detectedGrounded;
    private int hand;
    private float threshold = 1;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.Init(animator);

        animator.SetFloat("slashTime", 0);
        animator.SetBool("CanExit", false);

        hand = (stateInfo.IsTag("L")) ? 0 : 1;
        mcbt.OnWeaponStateCallBack<Sword>(hand, this, (int)MeleeWeapon.StateCallBackType.AttackStateEnter);//threshold is set in this

        if (cc == null || !cc.enabled) return;

        mcbt.CanMeleeAttack = !animator.GetBool(jump_id);
        mcbt.SetMeleePlaying(true);
        mctrl.ResetCurBoostingSpeed();

        inAir = animator.GetBool(jump_id);
        detectedGrounded = false;

        animator.SetBool(onMelee_id, true);
        animator.SetBool(slash_id, false);
        animator.SetBool(boost_id, false);

        //Boost Effect
        if (inAir) {
            mctrl.Boost(true);
        } else {
            mctrl.Boost(false);
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetFloat("slashTime", stateInfo.normalizedTime);

        if (stateInfo.normalizedTime > threshold && !animator.IsInTransition(0)) {
            animator.SetBool("CanExit", true);
        }

        if (cc == null || !cc.enabled) return;

        bool b = (inAir && !mcbt.IsMeleePlaying());
        if (b) {
            mctrl.JumpMoveInAir();
        }

        mctrl.CallLockMechRot(!animator.IsInTransition(0));

        if (stateInfo.normalizedTime > 0.5f && !detectedGrounded) {
            if (b) {
                mctrl.Boost(false);
            }

            mcbt.CanMeleeAttack = !animator.GetBool(jump_id);
            if (mctrl.CheckIsGrounded()) {
                detectedGrounded = true;
                mctrl.grounded = true;
                mctrl.SetCanVerticalBoost(false);
                animator.SetBool(jump_id, false);
                animator.SetBool(grounded_id, true);
                mctrl.Boost(false);
            }
        }
    }

    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
        mcbt.OnWeaponStateCallBack<Sword>(hand, this, (int)MeleeWeapon.StateCallBackType.AttackStateMachineExit);

        if (cc == null || !cc.enabled) return;
        mcbt.CanMeleeAttack = true;
        mcbt.SetMeleePlaying(false);

        animator.SetBool(onMelee_id, false);
        animator.SetBool(slash_id, false);
        animator.SetBool(finalSlash_id, false);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        animator.SetFloat("slashTime", 0);
        animator.SetBool("CanExit", false);
        mcbt.OnWeaponStateCallBack<Sword>(hand, this, (int)MeleeWeapon.StateCallBackType.AttackStateExit);

        if (cc == null || !cc.enabled)return;

        mctrl.SetCanVerticalBoost(false);

        if (inAir) {//exiting from jump melee attack
            animator.SetBool(onMelee_id, false);
            mcbt.SetMeleePlaying(false);
            animator.SetBool(slash_id, false);
        } else {
            mcbt.CanMeleeAttack = true;//sometimes OnstateMachineExit does not ensure canslash set to true ( called before update )
        }

        mctrl.CallLockMechRot(false);
    }

    public void SetThreshold(float threshold) {
        this.threshold = threshold;
    }

    public bool IsInAir() {
        return inAir;
    }
}