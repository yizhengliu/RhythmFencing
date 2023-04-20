using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.EventSystems;

public class Transfer : MonoBehaviour
{
    public GameObject parent;

    //tell parent the hit information
    public void Hit(double[] Performance) {
        parent.SendMessage("Hit", Performance);
    }

    //tell parent the position of hitting
    public void passPos(Vector3 cep)
    {
        parent.SendMessage("passPos", cep);
    }
}
