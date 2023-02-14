using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ResultController : MonoBehaviour
{
    public Text songNameDisplay;
    public TMPro.TextMeshProUGUI scoreDisplay;
    public TMPro.TextMeshProUGUI maxComboDisplay;
    // Start is called before the first frame update
    void Start() {
        songNameDisplay.text =
        "Song Name: " + UserPref.SONG_FILEPATH.Replace(System.Environment.GetFolderPath(
        System.Environment.SpecialFolder.MyMusic) + "\\", "");

        scoreDisplay.text = "Your Score: " + UserPref.SCORE;

        maxComboDisplay.text = "Max Combo: " + UserPref.MAX_COMBO;

    }

    public void onClickBack() {
        UserPref.HP = 100;
        UserPref.GAME_MODE = false;
        UserPref.DIFFICULTY_LEVEL = 0;
        UserPref.SONG_FILEPATH = "C:\\Users\\27571\\Music\\¡¾Music¡¿¥¸¥§¥Ã¥È¥Ú¥ó¥®¥ó£²ºÅ- òXÒô¤Î¤Ê¤¤ÊÀ½ç.mp3";
        //public static string SONG_FILEPATH = null;
        UserPref.SCORE = 0;
        UserPref.MAX_COMBO = 0;
        SceneManager.LoadScene("StartMenu");
    }
}
