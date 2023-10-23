using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPhys_Cam : MonoBehaviour
{
    [SerializeField] private Transform cameraPos;

    void Update()
    {
        transform.position = cameraPos.position;
    }
}
