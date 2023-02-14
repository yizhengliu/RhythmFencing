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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.position + transform.forward, Color.green, 1f);
        previousPos = transform.position;
    }
    private void OnTriggerEnter(Collider other) {
        Debug.Log("enterd");
        int performance = 0;
        RaycastHit hit;
        if (other.gameObject.layer == LayerMask.NameToLayer("Body")) {
            performance = -1;
        }
        if (Physics.Raycast(transform.position, transform.position + transform.forward, out hit, 1, swordLayer))
        {
            print(hit.transform.name);
            float saberAngle = Vector3.Angle(transform.position - previousPos, hit.transform.forward);
            if (saberAngle >= 86 && saberAngle <= 94)
                performance = 3;
            else if (saberAngle >= 75 && saberAngle <= 105)
                performance = 2;
            else
                performance = 1;
            if (UserPref.GAME_MODE)
                Destroy(hit.transform.gameObject);
        }
        //SendMessage
        other.gameObject.BroadcastMessage("Hit", performance);
    }
    //if collision occurs
    private void OnCollisionEnter(Collision collision)
    {
       
        
    }
}