using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedSpawner : MonoBehaviour
{
    private AudioSource m_MyAudioSource;
    public GameObject Enemy;
    public Transform[] SpawnPoints;
    public float beat;
    public int enemiesToSpawn = 0;
    private float[] timing;
    private int counter = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_MyAudioSource = GetComponent<AudioSource>();
        TextAsset textAsset = Resources.Load("Labels 1") as TextAsset;

        string[] textInfo = textAsset.text.Split('\n');
        timing = new float[textInfo.Length];

        for (int i = 0; i < textInfo.Length; i++)
            timing[i] = float.Parse(textInfo[i]);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_MyAudioSource.time > timing[counter] && counter < timing.Length)
        {
            counter++;
            GameObject newEnemy = Instantiate(Enemy, SpawnPoints[Random.Range(0, 4)]);
        }
    }
}
