using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum KiteState { HoistIn,  HoistOut }

public class Kite : MonoBehaviour
{
    public KiteState KiteState;

    public Vector3 StartPos;
    public Vector3 PositionTo;
    public float Speed;

    public GameObject HoistInPoint;

    public List<GameObject> FeathersPlaceHolders;
    public int FeatherAttacheds;
    public bool FeatherAttacheded;

    public KiteLine KiteLine; 

    [Range(0f, 100f)]
    public int Happiness;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        PositionTo = new Vector3(StartPos.x + ((100 - Happiness) * 0.1f), StartPos.y - ((100 - Happiness) * 0.1f), StartPos.z - ((100 - Happiness) * 0.1f));

        if (KiteState == KiteState.HoistOut)
        {
           
            transform.position = Vector3.MoveTowards(transform.position, PositionTo, Speed);

            if (FeatherAttacheded == true)
            {
                FindAnyObjectByType<OstrichLogic>().FeatherHasBeenAttached = true;
            }
        }

        if (KiteState == KiteState.HoistIn)
        {
            transform.position = Vector3.MoveTowards(transform.position, HoistInPoint.transform.position, Speed);
        }
    }

    public void FeatherPlaced()
    {
        FeathersPlaceHolders[FeatherAttacheds].SetActive(true);

        FeatherAttacheds += 1;

        FeatherAttacheded = true;
        

        if (FeatherAttacheds == 1)
        {
            Happiness = 100;
        }

        if (FeatherAttacheds == 2)
        {
            Happiness = 70;
        }

        if (FeatherAttacheds == 3)
        {
            Happiness = 40;
        }

        if (FeatherAttacheds == 4)
        {
            Happiness = 100;
        }
    }

}
