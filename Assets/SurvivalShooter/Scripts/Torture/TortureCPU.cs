using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Stopwatch = System.Diagnostics.Stopwatch;

public class TortureCPU : MonoBehaviour
{
    private Vector3[] uselessNumbers = new Vector3[1280];

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < uselessNumbers.Length; i++)
        {
            uselessNumbers[i] = Random.onUnitSphere;
        }
    }

    void PrintVectors()
    {
        for (int i = 0; i < uselessNumbers.Length; i++)
        {
            string concat = "MYVector" + uselessNumbers[i].ToString();
            Debug.Log(concat);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // to be used with the profiler
        PrintVectors();

        // to be used with observation in the Console/Log
        Stopwatch concatStopwatch = Stopwatch.StartNew();
        concatStopwatch.Start();
        for (int i = 0; i < uselessNumbers.Length; i++)
        {
            string concat = "MYVector" + uselessNumbers[i].ToString();
        }
        concatStopwatch.Stop();

        Stopwatch interpStopwatch = Stopwatch.StartNew();
        interpStopwatch.Start();
        for (int i = 0; i < uselessNumbers.Length; i++)
        {
            string interp = $"MYVector{uselessNumbers[i]}";
        }
        interpStopwatch.Stop();

        UnityEngine.Debug.LogWarning("\n" +
                                     "CC: " + concatStopwatch.ElapsedMilliseconds + "ms" + "\n" +
                                     "IN: " + interpStopwatch.ElapsedMilliseconds + "ms");
    }
}
