using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saber : MonoBehaviour
{
    public bool isLeft;
    public Light wrongHitIndicator;
    public LayerMask layer;
    private Vector3 previousPos;
    public AudioClip hitAudio;
    private AudioSource source;
    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        //Debug.DrawRay(transform.position, transform.forward, Color.blue, 5f);
        //Debug.DrawRay(transform.position, transform.up, Color.green, 5f);
        //Debug.DrawRay(transform.position, transform.right, Color.red, 5f);
        previousPos = transform.position;
    }
    private void FixedUpdate()
    {
        if (wrongHitIndicator.enabled)
            wrongHitIndicator.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("enterd Ontriggered");
        int performance = 0;
        if (other.gameObject.layer == LayerMask.NameToLayer("Sword"))
        {
            source.PlayOneShot(hitAudio);
            float saberAngle = Vector3.Angle(transform.forward, other.transform.forward);
            if (saberAngle >= 86 && saberAngle <= 94)
                performance = 3;
            else if (saberAngle >= 75 && saberAngle <= 105)
                performance = 2;
            else
                performance = 1;
            //if (UserPref.GAME_MODE)
               // Destroy(other.gameObject);
            print("angle: " + Vector3.Angle(transform.forward, other.transform.forward));
            print("sword detected");
            other.gameObject.SendMessage("Hit", new double[] {performance, saberAngle});

            vibration();
        }

        //if (other.gameObject.layer == LayerMask.NameToLayer("Body"))
        //{
           // other.gameObject.SendMessage("Hit", new double[] { performance});
            //wrongHitIndicator.enabled = true;
        //}
        //SendMessage

        //other.gameObject.BroadcastMessage("Hit", performance);
    }
   
    private void vibration()
    {
        OVRHapticsClip hapticsClip = new OVRHapticsClip(hitAudio);

        if (isLeft)
            OVRHaptics.LeftChannel.Preempt(hapticsClip);
        else
            OVRHaptics.RightChannel.Preempt(hapticsClip);

    }

    private void vibration(int iteration, int frequency, int strength)
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