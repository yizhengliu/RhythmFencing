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
    public Text performancesDisplay;
    // Start is called before the first frame update
    void Start() {
        

        scoreDisplay.text = "Your Score: " + UserPref.SCORE;

        maxComboDisplay.text = "Max Combo: " + UserPref.MAX_COMBO;
        performancesDisplay.text = "punished: " + UserPref.PUNISHED +
            "\nmissed: " + UserPref.MISSED +
            "\nnormal: " + UserPref.NORMAL +
            "\ngood: " + UserPref.GOOD +
            "\nperfect: " + UserPref.PERFECT;
        songNameDisplay.text =
        "Song Name: " + UserPref.SONG_FILEPATH.Replace(System.Environment.GetFolderPath(
        System.Environment.SpecialFolder.MyMusic) + "\\", "");
    }

    public void onClickBack() {
        UserPref.HP = 100;
        UserPref.MISSED = 0;
        UserPref.NORMAL = 0;
        UserPref.GOOD = 0;
        UserPref.PERFECT = 0;
        UserPref.MAX_COMBO = 0;
        UserPref.SCORE = 0;
        UserPref.COMBO = 0;
        SceneManager.LoadScene("StartMenu");
    }
}
