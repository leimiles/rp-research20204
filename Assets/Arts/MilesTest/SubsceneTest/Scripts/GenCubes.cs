using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenCubes : MonoBehaviour
{
    [SerializeField]
    public GameObject cube;
    [SerializeField]
    [Range(1, 500)]
    public int radius = 5;
    [Range(10, 50000)]
    public int count = 5;

}
