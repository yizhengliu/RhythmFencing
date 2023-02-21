using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationStateControllerNormal : MonoBehaviour
{
    public Light lightIndicator;
    public GameObject controller;
    private Animator animator;
    private Vector3 stationaryPoint;
    private long startTime;
    private int index = -1;
    private bool performed = false;
    private Quaternion stationaryRotation;
    private int behaviour;
    private int counter;
    private bool started = false;
    private bool hitted = false;

    private float indicatorTimer = 0;
    private int indicatorCount = 0;
    private bool firstUpdate = true;
    //fix rotation problem, and transformation

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        //record the initial position and rotation
        if (firstUpdate) {
            stationaryRotation = transform.rotation;
            stationaryPoint = transform.position;
            firstUpdate = false;
        }
    }
    private void FixedUpdate()
    {
        if (lightIndicator.enabled) {
            indicatorCount++;
            if (indicatorCount > 1)
            {
                indicatorCount = 0;
                lightIndicator.enabled = false;
            }
        }
        if (started) {
            indicatorTimer += Time.deltaTime;
            //type 0 another slash: 1135 809
            //type 1 normal slash: 584 585
            if (behaviour == 0) {
                //another slash
                if (indicatorTimer > 0.809) { 
                    indicatorTimer = 0;
                    started = false;
                    lightIndicator.enabled = true;
                }
            } else {
                //normal slash
                if (indicatorTimer > 0.585)
                {
                    indicatorTimer = 0;
                    started = false;
                    lightIndicator.enabled = true;
                }
            }
        }
        AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        //set the action back to idle
        if (performed && index != -1 && animationInfo.IsName("Sword And Shield Idle")) {
            UserPref.ENEMIES[index].isActive = false;
            index = -1;
            performed = false;
            transform.position = stationaryPoint;
            transform.rotation = stationaryRotation;
            animator.SetBool("AnotherSlash", false);
            animator.SetBool("NormalSlash", false);
        }
        //set the 2d position to be unchanged
        if (animationInfo.IsName("Another Sword And Shield Slash")
          || animationInfo.IsName("Sword And Shield Normal Slash"))
        {
            performed = true;
            //Vector3 pos = new Vector3(stationaryPoint.x, animator.rootPosition.y, stationaryPoint.z);
            //transform.position = pos;
            transform.position = stationaryPoint;
        }
    }

    public void actionPerformed(int animationType)
    {
        //print the time the action performed
       // long result = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond - startTime;
        //Debug.Log("action performed: " + result + ", type : " + animationType);
        //lightIndicator.enabled = true;
        
    }

    //
    public void startAction(int i)
    {
        transform.position = stationaryPoint;
        transform.rotation = stationaryRotation;
        startTime = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
        index = i;

        switch (behaviour)
        {
            case 0:
                animator.SetBool("AnotherSlash", true);
                break;
            case 1:
                animator.SetBool("NormalSlash", true);
                break;
        }
        started = true;
    }
    public void ActionEnd() {
        //if (hitted > 0){
        //hitted--;
        //return;
        //}
        if (hitted) { 
            hitted = false;
            return;
        }
        Debug.Log("Animation Ending...");
        animator.SetBool("AnotherSlash", false);
        animator.SetBool("NormalSlash", false);
        transform.position = stationaryPoint;
        transform.rotation = stationaryRotation;
        //missed
        Hit(new double[] {0});
    }

    public void setBehaviour(int behaviour)
    {
        this.behaviour = behaviour;
    }
    public void counterIndex(int count) {
        counter = count;
    }
    public void Hit(double[] performance) {
        AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (animationInfo.IsName("Another Sword And Shield Slash")
          || animationInfo.IsName("Sword And Shield Normal Slash")) {
            if (performance[0] != 0)
            {
                print("Hitted");
                hitted = true;
            }
            else
                print("Missed");
            animator.SetBool("AnotherSlash", false);
            animator.SetBool("NormalSlash", false);
            animator.SetBool("Back", true);
            if(performance.Length == 1)
                controller.SendMessage("Hit", new double[] { performance[0], counter });
            else
                controller.SendMessage("Hit", new double[] { performance[0], counter, performance[1] });
        }
    }


}
