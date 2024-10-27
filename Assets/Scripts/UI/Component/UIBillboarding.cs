using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillboarding : MonoBehaviour
{
    private Camera faceCamera;

    private void Awake()
    {
        faceCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = faceCamera.transform.forward;
    }
}
