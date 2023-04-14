using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectManager : MonoBehaviour
{
    public AudioClip[] audioClips;
    private AudioSource m_Source;

    private void Awake()
    {
        m_Source = GetComponent<AudioSource>();
    }
    //play fencing sound effect based on enemy's behaviour
    public void playHitEffect(int behaviour) {
        m_Source.PlayOneShot(audioClips[behaviour]);
    }
}
