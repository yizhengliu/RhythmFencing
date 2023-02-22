using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class AnimationStateControllerNormal : MonoBehaviour
{
    public AudioClip[] clip;
    public Light lightIndicator;
    public GameObject controller;
    private Animator animator;
    private Vector3 stationaryPoint;
    private long startTime;
    private int index = -1;
    private Quaternion stationaryRotation;
    private int behaviour;
    private int counter;
    private bool started = false;
    private AudioSource source;

    private float indicatorTimer = 0;
    private int indicatorCount = 0;
    private bool firstUpdate = true;
    //fix rotation problem, and transformation
    private bool beenHitted = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
    }
    private void resetTransform() {
        transform.position = stationaryPoint;
        transform.rotation = stationaryRotation;
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
        if (lightIndicator.enabled)
        {
            indicatorCount++;
            if (indicatorCount > 1)
            {
                indicatorCount = 0;
                lightIndicator.enabled = false;
            }
        }

        if (started)
        {
            indicatorTimer += Time.deltaTime;
            //type 0 another slash: 1135 809
            //type 1 normal slash: 584 585
            if (behaviour == 0)
            {

                //another slash
                if (indicatorTimer > 0.809)
                {
                    indicatorTimer = 0;
                    started = false;
                    lightIndicator.enabled = true;
                }
            }
            else
            {
                //normal slash
                if (indicatorTimer > 0.585)
                {
                    indicatorTimer = 0;
                    started = false;
                    lightIndicator.enabled = true;
                }
            }
        }
        //AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        //set the action back to idle
        if (index != -1 &&
            //animationInfo.IsName("Sword And Shield Idle")
            !(animator.GetBool("NormalSlash") || animator.GetBool("AnotherSlash"))) {
            beenHitted = false;
            // it seems that the info is changed but the animation is not reset
            Debug.Log("im available now");
            UserPref.ENEMIES[index].isActive = false;
            index = -1;
            resetTransform();
        }
    }

    public void actionPerformed(int animationType)
    {
        string at_ = animator.GetBool("NormalSlash") ? "Normal Slash" : "Another Slash";
        
        //print the time the action performed
        long result = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond - startTime;
        Debug.Log("action performed: " + result + ", type : " + at_);
        //lightIndicator.enabled = true;
        
    }

    //
    public void startAction(int i)
    {
        Debug.Log("Start Action"); 
        //reset before action
        resetTransform();
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
    public void Hit(double[] performance)
    {
        if (performance[0] == -1) {
            controller.SendMessage("Hit", new double[] { -1, counter });
            return;
        }
        if (beenHitted)
            return;
        beenHitted = true;
        //AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (
          animator.GetBool("NormalSlash") || animator.GetBool("AnotherSlash"))
        {
            
            animator.SetBool("AnotherSlash", false);
            animator.SetBool("NormalSlash", false);

            if (performance[0] != 0)
            {
                source.PlayOneShot(clip[behaviour]);
                vibration(75, 2, 255, performance[2] == 0 ? true : false);
                print("from saber");
                controller.SendMessage("Hit", new double[] { performance[0], counter, performance[1] });
            }
            else
            {
                print("from action end");
                controller.SendMessage("Hit", new double[] { performance[0], counter });
            }
            //reset after action
            resetTransform();
        }
    }

    private void vibration(int iteration, int frequency, int strength, bool isLeft)
    {
        OVRHapticsClip hapticsClip = new OVRHapticsClip();
        for (int i = 0; i < iteration; i++)
            hapticsClip.WriteSample(i % frequency == 0 ? (byte)strength : (byte)0);
        if (isLeft)
            OVRHaptics.LeftChannel.Preempt(hapticsClip);
        else
            OVRHaptics.RightChannel.Preempt(hapticsClip);
    }
}
