using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEngine;
#endif

public class AdvancedSpawner : MonoBehaviour
{
    private AudioSource currentAudio;
    public GameObject Enemy;
    public Transform[] SpawnPoints;
    //how many samples of the sone in each point
    public int SAMPLES_PER_POINT = 1024;
    //scale factor to make the value big enough to 
    //make a easy line more accurate
    public float SCALE;
    //how simple will the line after analysis
    public float TOLERANCE;
    private class Point {
        public int index = -1;
        public float timeInSong = -1;
        public int startFromSample = -1;
        public int finishAtSample = -1;
        public float energy = 0;
        public bool isSimplified = false;
        public bool isBeat = false;
        public Vector2 position;
        public string printPoint(bool head) {
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
    private struct Song {
        public List<Point> pointsInSong;
    }
    private struct Beat {
        public float timing;
        public int behaviour;
    }
    private Beat[] beats;
    private int counter = 0;
    private Song currentSong;
    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        currentAudio = GetComponent<AudioSource>();
        initializeSong();
        toSimpleLine();
        findUpBeat();
        writeResult();
        addToBeats();
        //currentAudio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
        timer += Time.deltaTime;
        if (!currentAudio.isPlaying)
        {
            if (timer > 5.987f)
                currentAudio.Play();
        }
        spawn();
    }

    private void addToBeats() {
        Point[] upBeats = currentSong.pointsInSong.Where(x => x.isBeat == true).ToArray();
        beats = new Beat[upBeats.Length];
        //type 0: 5770
        //type 1: 5987
        //type 2: 5220
        for (int i = 0; i < beats.Length; i++){
            beats[i].behaviour = Random.Range(0, 3);
            switch (beats[i].behaviour) {
                case 0:
                    beats[i].timing = upBeats[i].timeInSong + 0.217f;
                    break;
                case 1:
                    beats[i].timing = upBeats[i].timeInSong;
                    break;
                case 2:
                    beats[i].timing = upBeats[i].timeInSong + 0.767f;
                    break;
            }
            
        }
    }

    private void findUpBeat() {
        Point[] potentialBeats = currentSong.pointsInSong.Where(x => x.isSimplified == true).ToArray();
        for (int i = 0; i < potentialBeats.Length; i++) {
            if (i == 0){
                if (potentialBeats[i].energy > potentialBeats[i + 1].energy)
                    currentSong.pointsInSong[potentialBeats[i].index].isBeat = true;
            }
            else if (i == potentialBeats.Length - 1) {
                if (potentialBeats[i].energy > potentialBeats[i - 1].energy)
                    currentSong.pointsInSong[potentialBeats[i].index].isBeat = true;
            }
            else {
                if (potentialBeats[i].energy > potentialBeats[i - 1].energy &&
                    potentialBeats[i].energy > potentialBeats[i + 1].energy)
                    currentSong.pointsInSong[potentialBeats[i].index].isBeat = true;
            }
        }
    }

    private void toSimpleLine() {
        List<int> keep = new List<int>();
        LineUtility.Simplify(
            currentSong.pointsInSong.Select(x => x.position).ToList(),
            TOLERANCE,
            keep);
        for (int i = 0; i < keep.Count; i++) {
            int indexToKeep = keep[i];
            currentSong.pointsInSong[indexToKeep].isSimplified = true;
        }
    }

    private void setupManually() {
        TextAsset textAsset = Resources.Load("Labels 1") as TextAsset;

        string[] textInfo = textAsset.text.Split('\n');
        //timing = new float[textInfo.Length];

        //for (int i = 0; i < textInfo.Length; i++)
            //timing[i] = float.Parse(textInfo[i]);
    }
   
    private void spawn() {
        if (counter < beats.Length && timer > beats[counter].timing)
        {   
            GameObject newEnemy = Instantiate(Enemy, SpawnPoints[Random.Range(0, 4)]);
            newEnemy.SendMessage("setBehaviour", beats[counter].behaviour);
            counter++;
        }
    }

    private void writeResult() {
        string path = "Assets/Result/Debug.txt";
        StreamWriter sw = new StreamWriter(path, false);

        sw.WriteLine(string.Format("\tsamples = {0}", currentAudio.clip.samples));
        sw.WriteLine(string.Format("\tchannels = {0}", currentAudio.clip.channels));
        sw.WriteLine(string.Format("\ttotalsamples = {0}", currentAudio.clip.samples * currentAudio.clip.channels));
        sw.WriteLine(string.Format("\tsamples per point = {0}", SAMPLES_PER_POINT));
        sw.WriteLine(string.Format("\tmeterScale = {0}", SCALE));
        sw.WriteLine(string.Format("\ttolerance = {0}", TOLERANCE));
        sw.WriteLine(string.Format("\tSongLength = {0}", currentAudio.clip.length));
        sw.WriteLine(string.Format("\tBPM detected = {0}", 
            (currentSong.pointsInSong.Where(x => x.isBeat == true).ToArray().Length)
            /(currentAudio.clip.length / 60f)));

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
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.ImportAsset(path);
        #endif
    }

    private void initializeSong() {
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
        currentSong.pointsInSong.Capacity = (int)(samples.Length / (float)SAMPLES_PER_POINT + 1);
        int indexCounter = 0;
        for (int i = 0; i < samples.Length; i += SAMPLES_PER_POINT)
        {
            Point point = new Point();
            point.index = indexCounter++;
            point.startFromSample = i;
            //if i + 1023 <= samples.length - 1, means it would get out of the bound
            if (i + SAMPLES_PER_POINT <= samples.Length)
                point.finishAtSample = i + SAMPLES_PER_POINT - 1;
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
            point.energy = Mathf.Abs((energySum / SAMPLES_PER_POINT) * SCALE);
            point.position = new Vector2(point.timeInSong, point.energy);
            currentSong.pointsInSong.Add(point);
        }
    }
}
