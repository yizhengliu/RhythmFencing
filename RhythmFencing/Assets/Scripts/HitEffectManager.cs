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
    public void playHitEffect(int behaviour) {
        m_Source.PlayOneShot(audioClips[behaviour]);
    }
}
