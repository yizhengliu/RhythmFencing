using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saber : MonoBehaviour
{
    public GameObject enemyController;
    public LayerMask layer;
    private Vector3 previousPos;
    public LayerMask swordLayer;
    public LayerMask bodyLayer;


    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.blue, 5f);
        Debug.DrawRay(transform.position, transform.up, Color.green, 5f);
        Debug.DrawRay(transform.position, transform.right, Color.red, 5f);
        previousPos = transform.position;
    }
    private void OnTriggerEnter(Collider other) {
        Debug.Log("enterd Ontriggered");
        int performance = 0;
        if (other.gameObject.layer == LayerMask.NameToLayer("Sword"))
        {
            float saberAngle = Vector3.Angle(transform.forward, other.transform.forward);
            if (saberAngle >= 86 && saberAngle <= 94)
                performance = 3;
            else if (saberAngle >= 75 && saberAngle <= 105)
                performance = 2;
            else
                performance = 1;
            if (UserPref.GAME_MODE)
                Destroy(other.gameObject);
            print("angle: " + Vector3.Angle(transform.forward, other.transform.forward));
            print("sword detected");
            other.gameObject.SendMessage("Hit", performance);
        }
        //SendMessage

        //other.gameObject.BroadcastMessage("Hit", performance);
    }
    //if collision occurs
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("enterd onCollision");
        int performance = 0;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Sword"))
        {
            float saberAngle = Vector3.Angle(transform.forward, collision.gameObject.transform.forward);
            if (saberAngle >= 85 && saberAngle <= 115)
                performance = 3;
            else if (saberAngle >= 80 && saberAngle <= 130)
                performance = 2;
            else
                performance = 1;
            //if (UserPref.GAME_MODE)
                //Destroy(collision.gameObject.gameObject);
            print("angle: " + Vector3.Angle(transform.forward, collision.gameObject.transform.forward));
            print("sword detected");
            collision.gameObject.gameObject.SendMessage("Hit", performance);
        }

    }
}