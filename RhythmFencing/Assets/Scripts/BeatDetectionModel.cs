using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;

public static class BeatDetectionModel {
    public class Point {
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
            if (head == true) {
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
            } else {
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

    public static List<Point> initializeLineOfTheAudio(AudioSource currentAudio) {
        List<Point> song = new List<Point>();
        //-------------------------manually pop beat information
        //setupManually();
        //------------------------------------------------------
        //automatically beat detection
        float[] samples = new float[currentAudio.clip.samples * currentAudio.clip.channels];
        //get the data into the samples array from start point 0
        currentAudio.clip.GetData(samples, 0);
        //make sure pre set the capacity of the array so that system dont need to allocate 
        //memory dynamically which could cause the speed goes down
        song.Capacity = (int)(samples.Length / (float)UserPref.SAMPLES_PER_POINT + 1);
        int indexCounter = 0;
        for (int i = 0; i < samples.Length; i += UserPref.SAMPLES_PER_POINT) {
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
            for (int j = point.startFromSample; j <= point.finishAtSample; j += currentAudio.clip.channels) {
                int temp = currentAudio.clip.channels - 1;
                float sum = 0;
                while (temp >= 0) {
                    //if we got many channels then plus them 
                    sum += samples[j + temp];
                    temp--;
                }
                energySum += sum / (float)currentAudio.clip.channels;
            }
            point.energy = Mathf.Abs((energySum / UserPref.SAMPLES_PER_POINT) * UserPref.SCALE);
            point.position = new Vector2(point.timeInSong, point.energy);
            song.Add(point);
        }
        return song;
    }
    
    public static void simplifyLine(ref List<Point> song) {
        List<int> indexsToKeep = new List<int>();
        List<Point> pointsToKeep = new List<Point>();
        RamerDouglasPeucker(song, UserPref.TOLERANCE, ref pointsToKeep);
        indexsToKeep.AddRange(pointsToKeep.Select(x => x.index));
        for (int i = 0; i < indexsToKeep.Count; i++)
            song[indexsToKeep[i]].isSimplified = true;
    }

    public static void findUpBeat(ref List<Point> song) {
        Point[] potentialBeats = song.Where(x => x.isSimplified == true).ToArray();
        for (int i = 0; i < potentialBeats.Length; i++){
            if (i == 0) {
                if (potentialBeats[i].energy > potentialBeats[i + 1].energy)
                    song[potentialBeats[i].index].isBeat = true;
            } else if (i == potentialBeats.Length - 1) {
                if (potentialBeats[i].energy > potentialBeats[i - 1].energy)
                    song[potentialBeats[i].index].isBeat = true;
            } else {
                if (potentialBeats[i].energy > potentialBeats[i - 1].energy &&
                    potentialBeats[i].energy > potentialBeats[i + 1].energy)
                    song[potentialBeats[i].index].isBeat = true;
            }
        }
    }

    public static void writeResult(AudioSource currentAudio, List<Point> currentSong) {
        string path = "/sdcard/Download/BeatDetetctionModelResult.txt";

#if UNITY_EDITOR
        path = "Assets/Result/Debug.txt";

#endif
        StreamWriter sw = new StreamWriter(path, true);
        sw.WriteLine(string.Format("\tSong Name = {0}", currentAudio.clip.name));
        sw.WriteLine(string.Format("\tsamples = {0}", currentAudio.clip.samples));
        sw.WriteLine(string.Format("\tchannels = {0}", currentAudio.clip.channels));
        sw.WriteLine(string.Format("\ttotalsamples = {0}", currentAudio.clip.samples * currentAudio.clip.channels));
        sw.WriteLine(string.Format("\tsamples per point = {0}", UserPref.SAMPLES_PER_POINT));
        sw.WriteLine(string.Format("\tmeterScale = {0}", UserPref.SCALE));
        sw.WriteLine(string.Format("\ttolerance = {0}", UserPref.TOLERANCE));
        sw.WriteLine(string.Format("\tSongLength = {0}", currentAudio.clip.length));
        sw.WriteLine(string.Format("\tBPM detected = {0}",
            (currentSong.Where(x => x.isBeat == true).ToArray().Length)
            / (currentAudio.clip.length / 60f)));

        sw.WriteLine("\nUpBeats:({0})", currentSong.Where(x => x.isBeat == true).ToList().Count);
        sw.WriteLine(currentSong[0].printPoint(true));
        foreach (BeatDetectionModel.Point p in currentSong.Where(x => x.isBeat == true).ToList())
        {
            sw.WriteLine("\t" + p.printPoint(false));
        }

        sw.WriteLine("\nBeats:({0})", currentSong.Where(x => x.isSimplified == true).ToList().Count);
        sw.WriteLine(currentSong[0].printPoint(true));
        foreach (BeatDetectionModel.Point p in currentSong.Where(x => x.isSimplified == true).ToList())
        {
            sw.WriteLine("\t" + p.printPoint(false));
        }



        sw.WriteLine(string.Format("\nPoints:{0}", currentSong.Count));
        sw.WriteLine(currentSong[0].printPoint(true));
        foreach (BeatDetectionModel.Point p in currentSong)
        {
            sw.WriteLine(p.printPoint(false));
        }

        sw.Close();
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.ImportAsset(path);
#endif
    }

    private static void RamerDouglasPeucker(List<Point> pointList, float epsilon, ref List<Point> output) {

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

        if (dmax > epsilon) {
            List<Point> recResults1 = new List<Point>();
            List<Point> recResults2 = new List<Point>();
            List<Point> firstLine = pointList.Take(index + 1).ToList();
            List<Point> lastLine = pointList.Skip(index).ToList();
            RamerDouglasPeucker(firstLine, epsilon, ref recResults1);
            RamerDouglasPeucker(lastLine, epsilon, ref recResults2);
            output.AddRange(recResults1.Take(recResults1.Count - 1));
            output.AddRange(recResults2);
            if (output.Count < 2) throw new Exception("Probelm assembling output");
        } else {
            output.Clear();
            output.Add(pointList[0]);
            output.Add(pointList[pointList.Count - 1]);
        }
    }

    private static double PerpendicularDistance(Point pt, Point lineStart, Point lineEnd)
    {
        double dx = lineEnd.timeInSong - lineStart.timeInSong;
        double dy = lineEnd.energy - lineStart.energy;

        double mag = Mathf.Sqrt((float)(dx * dx + dy * dy));
        if (mag > 0.0)
        {
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
}
