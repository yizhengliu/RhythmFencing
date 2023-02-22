using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saber : MonoBehaviour
{
    public bool isLeft;
    public Light wrongHitIndicator;
    public LayerMask layer;
    private int frame = 0;
    private bool entered = false;
    private void FixedUpdate()
    {
        if (wrongHitIndicator.color == Color.red) { 
            frame++;
            if (frame > 1) {
                frame = 0;
                wrongHitIndicator.color = Color.white;
            }
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        int performance = 0;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Sword"))
        {
            float saberAngle = Vector3.Angle(transform.up, collision.transform.forward);
            if (saberAngle >= 86 && saberAngle <= 94)
                performance = 3;
            else if (saberAngle >= 75 && saberAngle <= 105)
                performance = 2;
            else
                performance = 1;
            collision.gameObject.SendMessage("Hit", new double[] { performance, saberAngle, isLeft ? 0 : 1 });

        }

        else if (!entered && collision.gameObject.layer == LayerMask.NameToLayer("Body"))
        {
            entered = true;
            collision.gameObject.SendMessage("Hit", new double[] { -1 });
            wrongHitIndicator.color = Color.red;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Body"))
            entered = false;
    }
    private void OnCollisionEnter(Collision collision)
    {/*
        Debug.Log("Collision detected");
        int performance = 0;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Sword"))
        {
            source.PlayOneShot(hitAudio);
            float saberAngle = Vector3.Angle(transform.up, collision.transform.forward);
            if (saberAngle >= 86 && saberAngle <= 94)
                performance = 3;
            else if (saberAngle >= 75 && saberAngle <= 105)
                performance = 2;
            else
                performance = 1;
            //if (UserPref.GAME_MODE)
            // Destroy(other.gameObject);
            print("angle: " + Vector3.Angle(transform.forward, collision.transform.forward));
            print("sword detected");
            collision.gameObject.SendMessage("Hit", new double[] { performance, saberAngle, isLeft ? 0 : 1});

            vibration(100, 1, 255);
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Body"))
        {
            collision.gameObject.SendMessage("Hit", new double[] { performance });
            wrongHitIndicator.color = Color.red;
        }
        */
    }
   

    
}