using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuController : MonoBehaviour
{
    public void OnclickClassic() {
        if (UserPref.SONG_FILEPATH != null)
            SceneManager.LoadScene("GameSceneClassic");
    }

    public void OnclickNormal(){
        if (UserPref.SONG_FILEPATH != null)
            SceneManager.LoadScene("GameSceneNormal");
    }

    public void OnclickTest()
    {
        SceneManager.LoadScene("Test");
    }
}
