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
        addOptions(songFileNameDropDown);
        songFileNameDropDown.onValueChanged.AddListener(delegate {
                onValueChanged(songFileNameDropDown);
            });
    }

    private void addOptions(Dropdown dropdown) {
        Debug.Log(Application.persistentDataPath);
        string audioPath;
        audioPath = "/sdcard/Music";
        string[] allowedFileTypes = new string[] { ".mp3", ".ogg", ".wav", ".aiff", ".aif" };
        List<string> fileNames = new List<string>();
#if UNITY_EDITOR
        audioPath = System.Environment.GetFolderPath(
        System.Environment.SpecialFolder.MyMusic);
        filteredAudioFilePaths = Directory
                .GetFiles(audioPath, "*.*")
                .Where(file => allowedFileTypes.Any(file.ToLower().EndsWith))
                .ToList();
        foreach (string s in filteredAudioFilePaths)
        {
            fileNames.Add(s.Replace(audioPath + "\\", ""));
        }
        songFileNameDropDown.AddOptions(fileNames);
        Debug.Log(filteredAudioFilePaths.Count);
        foreach (string s in filteredAudioFilePaths)
        {
            Debug.Log(s);
        }
        UserPref.SONG_FILEPATH = filteredAudioFilePaths[dropdown.value];
        return;
#endif
        filteredAudioFilePaths = Directory
                .GetFiles(audioPath, "*.*")
                .Where(file => allowedFileTypes.Any(file.ToLower().EndsWith))
                .ToList();
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
        UserPref.SONG_FILEPATH = filteredAudioFilePaths[dropdown.value];
    }

    private void onValueChanged(Dropdown dropdown){
        UserPref.SONG_FILEPATH = filteredAudioFilePaths[dropdown.value];
        Debug.Log("You have selected song: " + UserPref.SONG_FILEPATH);
    }
}
