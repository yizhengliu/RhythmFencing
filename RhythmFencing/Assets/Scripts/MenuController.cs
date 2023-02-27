using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuController : MonoBehaviour
{
    public void OnclickClassic() {
        if (UserPref.SONG_FILEPATH != null)
            SceneManager.LoadScene("GameSceneClassic");
        Debug.Log(Application.persistentDataPath);
    }

    public void OnclickNormal(){
        if (UserPref.SONG_FILEPATH != null)
            SceneManager.LoadScene("GameSceneNormal");
        Debug.Log(Application.persistentDataPath);
    }

    public void OnclickTest()
    {
        // /storage/emulated/0/Android/data/com.DefaultCompany.RhythmFencing/files
        if (UserPref.SONG_FILEPATH != null)
            SceneManager.LoadScene("Test");
    }
}
