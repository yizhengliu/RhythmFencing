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
    //add listener for the droplist
    void Start(){
        songFileNameDropDown = GetComponent<Dropdown>();
        addOptions(songFileNameDropDown);
        songFileNameDropDown.onValueChanged.AddListener(delegate {
                onValueChanged(songFileNameDropDown);
            });
    }

    //put all names of the clips from music file from user OS inside the dropdown
    private void addOptions(Dropdown dropdown) {
        
        Debug.Log(Application.persistentDataPath);
        string audioPath;
        audioPath = "/sdcard/Music";
        string[] allowedFileTypes = new string[] { ".mp3", ".ogg", ".wav", ".aiff", ".aif" };
        List<string> fileNames = new List<string>();
        int defaultOption = 0;
#if UNITY_EDITOR
        audioPath = System.Environment.GetFolderPath(
        System.Environment.SpecialFolder.MyMusic);
        filteredAudioFilePaths = Directory
                .GetFiles(audioPath, "*.*")
                .Where(file => allowedFileTypes.Any(file.ToLower().EndsWith))
                .ToList();
        if(filteredAudioFilePaths == null || filteredAudioFilePaths.Count == 0)
            return;
        foreach (string s in filteredAudioFilePaths)
        {
            fileNames.Add(s.Replace(audioPath + "\\", ""));
        }
        songFileNameDropDown.AddOptions(fileNames);
        Debug.Log(filteredAudioFilePaths.Count);
        foreach (string s in filteredAudioFilePaths)
        {
            Debug.Log(s);
        };
        if (UserPref.SONG_FILEPATH != null)
            for (int i = 0; i < filteredAudioFilePaths.Count; i++)
                if (filteredAudioFilePaths[i] == UserPref.SONG_FILEPATH)
                    defaultOption = i;
        UserPref.SONG_FILEPATH = filteredAudioFilePaths[dropdown.value];
        return;
#endif
        filteredAudioFilePaths = Directory
                .GetFiles(audioPath, "*.*")
                .Where(file => allowedFileTypes.Any(file.ToLower().EndsWith))
                .ToList();
        if (filteredAudioFilePaths == null || filteredAudioFilePaths.Count == 0)
            return;
        foreach (string s in filteredAudioFilePaths)
        {
            fileNames.Add(s.Replace(audioPath + "/", ""));
        }
        songFileNameDropDown.AddOptions(fileNames);
        Debug.Log(filteredAudioFilePaths.Count);
        foreach (string s in filteredAudioFilePaths)
        {
            Debug.Log(s);
        }
        if (UserPref.SONG_FILEPATH != null)
            for (int i = 0; i < filteredAudioFilePaths.Count; i++)
                if (filteredAudioFilePaths[i] == UserPref.SONG_FILEPATH)
                    defaultOption = i;
        songFileNameDropDown.value = defaultOption;
        UserPref.SONG_FILEPATH = filteredAudioFilePaths[dropdown.value];
    }

    //when user selected a option from dropdown list
    private void onValueChanged(Dropdown dropdown){
        UserPref.SONG_FILEPATH = filteredAudioFilePaths[dropdown.value];
        Debug.Log("You have selected song: " + UserPref.SONG_FILEPATH);
    }
}
