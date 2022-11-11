using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class PlayingLauncher:MonoBehaviour
{
    
    
    void Start()
    {

        StartCoroutine(starttt(0));

    }


    IEnumerator starttt(float sec)
    {
        yield return new WaitForSeconds(sec);
        string pythonPath = @"C:\Users\HP\Desktop\launcher.bat";
        Process proc = Process.Start(pythonPath);
    }
}
