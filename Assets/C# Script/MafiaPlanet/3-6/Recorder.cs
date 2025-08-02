using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    [SerializeField] private bool isRecording = false;

    public void RecorderClicked()
    {
        isRecording = !isRecording;
    }

    public bool BoolCheck()
    {
        return isRecording;
    }
}
