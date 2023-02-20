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
    public int frame = 0;
    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }
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

        if (other.gameObject.layer == LayerMask.NameToLayer("Body")) {
            other.gameObject.SendMessage("Hit", new double[] { performance});
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
            hapticsClip.WriteSample(i % frequency == 0 ? (byte)strength : (byte)0);
        if (isLeft)
            OVRHaptics.LeftChannel.Preempt(hapticsClip);
        else
            OVRHaptics.RightChannel.Preempt(hapticsClip);
    }
}