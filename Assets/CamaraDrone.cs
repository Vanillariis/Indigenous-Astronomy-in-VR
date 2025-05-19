using UnityEngine;

public class CamaraDrone : MonoBehaviour
{
    public int Speed;

    public GameObject StartPoint;
    public GameObject EndPoint;

    public Quaternion RotationTarget;


    void Start()
    {
        transform.position = StartPoint.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        

        if (Vector3.Distance(transform.position, EndPoint.transform.position) < .1)
        {
            Quaternion.RotateTowards(transform.rotation, RotationTarget, Speed);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, EndPoint.transform.position, Speed);
        }


        
    }
}
