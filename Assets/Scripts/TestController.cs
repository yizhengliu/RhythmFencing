using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using System;
using UnityEngine.Networking;

public class TestController : MonoBehaviour
{
    //UI components
    public Text helper;
    public Canvas loadingCanvas;
    public Text loadingProgress;
    private AudioSource currentAudio;
    private Text delay;
    public Button beatDetectionButton;
    public Button userDetectionButton;
    private Image beatDetectionImage;
    private Image playerImage;

    //beat counter
    private int counter = 0;
    //whether the audio is analysised
    private bool loaded = false;
    //timer for constantly change color of the button
    private float localTimer = 0;
    private float userTimer = 0;
    private bool keyPressed = false;
    //keep tracking the timing of the last beat
    private float lastBeatTime = -1;
    //analysised beats
    private BeatDetectionModel.Point[] beats;
    //user detection record
    private List<float> record = new List<float>();
    //the audio selected from start menu
    private AudioClip clip = null;
    //run time loading
    private UnityWebRequest uwr;
    public void onClickButton()
    {
        //if first time clicked, remove helper
        if (helper.enabled)
            helper.enabled = false;
        //if song is finished, guide back
        if (counter == beats.Length)
        {
            saveUserBeats();
            SceneManager.LoadScene("StartMenu");
        }
        //beatDetectionButton.text;
        //if it is first time pressed, calculate the difference of timing from
        //the first beat and the click timing
        if (lastBeatTime == -1 && currentAudio.isPlaying)
        {
            delay.text = "Delay: " + (currentAudio.time - beats[0].timeInSong);
            record.Add(currentAudio.time);
        }
        //otherwise calcullate the difference of timing from last beat and current beat.
        if (lastBeatTime != -1 && currentAudio.isPlaying)
        {
            delay.text = "Delay: " + (currentAudio.time - lastBeatTime) + "s";
            Debug.Log(currentAudio.time);
            record.Add(currentAudio.time);
            playerImage.color = Color.green;
            userTimer = 0;
        }

        
    }
    async Task<AudioClip> LoadClip()
    {
        AudioClip clip = null;
        string[] allowedFileTypes = new string[] { ".mp3", ".ogg", ".wav", ".aiff", ".aif" };
        //file.ToLower().EndsWith
        if (UserPref.SONG_FILEPATH.ToLower().EndsWith(allowedFileTypes[0]))
            uwr = UnityWebRequestMultimedia.GetAudioClip(UserPref.SONG_FILEPATH, AudioType.MPEG);
        else if (UserPref.SONG_FILEPATH.ToLower().EndsWith(allowedFileTypes[1]))
            uwr = UnityWebRequestMultimedia.GetAudioClip(UserPref.SONG_FILEPATH, AudioType.OGGVORBIS);
        else if (UserPref.SONG_FILEPATH.ToLower().EndsWith(allowedFileTypes[2]))
            uwr = UnityWebRequestMultimedia.GetAudioClip(UserPref.SONG_FILEPATH, AudioType.WAV);
        else if (UserPref.SONG_FILEPATH.ToLower().EndsWith(allowedFileTypes[3]) || UserPref.SONG_FILEPATH.ToLower().EndsWith(allowedFileTypes[4]))
            uwr = UnityWebRequestMultimedia.GetAudioClip(UserPref.SONG_FILEPATH, AudioType.AIFF);

        uwr.SendWebRequest();

        // wrap tasks in try/catch, otherwise it'll fail silently
        try
        {
            while (!uwr.isDone) await Task.Delay(0);

            if (uwr.isNetworkError || uwr.isHttpError) Debug.Log($"{uwr.error}");
            else
            {
                clip = DownloadHandlerAudioClip.GetContent(uwr);
            }
        }
        catch (Exception err)
        {
            Debug.Log($"{err.Message}, {err.StackTrace}");
        }

        return clip;
    }
    private async void Start()
    {
        clip = await LoadClip();
    }

    //fixed tolerance and initalize attributes
    private void Awake() {
        UserPref.TOLERANCE = 0.5f;
        currentAudio = GetComponent<AudioSource>();
        delay = GetComponentInChildren<Text>();
        beatDetectionImage = beatDetectionButton.GetComponent<Image>();
        playerImage = userDetectionButton.GetComponent<Image>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.T) && loaded)
            keyPressed = true;
    }

    private void FixedUpdate() {
        //run time loading
        if (clip == null)
        {
            loadingProgress.text = "Loading..." + (Mathf.Round(uwr.downloadProgress * 1000) / 10) + "%";
            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Connection error");
                loadingProgress.text = "Connection error\n" + uwr.responseCode;
            }
            else if (uwr.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Protocol error");
                loadingProgress.text = "Protocol error";
            }
            else if (uwr.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log("Data Processing error");
                loadingProgress.text = "Data Processing error";
            }
            
        }
        
        if (keyPressed) {
            keyPressed = false;
            onClickButton();
        }

        //if have not got the beat sheet go back to analysis
        if (!loaded && clip != null) {
            currentAudio.clip = clip;
            Destroy(loadingCanvas);
            findBeats();
            currentAudio.Play();
            loaded = true;
        }
        //if analysis is done
        if (currentAudio.isPlaying)
        {
            //if button color is not white, timer start to record the time
            if (beatDetectionImage.color.r != 1f)
                localTimer += Time.deltaTime;
            if (playerImage.color.r != 1f)
                userTimer += Time.deltaTime;
            //change the color of the button
            changeColor();
            //graduatly change the color back to white
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
        
    }
    //if we have got a beat, we change the color of the button, if all beats are finished, inform the
    //user by render text onto the button
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


    //ask beat detection model for beats informaiton 
    private void findBeats() {
        List<BeatDetectionModel.Point> song = BeatDetectionModel.initializeLineOfTheAudio(currentAudio);
        BeatDetectionModel.simplifyLine(ref song);
        BeatDetectionModel.findUpBeat(ref song);
        BeatDetectionModel.writeResult(currentAudio, song);
        beats = song.Where(x => x.isBeat == true).ToArray();
    }

    //save user detected beats
    private void saveUserBeats() {
        string path =  "/sdcard/Download/UserBeatDetection.txt";
#if UNITY_EDITOR
        path = "Assets/Result/UserBeatDetection.txt";
#endif
        StreamWriter sw = new StreamWriter(path, true);

        sw.WriteLine("Song: " + UserPref.SONG_FILEPATH);
        foreach (float f in record) {
            sw.WriteLine(f);
        }
        sw.Close();
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
    }
}
