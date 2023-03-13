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
    private struct Performance {
        public int index;
        public float delay;
        public float angle;
        public int category;
    }
    private List<Performance> userPerformances = new List<Performance>();
    private AudioClip clip = null;
    private void Awake() {
        currentAudio = GetComponent<AudioSource>();

        //difficulty setting
        difficultySetting();
        //importer.Loaded += OnLoaded;
        //importer.Import(UserPref.SONG_FILEPATH);
        clip = UserPref.CLIP_SELECTED;
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
    
    
    private void FixedUpdate() {
        

        
        if (clip != null && !loaded) {
            currentAudio.clip = clip;
            Destroy(loadingCanvas);
            
            currentSong = BeatDetectionModel.initializeLineOfTheAudio(currentAudio);
            BeatDetectionModel.simplifyLine(ref currentSong);
            BeatDetectionModel.findUpBeat(ref currentSong);
            BeatDetectionModel.writeResult(currentAudio, currentSong);
            addToBeats();
            loaded = true;
            currentSong = null;
            GC.Collect();
            //or using mannually set up 
            //setupManually();
        }
        if (loaded) {
            timer += Time.deltaTime;
            if (!currentAudio.isPlaying)
                //time
                if (timer > 0.804f && timer < 3f)
                    currentAudio.Play();
            SendMessages();
            if (counter == beats.Length && !currentAudio.isPlaying && timer > 0.804f + currentAudio.clip.length + 3f) {
                saveUserPerformance();
                SceneManager.LoadScene("GameOver");
            }
        }
    }

    private void addToBeats() {
        BeatDetectionModel.Point[] upBeats = currentSong.Where(x => x.isBeat == true).ToArray();
        beats = new Beat[upBeats.Length];
        //type 0 another slash: 1135 804
        //type 1 normal slash: 584 577
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
                    beats[i].timing = upBeats[i].timeInSong + 0.227f;
                    break;
            }

        }
    }

    private void setupManually() {
        TextAsset textAsset = Resources.Load("Labels_1") as TextAsset;

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
                    beats[i].timing = timing[i] + 0.227f;
                    break;
            }
        }
    }

    private void SendMessages() {
        if (counter < beats.Length && timer > beats[counter].timing) {
            //send messages
            //possible indexs
            List<int> possible = UserPref.ENEMIES.Where(e => e.isActive == false).Select(e => e.index).ToList();
            /*
            //if possible
            string pe = "";
            
            for (int i = 0; i < possible.Count; i++) {
                pe += possible[i];
                pe += " ";
            }
            Debug.Log("Possible enemy: " + pe);
            */
            if (possible.Count > 0) {
                int ran = Random.Range(0, possible.Count);
                enemies[possible[ran]].SendMessage("setBehaviour", beats[counter].behaviour);
                enemies[possible[ran]].SendMessage("counterIndex", counter);
                enemies[possible[ran]].SendMessage("startAction", possible[ran]);
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
        if (performance <= 0) {
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
            
        } else {
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

        //Debug.Log("score: " + UserPref.SCORE);
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

    public void saveUserPerformance() {
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
        sw.WriteLine(sb.ToString());
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
}
