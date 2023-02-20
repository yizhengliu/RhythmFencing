using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateControllerClassic : MonoBehaviour
{
    public Light lightIndicator;
    public GameObject controller;
    private Animator animator;
    private Vector3 stationaryPoint;
    private int behaviour;
    // Start is called before the first frame update
    private long startTime;
    private Transform destination;
    private int spawner;

    private int indicatorCount = 0;
    private int counter;
    void Start()
    {
        animator = GetComponent<Animator>(); 
        startTime = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
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
        AnimatorStateInfo animationInfo = animator.GetCurrentAnimatorStateInfo(0);
        //if the z coordi is reached
        if (animationInfo.IsName("Sword And Shield Run"))
        {

            if (Vector3.Distance(transform.position, destination.position) < 3f)
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
                started = true;

            }
        }
        else if (animationInfo.IsName("Sword And Shield Slash")
          || animationInfo.IsName("Sword And Shield Attack")
          || animationInfo.IsName("Sword And Shield Normal Slash"))
        {
            Vector3 pos = new Vector3(stationaryPoint.x, animator.rootPosition.y, stationaryPoint.z);
            transform.position = pos;
            if (animationInfo.normalizedTime > 1.0f)
            {
                //long result = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond - startTime;
                //Debug.Log("" + result);
                Destroy(this.gameObject);
            }
        }
    }

    public void actionPerformed(int animationType) {
        long result = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond - startTime;
        Debug.Log("" + result + ", type : " + animationType + ", spawner : " + spawner);
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
}
