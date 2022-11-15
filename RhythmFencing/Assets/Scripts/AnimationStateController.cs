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

                switch (Random.Range(0, 3))
                {
                    case 0:
                        animator.SetBool("StartAttack", true);
                        break;
                    case 1:
                        animator.SetBool("RotateSlash", true);
                        break;
                    case 2:
                        animator.SetBool("NormalSlash", true);
                        break;
                }
                
        }
        else if (animationInfo.IsName("Sword And Shield Slash") 
            || animationInfo.IsName("Sword And Shield Attack") 
            || animationInfo.IsName("Sword And Shield Normal Slash")) {
            if (animationInfo.normalizedTime > 1.0f) {
                Destroy(this.gameObject);
            }
        }
        
        
    }
}
