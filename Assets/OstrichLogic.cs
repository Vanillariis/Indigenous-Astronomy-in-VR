using UnityEngine;

public class OstrichLogic : MonoBehaviour
{
    public Transform StartPoint;
    public Transform EndPoint;

    public float Speed;
    public float FastSpeed;
    public float SlowSpeed;


    public GameObject Player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = StartPoint.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Looks at the x axis may be change to z
        if (Vector3.Distance(new Vector3(transform.position.x, transform.position.y, transform.position.z), new Vector3(transform.position.x, transform.position.y, Player.transform.position.z)) < 80)
        {
            if (Speed > SlowSpeed)
            {
                Speed -= Time.deltaTime / 2;
            }
            else
            {
                Speed = SlowSpeed;
            }
        }
        else
        {
            if (Speed < FastSpeed)
            {
                Speed += Time.deltaTime / 2;
            }
            else
            {
                Speed = FastSpeed;
            }
        }
    }
    private void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, EndPoint.position) < .01)
        {
            transform.position = StartPoint.position;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, EndPoint.position, Speed);
        }
    }
}
