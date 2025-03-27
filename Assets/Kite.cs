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
        }

        if (KiteState == KiteState.HoistIn)
        {
            transform.position = Vector3.MoveTowards(transform.position, HoistInPoint.transform.position, Speed);
        }
    }
}
