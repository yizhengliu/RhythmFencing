using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
public class TestController : MonoBehaviour
{
    public Canvas loadingCanvas;
    public Text loadingProgress;
    private Point[] beats;
    //how many samples of the sone in each point
    public int SAMPLES_PER_POINT = 1024;
    //scale factor to make the value big enough to 
    //make a easy line more accurate
    public float SCALE = 300;
    //how simple will the line after analysis
    public float TOLERANCE = 2;
    public AudioImporter importer;
    private AudioSource currentAudio;
    private class Point {
        public int index = -1;
        public float timeInSong = -1;
        public int startFromSample = -1;
        public int finishAtSample = -1;
        public float energy = 0;
        public bool isSimplified = false;
        public bool isBeat = false;
        public Vector2 position;
    }
    private List<Point> songPoints;
    private int counter = 0;
    private bool loaded = false;
    private float localTimer = 0;
    private float userTimer = 0;

    private bool keyPressed = false;

    private Text delay;
    public Button beatDetectionButton;
    public Button userDetectionButton;
    private float lastBeatTime;
    private Image beatDetectionImage;
    private Image playerImage;
    private List<float> record = new List<float>();
    private void Awake()
    {
        currentAudio = GetComponent<AudioSource>();
        importer.Loaded += OnLoaded;
        importer.Import(UserPref.SONG_FILEPATH);

        delay = GetComponentInChildren<Text>();
        beatDetectionImage = beatDetectionButton.GetComponent<Image>();
        playerImage = userDetectionButton.GetComponent<Image>();

    }
    private void OnLoaded(AudioClip clip) { currentAudio.clip = clip; }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && loaded) {
            keyPressed = true;
        }
    }
    private void FixedUpdate()
    {
        if (!importer.isDone) {
            loadingProgress.text = "Loading..." + Mathf.Round(importer.progress * 100) + "%";
        }
        if (keyPressed)
        {
            keyPressed = false;
            onClickButton();
        }
        if (importer.isDone && !loaded)
        {
            Destroy(loadingCanvas);
            findBeats();
            currentAudio.Play();
            loaded = true;
        }
         if (currentAudio.isPlaying){
            if (beatDetectionImage.color.r != 1f)
                localTimer += Time.deltaTime;
            if (playerImage.color.r != 1f)
                userTimer += Time.deltaTime;
            changeColor();
         }

        if (beatDetectionImage.color.r != 1f && localTimer > 0.05f)
        {
            localTimer -= 0.05f;
            beatDetectionImage.color = new Color(beatDetectionImage.color.r + 0.1f,
                beatDetectionImage.color.g + 0.1f,
                beatDetectionImage.color.b,
                beatDetectionImage.color.a);
        }
        if (playerImage.color.r != 1f && userTimer > 0.05f)
        {
            userTimer -= 0.05f;
            playerImage.color = new Color(playerImage.color.r + 0.1f,
                playerImage.color.g,
                playerImage.color.b + 0.1f,
                playerImage.color.a);
        }
    }
    private void changeColor() {
        if (counter < beats.Length && currentAudio.time > beats[counter].timeInSong)
        {
            beatDetectionImage.color = Color.blue;
            localTimer = 0;
            lastBeatTime = currentAudio.time;
            counter++;
            //StartCoroutine(beatColour());
            return;
        }
        if (counter == beats.Length)
            delay.text = "Song finished, press to go back to main menu";
    }

   /* private IEnumerator beatColour()
    {
        StopAllCoroutines();
        float col = 0f;
        float myTime = 0.1f;
        float step = myTime;
        image.color = Color.blue;
        while(myTime >= 0)
        {
            myTime -= Time.deltaTime;
            col += step * Time.deltaTime;
            image.color = new Color(col, col, 1f);
            yield return new WaitForEndOfFrame();
        }
    }*/

    private void findBeats() {
        float[] samples = new float[currentAudio.clip.samples * currentAudio.clip.channels];
        //get the data into the samples array from start point 0
        currentAudio.clip.GetData(samples, 0);

        songPoints = new List<Point>();
        songPoints.Capacity = (int)(samples.Length / (float)SAMPLES_PER_POINT + 1);
        int indexCounter = 0;
        for (int i = 0; i < samples.Length; i += SAMPLES_PER_POINT)
        {
            Point point = new Point();
            point.index = indexCounter++;
            point.startFromSample = i;
            //if i + 1023 <= samples.length - 1, means it would get out of the bound
            if (i + SAMPLES_PER_POINT <= samples.Length)
                point.finishAtSample = i + SAMPLES_PER_POINT - 1;
            else
                point.finishAtSample = samples.Length - 1;
            //frequency indicates how many samples per second actually in the song,
            //the equation in the bracket cauculates 
            point.timeInSong = (point.startFromSample /
                (float)currentAudio.clip.channels) / (float)currentAudio.clip.frequency;
            float energySum = 0;
            for (int j = point.startFromSample; j <= point.finishAtSample; j += currentAudio.clip.channels)
            {
                int temp = currentAudio.clip.channels - 1;
                float sum = 0;
                while (temp >= 0)
                {
                    //if we got two channels then plus them 
                    sum += samples[j + temp];
                    temp--;
                }
                energySum += sum / (float)currentAudio.clip.channels;
            }
            point.energy = Mathf.Abs((energySum / SAMPLES_PER_POINT) * SCALE);
            point.position = new Vector2(point.timeInSong, point.energy);
            songPoints.Add(point);
        }

        List<int> keep = new List<int>();
        LineUtility.Simplify(
            songPoints.Select(x => x.position).ToList(),
            TOLERANCE,
            keep);
        for (int i = 0; i < keep.Count; i++)
        {
            int indexToKeep = keep[i];
            songPoints[indexToKeep].isSimplified = true;
        }

        Point[] potentialBeats = songPoints.Where(x => x.isSimplified == true).ToArray();
        for (int i = 0; i < potentialBeats.Length; i++)
        {
            if (i == 0)
            {
                if (potentialBeats[i].energy > potentialBeats[i + 1].energy)
                    songPoints[potentialBeats[i].index].isBeat = true;
            }
            else if (i == potentialBeats.Length - 1)
            {
                if (potentialBeats[i].energy > potentialBeats[i - 1].energy)
                    songPoints[potentialBeats[i].index].isBeat = true;
            }
            else
            {
                if (potentialBeats[i].energy > potentialBeats[i - 1].energy &&
                    potentialBeats[i].energy > potentialBeats[i + 1].energy)
                    songPoints[potentialBeats[i].index].isBeat = true;
            }
        }

        beats = songPoints.Where(x => x.isBeat == true).ToArray();
    }

    public void onClickButton() {
        if (counter == beats.Length)
        {
            SceneManager.LoadScene("StartMenu");
            saveUserBeats();
        }
        //beatDetectionButton.text;
        if (currentAudio.isPlaying) {
            delay.text = "Delay: " + (currentAudio.time - lastBeatTime) + "s";
            Debug.Log(currentAudio.time);
            record.Add(currentAudio.time);
            
            //ColorBlock colorBlock = beatDetectionButton.colors;
            //colorBlock.normalColor = Color.white;
            //beatDetectionButton.colors = colorBlock;
            playerImage.color = Color.green;
            userTimer = 0;
            //localTimer = 0;
        }
    }

    private void saveUserBeats() {
        string path = "Assets/Result/UserBeatDetection.txt";
        StreamWriter sw = new StreamWriter(path, false);
        foreach (float f in record) {
            sw.WriteLine(f);
        }
        sw.Close();
        UnityEditor.AssetDatabase.ImportAsset(path);
    }
}
