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

    [Range(0f, 100f)]
    public int Happiness;

    [Header("End Scene")]
    public bool EndScene;
    public GameObject EndPoint;
    public OstrichLogic OstrichLogic;
    public KiteLine kiteLine;
    public SkyBoxSequenceController SkyBoxSequenceController;
    public StarBurstRenderer StarBurstRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (EndScene == true && OstrichLogic.ReadyForEnd == true && Vector3.Distance(transform.position, OstrichLogic.transform.position) < 30)
        {
            OstrichLogic.LookAtBird = true;

            OstrichLogic.transform.LookAt(transform, Vector3.up);

            Speed = OstrichLogic.Speed;

            if (Vector3.Distance(transform.position, EndPoint.transform.position) < 0.1)
            {
                SkyBoxSequenceController.Explode();
                StarBurstRenderer.Explode();

                Destroy(OstrichLogic.gameObject);
                Destroy(this.gameObject);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, EndPoint.transform.position, Speed);
                kiteLine.ground = HoistInPoint.transform;
            }
        }
        else
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

        KiteState = KiteState.HoistOut;
    }

}
