using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class SongFilePathManager : MonoBehaviour
{
    public AudioClip[] audioClips;
    private Dropdown songFileNameDropDown;
    // Start is called before the first frame update
    void Start(){
        songFileNameDropDown = GetComponent<Dropdown>();
        addOptions();
        songFileNameDropDown.onValueChanged.AddListener(delegate {
                onValueChanged();
            });
    }

    private void addOptions() {
        if(audioClips.Length == 0)
            return;
        songFileNameDropDown.AddOptions(audioClips.Select(clip => clip.name).ToList());
        int defaultOption = 0;
        if (UserPref.CLIP_SELECTED != null)
            for (int i = 0; i < audioClips.Length; i++)
                if (audioClips[i].name == UserPref.CLIP_SELECTED.name)
                    defaultOption = i;
        songFileNameDropDown.value = defaultOption;
        UserPref.CLIP_SELECTED = audioClips[songFileNameDropDown.value];
    }

    private void onValueChanged(){
        UserPref.CLIP_SELECTED = audioClips[songFileNameDropDown.value];
        Debug.Log("You have selected song: " + audioClips[songFileNameDropDown.value]);
    }
}
