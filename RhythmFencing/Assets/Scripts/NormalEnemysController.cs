using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NormalEnemysController : MonoBehaviour
{
    public Text score;
    public Text combo;
    public Canvas loadingCanvas;
    public Text loadingProgress;
    public AudioImporter importer;
    private AudioSource currentAudio;
    public GameObject[] enemies;
    private class Point
    {
        public int index = -1;
        public float timeInSong = -1;
        public int startFromSample = -1;
        public int finishAtSample = -1;
        public float energy = 0;
        public bool isSimplified = false;
        public bool isBeat = false;
        public Vector2 position;
        public string printPoint(bool head)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (head == true)
            {
                sb.Append("\n\t");
                sb.Append("index".PadRight(10));
                sb.Append("timeInSong".PadRight(20));
                sb.Append("startsample".PadRight(20));
                sb.Append("endsample".PadRight(20));
                sb.Append("energy".PadRight(20));

                sb.Append("\n\t");
                sb.Append("---------".PadRight(10));
                sb.Append("-------------------".PadRight(20));
                sb.Append("-------------------".PadRight(20));
                sb.Append("-------------------".PadRight(20));
                sb.Append("-------------------".PadRight(20));
            }
            else
            {
                sb.Append("\t");
                sb.Append(index.ToString().PadRight(10));
                sb.Append(timeInSong.ToString().PadRight(20));
                sb.Append(startFromSample.ToString().PadRight(20));
                sb.Append(finishAtSample.ToString().PadRight(20));
                sb.Append(energy.ToString().PadRight(20));
            }
            return sb.ToString();
        }
    }
    private struct Song
    {
        public List<Point> pointsInSong;
    }
    private struct Beat
    {
        public float timing;
        public int behaviour;
    }
    private Beat[] beats;
    private int counter = 0;
    private Song currentSong;
    private float timer = 0;
    private bool loaded = false;


    private void Awake()
    {
        currentAudio = GetComponent<AudioSource>();
        importer.Loaded += OnLoaded;
        importer.Import(UserPref.SONG_FILEPATH);
        UserPref.Enemy[] es = new UserPref.Enemy[5];
        for (int i = 0; i < es.Length; i++) {
            UserPref.Enemy e = new UserPref.Enemy();
            e.index = i;
            e.isActive = false;
            es[i] = e;
        }
        UserPref.ENEMIES = es;
    }

    private void FixedUpdate()
    {
        if (!importer.isDone)
        {
            loadingProgress.text = "Loading..." + Mathf.Round(importer.progress * 100) + "%";
        }
        if (importer.isDone && !loaded)
        {
            Destroy(loadingCanvas);
            initializeSong();
            toSimpleLine();
            findUpBeat();
            writeResult();
            addToBeats();
            loaded = true;
            score.enabled = true;
        }
        if (loaded)
        {
            timer += Time.deltaTime;
            if (!currentAudio.isPlaying)
            {
                //time
                if (timer > 1.135f)
                    currentAudio.Play();
            }
            SendMessages();
        }
    }
    private void OnLoaded(AudioClip clip)
    {
        currentAudio.clip = clip;
    }

    private void addToBeats()
    {
        Point[] upBeats = currentSong.pointsInSong.Where(x => x.isBeat == true).ToArray();
        beats = new Beat[upBeats.Length];
        //type 0 another slash: 1135
        //type 2 normal slash: 584
        for (int i = 0; i < beats.Length; i++)
        {
            beats[i].behaviour = Random.Range(0, 2);
            //beats[i].behaviour = 0;
            switch (beats[i].behaviour)
            {
                case 0:
                    beats[i].timing = upBeats[i].timeInSong;
                    break;
                case 1:
                    beats[i].timing = upBeats[i].timeInSong + 0.551f;
                    break;
            }

        }
    }

    private void findUpBeat()
    {
        Point[] potentialBeats = currentSong.pointsInSong.Where(x => x.isSimplified == true).ToArray();
        for (int i = 0; i < potentialBeats.Length; i++)
        {
            if (i == 0)
            {
                if (potentialBeats[i].energy > potentialBeats[i + 1].energy)
                    currentSong.pointsInSong[potentialBeats[i].index].isBeat = true;
            }
            else if (i == potentialBeats.Length - 1)
            {
                if (potentialBeats[i].energy > potentialBeats[i - 1].energy)
                    currentSong.pointsInSong[potentialBeats[i].index].isBeat = true;
            }
            else
            {
                if (potentialBeats[i].energy > potentialBeats[i - 1].energy &&
                    potentialBeats[i].energy > potentialBeats[i + 1].energy)
                    currentSong.pointsInSong[potentialBeats[i].index].isBeat = true;
            }
        }
    }

    private void toSimpleLine()
    {
        List<int> keep = new List<int>();
        /*
        LineUtility.Simplify(
            currentSong.pointsInSong.Select(x => x.position).ToList(),
            UserPref.TOLERANCE,
            keep);
        */
        List<Point> tokeep = new List<Point>();
        RamerDouglasPeucker(currentSong.pointsInSong, UserPref.TOLERANCE, ref tokeep);
        keep.AddRange(tokeep.Select(x => x.index));
        for (int i = 0; i < keep.Count; i++)
        {
            int indexToKeep = keep[i];
            currentSong.pointsInSong[indexToKeep].isSimplified = true;
        }
    }

    private void setupManually()
    {
        TextAsset textAsset = Resources.Load("Labels 1") as TextAsset;

        string[] textInfo = textAsset.text.Split('\n');
        //timing = new float[textInfo.Length];

        //for (int i = 0; i < textInfo.Length; i++)
        //timing[i] = float.Parse(textInfo[i]);
    }

    private void SendMessages()
    {
        if (counter < beats.Length && timer > beats[counter].timing)
        {
            
            //send messages
            //possible indexs
            List<int> possible = UserPref.ENEMIES.Where(e => e.isActive == false).Select(e => e.index).ToList();
            
            //if possible
            if (possible.Count > 0) {
                int ran = Random.Range(0, possible.Count);
                enemies[possible[ran]].SendMessage("setBehaviour", beats[counter].behaviour);
                enemies[possible[ran]].SendMessage("startAction", possible[ran]);
                enemies[possible[ran]].SendMessage("counterIndex", counter);
                UserPref.ENEMIES[possible[ran]].isActive = true;
            }
            counter++;
        }
    }

    private void writeResult()
    {
        string path = "Assets/Result/Debug.txt";
        StreamWriter sw = new StreamWriter(path, false);

        sw.WriteLine(string.Format("\tsamples = {0}", currentAudio.clip.samples));
        sw.WriteLine(string.Format("\tchannels = {0}", currentAudio.clip.channels));
        sw.WriteLine(string.Format("\ttotalsamples = {0}", currentAudio.clip.samples * currentAudio.clip.channels));
        sw.WriteLine(string.Format("\tsamples per point = {0}", UserPref.SAMPLES_PER_POINT));
        sw.WriteLine(string.Format("\tmeterScale = {0}", UserPref.SCALE));
        sw.WriteLine(string.Format("\ttolerance = {0}", UserPref.TOLERANCE));
        sw.WriteLine(string.Format("\tSongLength = {0}", currentAudio.clip.length));
        sw.WriteLine(string.Format("\tBPM detected = {0}",
            (currentSong.pointsInSong.Where(x => x.isBeat == true).ToArray().Length)
            / (currentAudio.clip.length / 60f)));

        sw.WriteLine("\nUpBeats:({0})", currentSong.pointsInSong.Where(x => x.isBeat == true).ToList().Count);
        sw.WriteLine(currentSong.pointsInSong[0].printPoint(true));
        foreach (Point p in currentSong.pointsInSong.Where(x => x.isBeat == true).ToList())
        {
            sw.WriteLine("\t" + p.printPoint(false));
        }

        sw.WriteLine("\nBeats:({0})", currentSong.pointsInSong.Where(x => x.isSimplified == true).ToList().Count);
        sw.WriteLine(currentSong.pointsInSong[0].printPoint(true));
        foreach (Point p in currentSong.pointsInSong.Where(x => x.isSimplified == true).ToList())
        {
            sw.WriteLine("\t" + p.printPoint(false));
        }



        sw.WriteLine(string.Format("\nPoints:{0}", currentSong.pointsInSong.Count));
        sw.WriteLine(currentSong.pointsInSong[0].printPoint(true));
        foreach (Point p in currentSong.pointsInSong)
        {
            sw.WriteLine(p.printPoint(false));
        }

        sw.Close();
        UnityEditor.AssetDatabase.ImportAsset(path);
    }

    private void initializeSong()
    {
        //-------------------------manually pop beat information
        //setupManually();
        //------------------------------------------------------
        //automatically beat detection
        float[] samples = new float[currentAudio.clip.samples * currentAudio.clip.channels];
        //get the data into the samples array from start point 0
        currentAudio.clip.GetData(samples, 0);
        currentSong.pointsInSong = new List<Point>();
        //make sure pre set the capacity of the array so that system dont need to allocate 
        //memory dynamically which could cause the speed goes down
        currentSong.pointsInSong.Capacity = (int)(samples.Length / (float)UserPref.SAMPLES_PER_POINT + 1);
        int indexCounter = 0;
        for (int i = 0; i < samples.Length; i += UserPref.SAMPLES_PER_POINT)
        {
            Point point = new Point();
            point.index = indexCounter++;
            point.startFromSample = i;
            //if i + 1023 <= samples.length - 1, means it would get out of the bound
            if (i + UserPref.SAMPLES_PER_POINT <= samples.Length)
                point.finishAtSample = i + UserPref.SAMPLES_PER_POINT - 1;
            else
                point.finishAtSample = samples.Length - 1;
            //frequency indicates how many samples per second actually in the song,
            //the equation in the bracket cauculates 
            point.timeInSong = (point.startFromSample /
                (float)currentAudio.clip.channels) / (float)currentAudio.clip.frequency;
            float energySum = 0;
            for (int j = point.startFromSample; j <= point.finishAtSample; j += currentAudio.clip.channels)
            {
                int temp = currentAudio.clip.channels - 1;
                float sum = 0;
                while (temp >= 0)
                {
                    //if we got two channels then plus them 
                    sum += samples[j + temp];
                    temp--;
                }
                energySum += sum / (float)currentAudio.clip.channels;
            }
            point.energy = Mathf.Abs((energySum / UserPref.SAMPLES_PER_POINT) * UserPref.SCALE);
            point.position = new Vector2(point.timeInSong, point.energy);
            currentSong.pointsInSong.Add(point);
        }
    }

    private void RamerDouglasPeucker(List<Point> pointList, float epsilon, ref List<Point> output) {

        if (pointList == null || pointList.Count < 2)
            throw new ArgumentOutOfRangeException("Not enough points to simplify");
        double dmax = 0.0;
        int index = 0;
        int end = pointList.Count - 1;
        for (int i = 1; i < end; i++) {
            double d = PerpendicularDistance(pointList[i], pointList[0], pointList[end]);
            if (d > dmax) {
                index = i;
                dmax = d;
            }
        }

        if (dmax > epsilon)
        {
            List<Point> recResults1 = new List<Point>();
            List<Point> recResults2 = new List<Point>();
            List<Point> firstLine = pointList.Take(index + 1).ToList();
            List<Point> lastLine = pointList.Skip(index).ToList();
            RamerDouglasPeucker(firstLine, epsilon, ref recResults1);
            RamerDouglasPeucker(lastLine, epsilon, ref recResults2);
            output.AddRange(recResults1.Take(recResults1.Count - 1));
            output.AddRange(recResults2);
            if (output.Count < 2) throw new Exception("Probelm assembling output");
        }
        else {
            output.Clear();
            output.Add(pointList[0]);
            output.Add(pointList[pointList.Count - 1]);
        }
    }

    private double PerpendicularDistance(Point pt, Point lineStart, Point lineEnd) {
        double dx = lineEnd.timeInSong - lineStart.timeInSong;
        double dy = lineEnd.energy - lineStart.energy;

        double mag = Mathf.Sqrt((float)(dx * dx + dy * dy));
        if (mag > 0.0) {
            dx /= mag;
            dy /= mag;
        }
        double pvx = pt.timeInSong - lineStart.timeInSong;
        double pvy = pt.energy - lineStart.energy;

        double pvdot = dx * pvx + dy * pvy;

        double ax = pvx - pvdot * dx;
        double ay = pvy - pvdot * dy;

        return Mathf.Sqrt((float)(ax * ax + ay * ay));
    }
    public void Hit(int[] info)
    {
        
        int performance = info[0];
        int index = info[1];
        if (index == beats.Length)
        {
            SceneManager.LoadScene("GameOver");
            saveUserPerformance();
        }
        if (performance == 0 || performance == -1)
        {
            UserPref.MAX_COMBO = 0;
            if (UserPref.SCORE >= 100)

                UserPref.SCORE -= 100;
            else
                UserPref.SCORE = 0;
            UserPref.HP -= 10;
        }
        else
        {
            UserPref.MAX_COMBO++;
            UserPref.SCORE += (int)MathF.Round(performance * (1 + (UserPref.MAX_COMBO * UserPref.DIFFICULTY_LEVEL) * 0.3f / MathF.Abs(timer - beats[index].timing)) / 4f);

        }

        Debug.Log("score: " + UserPref.SCORE);
        score.text = "Score: " + UserPref.SCORE;
        combo.text = UserPref.MAX_COMBO + "\nCOMBO";
    }
    public void saveUserPerformance() { 
    
    }
}
