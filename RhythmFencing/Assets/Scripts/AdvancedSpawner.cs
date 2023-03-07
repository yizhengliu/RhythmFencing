using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AdvancedSpawner : MonoBehaviour
{
    public GameObject hitEffect;
    public Light lightIndicator;
    public Image HPBar;
    public Text score;
    public Text combo;
    public Canvas loadingCanvas;
    public Text loadingProgress;
    private AudioSource currentAudio;
    public GameObject Enemy;
    public Transform[] SpawnPoints;
    public Transform playerPos;
    private Transform destination;

    private int indicatorFrameCount = 0;
    private struct Beat {
        public float timing;
        public int behaviour;
    }
    private Beat[] beats;
    private int counter = 0;
    private List<BeatDetectionModel.Point> currentSong;
    private float timer = 0;
    private bool loaded = false;
    private float[] flashing;
    private int lightCounter = 0;
    private struct Performance
    {
        public int index;
        public float delay;
        public float angle;
        public int category;
    }
    private List<Performance> userPerformances = new List<Performance>();
    private AudioClip clip = null;
    private float[] spawnerCooldowns = new float[] { 1.2f,1.2f,1.2f,1.2f};
    private bool[] willFlash;
    private void Awake(){
        difficultySetting();
        currentAudio = GetComponent<AudioSource>();
        //importer.Loaded += OnLoaded;
        //importer.Import(UserPref.SONG_FILEPATH);
        destination = playerPos;
        clip = UserPref.CLIP_SELECTED;
    }

    private void FixedUpdate()
    {
        if (lightIndicator.enabled)
        {
            indicatorFrameCount++;
            if (indicatorFrameCount > 1)
            {
                indicatorFrameCount = 0;
                lightIndicator.enabled = false;
            }
        }
        
        if (!loaded && clip != null)
        {
            currentAudio.clip = clip;
            Destroy(loadingCanvas);
            currentSong = BeatDetectionModel.initializeLineOfTheAudio(currentAudio);
            BeatDetectionModel.simplifyLine(ref currentSong);
            BeatDetectionModel.findUpBeat(ref currentSong);
            BeatDetectionModel.writeResult(currentAudio, currentSong);
            addToBeats();
            loaded = true;
            //or using mannually set up 
            //setupManually();
        }
        if (loaded)
        {
            timer += Time.deltaTime;
            for (int i = 0; i < spawnerCooldowns.Length; i++)
                spawnerCooldowns[i] -= Time.deltaTime;
            if (!currentAudio.isPlaying)
            {
                if (timer >4.535f)
                    currentAudio.Play();
            }
            spawn();
            if (counter == beats.Length && !currentAudio.isPlaying && timer > 4.535f + currentAudio.clip.length + 3f)
            {
                saveUserPerformance();
                SceneManager.LoadScene("GameOver");
            }
        }
    }

    private void addToBeats() {
        BeatDetectionModel.Point[] upBeats = currentSong.Where(x => x.isBeat == true).ToArray();
        beats = new Beat[upBeats.Length];
        flashing = new float[upBeats.Length];
        willFlash = new bool[upBeats.Length];
        for(int i = 0;i<willFlash.Length ;i++)
            willFlash[i] = false;
        //type 0 another slash: 809 4735
        //type 1 normal slash: 585 4513
        for (int i = 0; i < beats.Length; i++)
        {
            beats[i].behaviour = Random.Range(0, 2);
            //beats[i].behaviour = 0;
            switch (beats[i].behaviour)
            {
                case 0:
                    beats[i].timing = upBeats[i].timeInSong;
                    flashing[i] = beats[i].timing + 4.735f;
                    break;
                case 1:
                    beats[i].timing = upBeats[i].timeInSong + 0.222f;
                    flashing[i] = beats[i].timing + 4.513f;
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
        for (int i = 0; i < beats.Length; i++)
        {
            beats[i].behaviour = Random.Range(0, 2);
            switch (beats[i].behaviour)
            {
                case 0:
                    beats[i].timing = timing[i]; 
                    flashing[i] = beats[i].timing + 4.735f;
                    break;
                case 1:
                    beats[i].timing = timing[i] + 0.222f;
                    flashing[i] = beats[i].timing + 4.513f;
                    break;
            }
        }
    }
   
    private void spawn() {
        if (lightCounter < flashing.Length && timer > flashing[lightCounter]) {
            if (willFlash[lightCounter])
                lightIndicator.enabled = true;
            lightCounter++;
        }
        if (counter < beats.Length && timer > beats[counter].timing)
        {
            List<int> possible = new List<int>();
            for (int i = 0; i < spawnerCooldowns.Length; i++)
                if (spawnerCooldowns[i] <= 0)
                    possible.Add(i);
            //if there is a possible pos
            if (possible.Count > 0) {
                //choose an item
                int ranItem = Random.Range(0, possible.Count);
                //get its index
                int ran = possible[ranItem];
                GameObject newEnemy = Instantiate(Enemy, SpawnPoints[ran]);
                newEnemy.SendMessage("setBehaviour", beats[counter].behaviour);
                newEnemy.SendMessage("setDestination", destination);
                newEnemy.SendMessage("setSpawner", ran);
                newEnemy.SendMessage("setCounter", counter);
                newEnemy.SendMessage("setController", this.gameObject);
                spawnerCooldowns[possible[ranItem]] = 1.2f;
                willFlash[counter] = true;
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

    public void Hit(double[] info)
    {
        int performance = (int)info[0];
        int index = (int)info[1];
        float hitAngle = -1;

        if (info.Length == 3)
            hitAngle = (float)info[2];
        //missed
        if (performance <= 0)
        {
            UserPref.COMBO = 0;
            if (UserPref.SCORE >= 100)

                UserPref.SCORE -= 100;
            else
                UserPref.SCORE = 0;
            UserPref.HP -= 10;
            HPBar.fillAmount = UserPref.HP / 100f;

            Performance np = new Performance();
            np.index = index;
            np.delay = -1;
            np.angle = -1;
            np.category = performance;
            userPerformances.Add(np);
            if (performance == 0)
                UserPref.MISSED++;
            else if (performance == -1)
                UserPref.PUNISHED++;
            if (UserPref.HP == 0)
            {
                saveUserPerformance();
                SceneManager.LoadScene("GameOver");
            }
            
        }
        else
        {
            //hit
            UserPref.COMBO++;
            if (UserPref.COMBO > UserPref.MAX_COMBO)
                UserPref.MAX_COMBO = UserPref.COMBO;
            if (currentAudio.time - beats[index].timing < 0.0001)
                UserPref.SCORE += (int)MathF.Round(performance * (1 + (UserPref.COMBO * (UserPref.DIFFICULTY_LEVEL + 1)) * 0.3f / 0.0001f) / 4f);
            else
                UserPref.SCORE += (int)MathF.Round(performance * (1 + (UserPref.COMBO * (UserPref.DIFFICULTY_LEVEL + 1)) * 0.3f / MathF.Abs(currentAudio.time - beats[index].timing)) / 4f);
            Performance np = new Performance();
            np.index = index;
            np.delay = currentAudio.time - beats[index].timing;
            np.angle = hitAngle;
            np.category = performance;
            userPerformances.Add(np);
        }

        Debug.Log("score: " + UserPref.SCORE);
        score.text = "Score: " + UserPref.SCORE;
        string addition = "";
        combo.color = Color.white;
        if (performance == 1)
        {
            UserPref.NORMAL++;
            addition = "NORMAL";
            combo.color = Color.cyan;
        }
        else if (performance == 2)
        {
            UserPref.GOOD++;
            addition = "GOOD";
            combo.color = Color.green;
        }
        else if (performance == 3)
        {
            UserPref.PERFECT++;
            combo.color = Color.yellow;
            addition = "PERFECT";
        }
        combo.text = UserPref.COMBO + "\nCOMBO\n" + addition;
    }
    public void saveUserPerformance()
    {
        string path = "/sdcard/Download/UserPerformance.txt";
#if UNITY_EDITOR
        path = "Assets/Result/UserPerformance.txt";
#endif
        
        StreamWriter sw = new StreamWriter(path, true);
        sw.WriteLine(string.Format("\tSongName = {0}", UserPref.CLIP_SELECTED.name));
        sw.WriteLine(string.Format("\tMaxCombos = {0}", UserPref.MAX_COMBO));
        sw.WriteLine(string.Format("\tScore = {0}", UserPref.SCORE));
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("\n\t");
        sb.Append("index".PadRight(10));
        sb.Append("delay".PadRight(20));
        sb.Append("angle".PadRight(20));
        sb.Append("category".PadRight(20));

        sb.Append("\n\t");
        sb.Append("---------".PadRight(10));
        sb.Append("-------------------".PadRight(20));
        sb.Append("-------------------".PadRight(20));
        sb.Append("-------------------".PadRight(20));
        foreach (Performance p in userPerformances)
        {
            sb = new System.Text.StringBuilder();
            sb.Append(p.index.ToString().PadRight(10));
            sb.Append(p.delay.ToString().PadRight(20));
            sb.Append(p.angle.ToString().PadRight(20));
            sb.Append(p.category.ToString().PadRight(20));
            sw.WriteLine("\t" + sb.ToString());
        }
        sw.Close();
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
    }

    private void playHitEffect(int behaviour) {
        hitEffect.SendMessage("playHitEffect", behaviour);
    }
}
