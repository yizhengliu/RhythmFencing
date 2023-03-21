using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saber : MonoBehaviour
{

    public bool isLeft;
    public Light wrongHitIndicator;
    public LayerMask layer;
    private float frame = 0;
    private bool entered = false;
    private void FixedUpdate()
    {
        //Debug.DrawRay(transform.position, transform.forward, Color.blue, 1f);
        //Debug.DrawRay(transform.position, transform.right, Color.red, 1f);
        //Debug.DrawRay(transform.position, transform.up, Color.green, 1f);
        if (wrongHitIndicator.color == Color.red) { 
            frame+= Time.deltaTime;
            if (frame > 0.1f) {
                frame = 0;
                wrongHitIndicator.color = Color.white;
                wrongHitIndicator.enabled = false;
            }
        }
    }
    /*
    private void OnTriggerEnter(Collider collision)
    {
        //print("on trigger");
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
    */
    private void OnCollisionEnter(Collision collision)
    {
        //print("on collision" + collision.gameObject.layer.ToString());
        //the first point of collision
        Vector3 collisionEnterPoint = collision.contacts[0].point;
        //for effect
        
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
            collision.gameObject.SendMessage("passPos", collisionEnterPoint);
           collision.gameObject.SendMessage("Hit", new double[] { performance, saberAngle, isLeft ? 0 : 1 });

        }

        else if (!entered && collision.gameObject.layer == LayerMask.NameToLayer("Body"))
        {
            entered = true;
            collision.gameObject.SendMessage("Hit", new double[] { -1 , isLeft ? 0 : 1});
            wrongHitIndicator.color = Color.red;
            wrongHitIndicator.enabled = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Body"))
            entered = false;
    }
    
}