using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


public class CamaraDrone : MonoBehaviour
{
    public bool TakePicture;

    public float Speed;

    public bool DoTheDrone;

    public GameObject StartPoint;
    public GameObject EndPoint;

    public Quaternion RotationTarget;

    public float WaitTimer;

    void Start()
    {
        if (DoTheDrone == true)
        {
            transform.position = StartPoint.transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (DoTheDrone == true)
        {
            if (Vector3.Distance(transform.position, EndPoint.transform.position) < .1)
            {
                if (WaitTimer > 2)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, RotationTarget, Speed / 2);
                }
                else
                {
                    WaitTimer += Time.deltaTime;
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, EndPoint.transform.position, Speed);
            }
        }


        
        if (TakePicture == true)
        {
            ScreenCapture.CaptureScreenshot(Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "pooo" + ".png"), 8);
            Debug.Log("Screenshot Captured");

            TakePicture = false;
        }

    }
}
