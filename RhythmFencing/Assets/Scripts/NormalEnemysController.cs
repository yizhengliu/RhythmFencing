using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NormalEnemysController : MonoBehaviour
{
    public Image HPBar;
    public Text score;
    public Text combo;
    public Canvas loadingCanvas;
    public Text loadingProgress;
    public AudioImporter importer;
    public GameObject[] enemies;

    private struct Beat {
        public float timing;
        public int behaviour;
    }
    private AudioSource currentAudio;
    private Beat[] beats;
    private int counter = 0;
    private List<BeatDetectionModel.Point> currentSong;
    private float timer = 0;
    private bool loaded = false;


    private void Awake() {
        currentAudio = GetComponent<AudioSource>();

        //difficulty setting
        difficultySetting();
        //import music
        importer.Loaded += OnLoaded;
        importer.Import(UserPref.SONG_FILEPATH);
        //delete enemies that are not belongs to this level
        for (int i = UserPref.DIFFICULTY_LEVEL + 1; i < enemies.Length; i++) {
            Destroy(enemies[i]);
        }
        //preset enemies
        UserPref.Enemy[] es = new UserPref.Enemy[UserPref.DIFFICULTY_LEVEL + 1];
        for (int i = 0; i < es.Length; i++) {
            UserPref.Enemy e = new UserPref.Enemy();
            e.index = i;
            e.isActive = false;
            es[i] = e;
        }
        UserPref.ENEMIES = es;
    }
    private void Start()
    {

    }
    private void FixedUpdate() {

        HPBar.fillAmount = UserPref.HP / 100;
        if (!importer.isDone)
            loadingProgress.text = "Loading..." + (Mathf.Round(importer.progress * 1000) / 10) + "%";
        if (importer.isDone && !loaded) {
            Destroy(loadingCanvas);
            currentSong = BeatDetectionModel.initializeLineOfTheAudio(currentAudio);
            BeatDetectionModel.simplifyLine(ref currentSong);
            BeatDetectionModel.findUpBeat(ref currentSong);
            BeatDetectionModel.writeResult(currentAudio, currentSong);
            addToBeats();
            loaded = true;
            score.enabled = true;
            //or using mannually set up 
            //setupManually();
        }
        if (loaded) {
            timer += Time.deltaTime;
            if (!currentAudio.isPlaying)
                //time
                if (timer > 1.135f)
                    currentAudio.Play();
            SendMessages();
            if (counter == beats.Length && timer > currentAudio.clip.length + 3f) {
                saveUserPerformance();
                SceneManager.LoadScene("GameOver");
            }
        }
    }
    private void OnLoaded(AudioClip clip) {
        currentAudio.clip = clip;
    }

    private void addToBeats() {
        BeatDetectionModel.Point[] upBeats = currentSong.Where(x => x.isBeat == true).ToArray();
        beats = new Beat[upBeats.Length];
        //type 0 another slash: 1135
        //type 2 normal slash: 584
        for (int i = 0; i < beats.Length; i++)
        {
            beats[i].behaviour = Random.Range(0, 2);
            //beats[i].behaviour = 0;
            switch (beats[i].behaviour)
            {
                case 0:
                    beats[i].timing = upBeats[i].timeInSong;
                    break;
                case 1:
                    beats[i].timing = upBeats[i].timeInSong + 0.551f;
                    break;
            }

        }
    }

    private void setupManually() {
        TextAsset textAsset = Resources.Load("Labels 1") as TextAsset;

        string[] textInfo = textAsset.text.Split('\n');
        float[] timing;
        timing = new float[textInfo.Length];

        for (int i = 0; i < textInfo.Length; i++)
            timing[i] = float.Parse(textInfo[i]);
        beats = new Beat[textInfo.Length];
        for (int i = 0; i < beats.Length; i++) {
            beats[i].behaviour = Random.Range(0, 2);
            switch (beats[i].behaviour) {
                case 0:
                    beats[i].timing = timing[i];
                    break;
                case 1:
                    beats[i].timing = timing[i] + 0.551f;
                    break;
            }
        }
    }

    private void SendMessages() {
        if (counter < beats.Length && timer > beats[counter].timing) {
            //send messages
            //possible indexs
            List<int> possible = UserPref.ENEMIES.Where(e => e.isActive == false).Select(e => e.index).ToList();
            
            //if possible
            if (possible.Count > 0) {
                int ran = Random.Range(0, possible.Count);
                enemies[possible[ran]].SendMessage("setBehaviour", beats[counter].behaviour);
                enemies[possible[ran]].SendMessage("startAction", possible[ran]);
                enemies[possible[ran]].SendMessage("counterIndex", counter);
                UserPref.ENEMIES[possible[ran]].isActive = true;
            }
            counter++;
        }
    }

    private void difficultySetting()
    {
        if (UserPref.DIFFICULTY_LEVEL == 0)
            //easy
            UserPref.TOLERANCE = 1.5f;
        else if (UserPref.DIFFICULTY_LEVEL == 1) //normal
            UserPref.TOLERANCE = 1f;
        else
            //hard
            UserPref.TOLERANCE = 0.5f;

    }

    public void Hit(int[] info)
    {
        HPBar.fillAmount = UserPref.HP / 100;
        int performance = info[0];
        int index = info[1];
        if (index == beats.Length || UserPref.HP == 0)
        {
            saveUserPerformance();
            SceneManager.LoadScene("GameOver");
        }
        if (performance == 0)
        {
            UserPref.MAX_COMBO = 0;
            if (UserPref.SCORE >= 100)

                UserPref.SCORE -= 100;
            else
                UserPref.SCORE = 0;
           // UserPref.HP -= 10;
        }
        else
        {
            UserPref.MAX_COMBO++;
            UserPref.SCORE += (int)MathF.Round(performance * (1 + (UserPref.MAX_COMBO * (UserPref.DIFFICULTY_LEVEL + 1)) * 0.3f / MathF.Abs(currentAudio.time - beats[index].timing)) / 4f);

        }

        Debug.Log("score: " + UserPref.SCORE);
        score.text = "Score: " + UserPref.SCORE;
        combo.text = UserPref.MAX_COMBO + "\nCOMBO";
    }

    public void saveUserPerformance() { 
    
    }
}
