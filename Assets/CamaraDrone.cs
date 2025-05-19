using UnityEngine;

public class CamaraDrone : MonoBehaviour
{
    public float Speed;

    public GameObject StartPoint;
    public GameObject EndPoint;

    public Quaternion RotationTarget;

    public float WaitTimer;

    void Start()
    {
        transform.position = StartPoint.transform.position;
    }

    // Update is called once per frame
    void Update()
    {


        if (Vector3.Distance(transform.position, EndPoint.transform.position) < .1)
        {
            if (WaitTimer > 2)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, RotationTarget, Speed /2);
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
}
