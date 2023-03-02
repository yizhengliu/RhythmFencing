using UnityEngine;

public static class UserPref
{
    public static float HP = 100;
    public static int DIFFICULTY_LEVEL = 0;

    //public static string SONG_FILEPATH = "C:\\Users\\edbod\\Music\\September - Cry For You (UK Radio Edit).mp3";
    //public static string SONG_FILEPATH = "C:\\Users\\27571\\Music\\September - Cry For You (UK Radio Edit).mp3";
    //public static string SONG_FILEPATH = null;
    public static long SCORE = 0;
    public static int COMBO = 0;
    public static Enemy[] ENEMIES;
    public class Enemy {
        public int index;
        public bool isActive;
    }
    
    //how many samples of the sone in each point
    public static int SAMPLES_PER_POINT = 1024;
    //scale factor to make the value big enough to 
    //make a easy line more accurate
    public static float SCALE = 300;
    //how simple will the line after analysis
    public static float TOLERANCE = 0.5f;
    public static int MAX_COMBO = 0;
    public static AudioClip CLIP_SELECTED = null;
    public static int MISSED = 0;
    public static int NORMAL = 0;
    public static int GOOD = 0;
    public static int PERFECT = 0;
    public static int PUNISHED = 0;
}
