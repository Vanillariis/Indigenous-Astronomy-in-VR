using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class OstrichLogic : MonoBehaviour
{
    public Transform StartPoint;
    public Transform EndPoint;

    public float Speed;
    public float SpeedMultiplaier;
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
        float distance = Vector3.Distance(new Vector3(0, 0, transform.position.z), new Vector3(0, 0, Player.transform.position.z));

        Speed = Mathf.Clamp(distance * SpeedMultiplaier, SlowSpeed, FastSpeed);
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
