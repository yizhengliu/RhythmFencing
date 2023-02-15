using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Transfer : MonoBehaviour
{
    public GameObject parent;

    public void Hit(int Performance) {
        parent.SendMessage("Hit", Performance);
    }
}
