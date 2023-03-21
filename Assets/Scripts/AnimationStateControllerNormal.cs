using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class AnimationStateControllerNormal : MonoBehaviour
{
    public Transform cam;
    public GameObject[] visualHitEffect;
    public GameObject[] actionHelpers;
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

    private Vector3 collisionPos;
    private float indicatorTimer = 0;
    private float indicatorCount = 0;
    //fix rotation problem, and transformation
    private bool beenHitted = false;
    //private bool startedBythis = false;
    private bool reseted = true;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
    }
    private void resetTransform() {
        transform.position = stationaryPoint;
        transform.rotation = stationaryRotation;
    }
    private void Start()
    {
        stationaryRotation = transform.rotation;
        stationaryPoint = transform.position;
    }
    private void FixedUpdate()
    {
        if (lightIndicator.enabled && lightIndicator.color == Color.white)
        {
            indicatorCount += Time.deltaTime; 
            if (indicatorCount > 0.1f)
            {
                indicatorCount = 0;
                lightIndicator.enabled = false;
            }
            //startedBythis = false;
        }

        if (started)
        {
            indicatorTimer += Time.deltaTime;
            //type 0 another slash: 1135 804
            //type 1 normal slash: 584 577
            if (behaviour == 0)
            {

                //another slash
                if (indicatorTimer >= 0.804f)
                {
                    
                    indicatorTimer = 0;
                    started = false;
                    lightIndicator.enabled = true;
                   // startedBythis = true;
                }
            }
            else
            {
                //normal slash
                if (indicatorTimer >= 0.577f)
                {
                    
                    indicatorTimer = 0;
                    started = false;
                    lightIndicator.enabled = true;
                   // startedBythis = true; 
                    print("enabled from " + index);
                }
            }
            
        }
        AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        //if((!(animator.GetBool("NormalSlash") || animator.GetBool("AnotherSlash"))&& UserPref.ENEMIES[index].isActive)
        //set the action back to idle
        if (index != -1 &&
            //animationInfo.IsName("Sword And Shield Idle")
            !(animator.GetBool("NormalSlash") || animator.GetBool("AnotherSlash")) &&
            animationInfo.IsName("Sword And Shield Idle")&&
            UserPref.ENEMIES[index].isActive) {
            beenHitted = false;
            // it seems that the info is changed but the animation is not reset
            //Debug.Log("im available now from " + index);
            UserPref.ENEMIES[index].isActive = false;
            reseted = true;
            resetTransform();
        }
    }

    /*
    public void actionPerformed(int animationType)
    {
        string at_ = animator.GetBool("NormalSlash") ? "Normal Slash" : "Another Slash";
        
        //print the time the action performed
        long result = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond - startTime;
        //Debug.Log("action performed: " + result + ", type : " + at_);
        //lightIndicator.enabled = true;
        
    }
    */
    //
    public void startAction(int i)
    {
        if (!reseted)
            return;
        reseted = false;
        //reset before action
        resetTransform();
        startTime = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
        index = i;
        //Debug.Log("Start Action from " + index);
        switch (behaviour)
        {
            case 0:
                animator.SetBool("AnotherSlash", true);
                actionHelpers[0].SetActive(true);
                break;
            case 1:
                animator.SetBool("NormalSlash", true);
                actionHelpers[1].SetActive(true);
                break;
        }
        started = true;
        UserPref.ENEMIES[index].isActive = true;
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
        /*
        switch (performance[0]) { 
            case 0:
                print("From Missed");
                break;
            case -1:
                print("From Body Hit");
                break;
            default:
                print("From Saber");
                break;
        }
        */
        if (performance[0] == -1)
        {
            source.PlayOneShot(clip[2]);
            vibration(35, 2, 255, performance[1] == 0 ? true : false);
            //print("Body hitted");
            controller.SendMessage("Hit", new double[] { -1, counter });
        }
        if (beenHitted)
            return;
        
        //AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (animator.GetBool("NormalSlash") || animator.GetBool("AnotherSlash"))
        {
            //race condition?
            animator.SetBool("AnotherSlash", false);
            animator.SetBool("NormalSlash", false);
            actionHelpers[0].SetActive(false);
            actionHelpers[1].SetActive(false);
            if (performance[0] > 0)
            {
                beenHitted = true;
                source.PlayOneShot(clip[behaviour]);
                vibration(75, 2, 255, performance[2] == 0 ? true : false);
                spawnEffect();
                //print("Im hitted");
                controller.SendMessage("Hit", new double[] { performance[0], counter, performance[1] });
            } else
            {
                //print("Missed");
                controller.SendMessage("Hit", new double[] { performance[0], counter });
            }
            //reset after action
            resetTransform();
        }
    }

    private void vibration(int iteration, int frequency, int strength, bool isLeft)
    {
        int temp = strength;
        OVRHapticsClip hapticsClip = new OVRHapticsClip();
        for (int i = 0; i < iteration; i++)
            hapticsClip.WriteSample(i % frequency == 0 ? (byte)temp-- : (byte)0);
        if (isLeft)
            OVRHaptics.LeftChannel.Preempt(hapticsClip);
        else
            OVRHaptics.RightChannel.Preempt(hapticsClip);
    }

    private void spawnEffect()
    {
        int r = Random.Range(0, 3);
        GameObject newEffect = Instantiate(visualHitEffect[r]);
        newEffect.transform.position = collisionPos;
        newEffect.transform.LookAt(Camera.main.transform);
    }
    public void passPos(Vector3 cep)
    {
        collisionPos = cep;
    }
}
