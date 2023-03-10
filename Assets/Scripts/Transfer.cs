using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.EventSystems;

public class Transfer : MonoBehaviour
{
    public GameObject parent;

    public void Hit(double[] Performance) {
        parent.SendMessage("Hit", Performance);
    }

    public void passPos(Vector3 cep)
    {
        parent.SendMessage("passPos", cep);
    }
}
