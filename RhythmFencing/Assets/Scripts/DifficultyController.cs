using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyController : MonoBehaviour
{
    private TMPro.TMP_Dropdown difficultyDropDown;

    void Start()
    {
        difficultyDropDown = GetComponent<TMPro.TMP_Dropdown>();
        difficultyDropDown.onValueChanged.AddListener(delegate {
            onValueChanged(difficultyDropDown);
        });

        Debug.Log("You have selected difficulty: " + UserPref.DIFFICULTY_LEVEL);
    }

    private void onValueChanged(TMPro.TMP_Dropdown dropdown) {
        UserPref.DIFFICULTY_LEVEL = dropdown.value;
        Debug.Log("You have selected difficulty: " + UserPref.DIFFICULTY_LEVEL);
    }
}
