using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedSpawner : MonoBehaviour
{
    public GameObject Enemy;
    public Transform[] SpawnPoints;
    public float beat;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (timer > beat)
        {
            GameObject newEnemy = Instantiate(Enemy, SpawnPoints[Random.Range(0, 4)]);
            timer -= beat;
        }

        timer += Time.deltaTime;
    }
}
