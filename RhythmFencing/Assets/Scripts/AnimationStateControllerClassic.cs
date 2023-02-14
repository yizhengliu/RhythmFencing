using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateControllerClassic : MonoBehaviour
{
    private Animator animator;
    private Vector3 stationaryPoint;
    private int behaviour;
    // Start is called before the first frame update
    private long startTime;
    private Transform destination;
    private int spawner;
    void Start()
    {
        animator = GetComponent<Animator>(); 
        startTime = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
    }
    private void FixedUpdate()
    {
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
                        animator.SetBool("StartAttack", true);
                        //transform.rotation = rot;
                        break;
                    case 1:
                        animator.SetBool("RotateSlash", true);
                        //transform.rotation = rot;
                        break;
                    case 2:
                        animator.SetBool("NormalSlash", true);
                        break;
                }

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
