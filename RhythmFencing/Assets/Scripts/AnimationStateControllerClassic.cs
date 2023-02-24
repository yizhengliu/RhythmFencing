using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

public class AnimationStateControllerClassic : MonoBehaviour
{
    private GameObject controller;
    private Animator animator;
    private Vector3 stationaryPoint;
    private int behaviour;
    // Start is called before the first frame update
    private long startTime;
    private Transform destination;
    private int spawner;
    private int counter;
    void Start()
    {
        animator = GetComponent<Animator>();
        startTime = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
    }
    private void FixedUpdate()
    {
        //if the z coordi is reached

        if (!(animator.GetBool("NormalSlash") || animator.GetBool("AnotherSlash")))
        {
            //if it is running
            //distance needs to be checked
            if (Vector3.Distance(transform.position, destination.position) < 2.4273f)
            {
                //Quaternion rot = transform.rotation;
                //rot.y = Quaternion.LookRotation(transform.position - destination.position).y;
                stationaryPoint = transform.position;
                switch (behaviour)
                {
                    case 0:
                        animator.SetBool("AnotherSlash", true);
                        break;
                    case 1:
                        animator.SetBool("NormalSlash", true);
                        break;
                }

            }
        }
        else
        {
            //Vector3 pos = new Vector3(stationaryPoint.x, animator.rootPosition.y, stationaryPoint.z);
            //transform.position = pos;
        }
    }
    public void ActionEnd() {
        //missed
        controller.SendMessage("Hit", new double[] { 0, counter });
        Destroy(this.gameObject);
    }
    public void actionPerformed(int animationType) {
        long result = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond - startTime;
        Debug.Log("" + result + ", type : " + animationType + ", spawner : " + spawner);
    }
    public void setController(GameObject controller) {
        this.controller = controller;
    }

    public void setBehaviour(int behaviour) {
        this.behaviour = behaviour;
    }

    public void setDestination(Transform dest) {
        destination = dest;
    }

    public void setSpawner(int spawner) {
        this.spawner = spawner;
    }

    public void setCounter(int count) {
        counter = count;
    }

    public void Hit(double[] performance)
    {
        if (performance[0] == -1)
        {
            controller.SendMessage("Hit", new double[] { -1, counter });
            Destroy(this.gameObject);
            return;
        }
        //AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (animator.GetBool("NormalSlash") || animator.GetBool("AnotherSlash"))
        {
            if (performance[0] != 0)
            {
                //source.PlayOneShot(clip[behaviour]);
                controller.SendMessage("playHitEffect", behaviour);
                vibration(75, 2, 255, performance[2] == 0 ? true : false);
                print("from saber");
                controller.SendMessage("Hit", new double[] { performance[0], counter, performance[1] });
                Destroy(this.gameObject);
            }
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
