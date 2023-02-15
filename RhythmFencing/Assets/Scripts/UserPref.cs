public static class UserPref
{
    public static int HP = 100;
    public static bool GAME_MODE = false;
    public static int DIFFICULTY_LEVEL = 0;
    public static string SONG_FILEPATH = "C:\\Users\\edbod\\Music\\suzumiya.mp3";
    //public static string SONG_FILEPATH = null;
    public static int SCORE = 0;
    public static int MAX_COMBO = 0;
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
    public static float TOLERANCE = 2f;
}
