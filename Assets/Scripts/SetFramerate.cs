using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SetFramerate : MonoBehaviour
{
    [SerializeField] int targetFramerate = 60;
    void Start()
    {
        Application.targetFrameRate = targetFramerate;
    }
}
