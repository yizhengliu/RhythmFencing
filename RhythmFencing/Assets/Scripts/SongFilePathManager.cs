using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class SongFilePathManager : MonoBehaviour
{
    private Dropdown songFileNameDropDown;
    private List<string> filteredAudioFilePaths;
    // Start is called before the first frame update
    void Start(){
        songFileNameDropDown = GetComponent<Dropdown>();
        addOptions();
        songFileNameDropDown.onValueChanged.AddListener(delegate {
                onValueChanged(songFileNameDropDown);
            });
    }

    private void addOptions() {
        string audioPath = System.Environment.GetFolderPath(
        System.Environment.SpecialFolder.MyMusic);

        string[] allowedFileTypes = new string[] { ".mp3", ".ogg", ".wav", ".aiff", ".aif" };
        
        filteredAudioFilePaths = Directory
                .GetFiles(audioPath, "*.*")
                .Where(file => allowedFileTypes.Any(file.ToLower().EndsWith))
                .ToList();

        List<string> fileNames = new List<string>();
        foreach (string s in filteredAudioFilePaths) {
            fileNames.Add(s.Replace(audioPath + "\\", ""));
        }
        songFileNameDropDown.AddOptions(fileNames);
        Debug.Log(filteredAudioFilePaths.Count);
        foreach (string s in filteredAudioFilePaths)
        {
            Debug.Log(s);
        }
    }

    private void onValueChanged(Dropdown dropdown){
        UserPref.SONG_FILEPATH = filteredAudioFilePaths[dropdown.value];
        Debug.Log("You have selected song: " + UserPref.SONG_FILEPATH);
    }
}
