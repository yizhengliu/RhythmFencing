using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
public class TestController : MonoBehaviour
{
    //UI components
    public Text helper;
    public Canvas loadingCanvas;
    public Text loadingProgress;
    public AudioImporter importer;
    private AudioSource currentAudio;
    private Text delay;
    public Button beatDetectionButton;
    public Button userDetectionButton;
    private Image beatDetectionImage;
    private Image playerImage;


    private int counter = 0;
    private bool loaded = false;
    private float localTimer = 0;
    private float userTimer = 0;
    private bool keyPressed = false;
    private float lastBeatTime = -1;
    private BeatDetectionModel.Point[] beats;
    private List<float> record = new List<float>();

    public void onClickButton()
    {
        if (helper.enabled)
            helper.enabled = false;
        if (counter == beats.Length)
        {
            saveUserBeats();
            SceneManager.LoadScene("StartMenu");
        }
        //beatDetectionButton.text;
        if (lastBeatTime == -1 && currentAudio.isPlaying)
        {
            delay.text = "Delay: " + (currentAudio.time - beats[0].timeInSong);
            record.Add(currentAudio.time);
        }
        if (lastBeatTime != -1 && currentAudio.isPlaying)
        {
            delay.text = "Delay: " + (currentAudio.time - lastBeatTime) + "s";
            Debug.Log(currentAudio.time);
            record.Add(currentAudio.time);
            playerImage.color = Color.green;
            userTimer = 0;
        }

        
    }

    private void Awake() {
        UserPref.TOLERANCE = 0.5f;
        currentAudio = GetComponent<AudioSource>();
        importer.Loaded += OnLoaded;
        importer.Import(UserPref.SONG_FILEPATH);

        delay = GetComponentInChildren<Text>();
        beatDetectionImage = beatDetectionButton.GetComponent<Image>();
        playerImage = userDetectionButton.GetComponent<Image>();
    }

    private void OnLoaded(AudioClip clip) { currentAudio.clip = clip; }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.T) && loaded)
            keyPressed = true;
    }

    private void FixedUpdate() {
        if (!importer.isDone)
            loadingProgress.text = "Loading..." + (Mathf.Round(importer.progress * 1000) / 10) + "%";
        if (keyPressed) {
            keyPressed = false;
            onClickButton();
        }
        if (!loaded && importer.isDone) {
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
        if (beatDetectionImage.color.r != 1f && localTimer > 0.05f) {
            localTimer -= 0.05f;
            beatDetectionImage.color = new Color(beatDetectionImage.color.r + 0.1f,
                beatDetectionImage.color.g + 0.1f,
                beatDetectionImage.color.b,
                beatDetectionImage.color.a);
        }
        if (playerImage.color.r != 1f && userTimer > 0.05f) {
            userTimer -= 0.05f;
            playerImage.color = new Color(playerImage.color.r + 0.1f,
                playerImage.color.g,
                playerImage.color.b + 0.1f,
                playerImage.color.a);
        }
    }

    private void changeColor() {
        if (counter < beats.Length && currentAudio.time > beats[counter].timeInSong) {
            beatDetectionImage.color = Color.blue;
            localTimer = 0;
            lastBeatTime = currentAudio.time;
            counter++;
            return;
        }
        if (counter == beats.Length)
            delay.text = "Song finished, press to go back to main menu";
    }


    private void findBeats() {
        List<BeatDetectionModel.Point> song = BeatDetectionModel.initializeLineOfTheAudio(currentAudio);
        BeatDetectionModel.simplifyLine(ref song);
        BeatDetectionModel.findUpBeat(ref song);
        BeatDetectionModel.writeResult(currentAudio, song);
        beats = song.Where(x => x.isBeat == true).ToArray();
    }

    private void saveUserBeats() {
        string path = "Assets/Result/UserBeatDetection.txt";
        StreamWriter sw = new StreamWriter(path, true);
        foreach (float f in record) {
            sw.WriteLine(f);
        }
        sw.Close();
        UnityEditor.AssetDatabase.ImportAsset(path);
    }
}
