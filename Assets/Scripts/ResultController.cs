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
        "Song Name: " + UserPref.CLIP_SELECTED.name;
    }

    public void onClickBack() {
        UserPref.reset();
        SceneManager.LoadScene("StartMenu");
    }
}
