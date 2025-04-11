using UnityEngine;

public class OstrichLogic : MonoBehaviour
{
    [Header("Move Around")]
    public Transform StartPoint;
    public Transform EndPoint;

    public float Speed;
    public float SpeedMultiplaier;
    public float FastSpeed;
    public float SlowSpeed;

    public GameObject Player;
    private Animator Ani;


    [Header("Feather")]
    public GameObject FeatherTarget;
    public GameObject FeatherPrefab;
    private bool CastFeatherOnce;


    [Header("Spawn")]
    public float WaitToFlyTimerMax;
    private float WaitToFlyTimer;
    public GameObject MoveUpTo;
    private bool ReadyToFly;
    public float MoveUpSpeed;


    private void Start()
    {
        CastFeatherOnce = true;

        Ani = GetComponentInChildren<Animator>();
    }


    // Update is called once per frame
    void Update()
    {
        // Looks at the z axis may be change to x
        float distance = Vector3.Distance(new Vector3(0, 0, transform.position.z), new Vector3(0, 0, Player.transform.position.z));

        Speed = Mathf.Clamp(distance * SpeedMultiplaier, SlowSpeed, FastSpeed);


        // Feather
        if (CastFeatherOnce == false)
        {
            if (Vector3.Distance(new Vector3(0, 0, transform.position.z), new Vector3(0, 0, Player.transform.position.z)) < 1)
            {
                Feather _feather = Instantiate(FeatherPrefab, transform.position, transform.rotation).GetComponent<Feather>();

                _feather.Target = FeatherTarget;

                CastFeatherOnce = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (ReadyToFly == true)
        {
            if (Vector3.Distance(transform.position, EndPoint.position) < .01)
            {
                transform.position = StartPoint.position;
                CastFeatherOnce = false;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, EndPoint.position, Speed);
            }
        }
        else
        {
            if (Vector3.Distance(new Vector3(0, transform.position.y, 0), new Vector3(0, MoveUpTo.transform.position.y, 0)) < .01)
            {
                if (WaitToFlyTimer > WaitToFlyTimerMax)
                {
                    ReadyToFly = true;
                    Ani.SetBool("Fly", true);
                }
                else
                {
                    WaitToFlyTimer += Time.deltaTime;
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, MoveUpTo.transform.position.y, transform.position.z), MoveUpSpeed);
            }
        }
    }
}
