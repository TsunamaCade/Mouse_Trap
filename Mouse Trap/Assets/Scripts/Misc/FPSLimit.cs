using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSLimit : MonoBehaviour
{
    public int frameRate = 60;
    
    void Start()
    {
        Application.targetFrameRate = frameRate;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
