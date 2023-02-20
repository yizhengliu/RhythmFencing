using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateControllerNormal : MonoBehaviour
{
    public Light lightIndicator;
    public GameObject controller;
    private Animator animator;
    private Vector3 stationaryPoint;
    // Start is called before the first frame update
    private long startTime;
    private int index = -1;
    private bool performed = false;
    private Quaternion stationaryRotation;
    private int behaviour;
    private int counter;
    //fix rotation problem, and transformation

    private void Awake()
    {
        animator = GetComponent<Animator>();
        stationaryRotation = transform.rotation;
        stationaryPoint = transform.position;
    }
    private void FixedUpdate()
    { 
        
        if (lightIndicator.enabled)
            lightIndicator.enabled = false;
        AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        //set the action back to idle
        if (performed && index != -1 && animationInfo.IsName("Sword And Shield Idle")) {
            UserPref.ENEMIES[index].isActive = false;
            performed = false;

            animator.SetBool("Back", false);
        }
        //set the 2d position to be unchanged
        if (animationInfo.IsName("Another Sword And Shield Slash")
          || animationInfo.IsName("Sword And Shield Normal Slash"))
        {
            performed = true;
            Vector3 pos = new Vector3(stationaryPoint.x, animator.rootPosition.y, stationaryPoint.z);
            transform.position = pos;
        }
    }

    public void actionPerformed(int animationType)
    {
        //print the time the action performed
        long result = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond - startTime;
        Debug.Log("" + result + ", type : " + animationType);
        lightIndicator.enabled = true;
    }
    
    //
    public void startAction(int i){
        transform.position = stationaryPoint;
        transform.rotation = stationaryRotation;
        startTime = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
        index = i;
        AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (animationInfo.IsName("Sword And Shield Idle"))
        {
            switch (behaviour){
                case 0:
                    animator.SetBool("NormalSlash", true);
                    break;
                case 1:
                    animator.SetBool("AnotherSlash", true);
                    break;
            }
        }
    }
    public void ActionEnd() {
        Debug.Log("ending");
        animator.SetBool("AnotherSlash", false);
        animator.SetBool("NormalSlash", false);
        transform.position = stationaryPoint;
        transform.rotation = stationaryRotation;
        animator.SetBool("Back", true);
        //missed
        Hit(0);
    }

    public void setBehaviour(int behaviour)
    {
        this.behaviour = behaviour;
    }
    public void counterIndex(int count) {
        counter = count;
    }
    public void Hit(int performance) {
        AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (animationInfo.IsName("Another Sword And Shield Slash")
          || animationInfo.IsName("Sword And Shield Normal Slash")) {
            print("im hitted");
            animator.SetBool("AnotherSlash", false);
            animator.SetBool("NormalSlash", false);
            transform.position = stationaryPoint;
            transform.rotation = stationaryRotation;
            animator.SetBool("Back", true);
            controller.SendMessage("Hit", new int[] { performance, counter });
        }
    }


}
