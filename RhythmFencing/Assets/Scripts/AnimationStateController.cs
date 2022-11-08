using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    private Animator animator;

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
            if (Vector3.Distance(transform.position, Vector3.zero) < 3) 
                animator.SetBool("StartAttack", true);
                
        }
        else if (animationInfo.IsName("Sword And Shield Slash")) {
            if (animationInfo.normalizedTime > 1.0f) {
                Destroy(this.gameObject);
            }
        }
        
        
    }
}
