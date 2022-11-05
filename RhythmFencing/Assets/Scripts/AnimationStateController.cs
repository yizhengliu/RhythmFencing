using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        AnimatorStateInfo animationInfo =animator.GetCurrentAnimatorStateInfo(0);
        //if the z coordi is reached
        if (animationInfo.IsName("Sword And Shield Run"))
        {
            if (transform.position.z < 3)
                animator.SetBool("StartAttack", true);
        }
        else if (animationInfo.IsName("Sword And Shield Slash")) {
            if (animationInfo.normalizedTime > 1.0f) {
                Destroy(this.gameObject);
            }
        }
        
        
    }
}
