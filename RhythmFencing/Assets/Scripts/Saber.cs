using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saber : MonoBehaviour
{
    public bool isLeft;
    public Light wrongHitIndicator;
    public LayerMask layer;
    public AudioClip hitAudio;
    private AudioSource source;
    private int frame = 0;
    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }
    private void FixedUpdate()
    {
        //Debug.DrawRay(transform.position, transform.forward, Color.blue, 1f);
        //should use right to detect collision uusing ray cast
        //Debug.DrawRay(transform.position, transform.right, Color.red, 1f);
        //should use green to detect angle
        //Debug.DrawRay(transform.position, transform.up, Color.green, 1f);
        if (wrongHitIndicator.color == Color.red) { 
            frame++;
            if (frame > 1) {
                frame = 0;
                wrongHitIndicator.color = Color.white;
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
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
            collision.gameObject.SendMessage("Hit", new double[] { performance, saberAngle });

            vibration(40, 1, 255);
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Body"))
        {
            collision.gameObject.SendMessage("Hit", new double[] { performance });
            wrongHitIndicator.color = Color.red;
        }
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
            hapticsClip.WriteSample((byte)strength);
        if (isLeft)
            OVRHaptics.LeftChannel.Preempt(hapticsClip);
        else
            OVRHaptics.RightChannel.Preempt(hapticsClip);
    }
}