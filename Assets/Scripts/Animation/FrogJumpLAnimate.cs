﻿using UnityEngine;
using System.Collections;

public class FrogJumpLAnimate : StateMachineBehaviour
{
    private float animTime;
    private Rigidbody2D player;

    override public void OnStateUpdate(Animator anim, AnimatorStateInfo animInfo, int layerIndex)
    {
        player = anim.transform.parent.GetComponent<Rigidbody2D>();
        animTime = animInfo.normalizedTime;
        if (player.velocity.x > 0)
        {
            anim.Play("Jump AnimationR", 0, animTime);
        }
    }
}
