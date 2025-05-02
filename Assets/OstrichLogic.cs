using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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
    public int FeatherCast;


    [Header("Spawn")]
    public float WaitToFlyTimerMax;
    private float WaitToFlyTimer;
    public GameObject MoveUpTo;
    public bool ReadyToFlyUp;
    private bool ReadyToFly;
    public float MoveUpSpeed;
    public bool FeatherHasBeenAttached;


    [Header("Gods")]
    public TwoBoneIKConstraint LightGodHandIK;
    public TwoBoneIKConstraint DarkGodHandIK;

    public bool LookAtFeather;
    public bool LookAtKite;
    public bool LookAtBird;

    public List<MultiAimConstraint> LightGodHeadIk;
    public MultiAimConstraint LightGodHeadLookAtNowIk;

    public List<MultiAimConstraint> DarkGodHeadIk;
    public MultiAimConstraint DarkGodHeadLookAtNowIk;

    public AudioSource LightGod_AudioSource;
    public AudioSource DarkGod_AudioSource;

    [Header("End Scene")]
    public Kite Kite;
    public VoiceOver VoiceOver;
    public bool ReadyForEnd;


    private void Start()
    {
        CastFeatherOnce = true;

        Ani = GetComponentInChildren<Animator>();

        //FeatherHasBeenAttached = true;
    }


    // Update is called once per frame
    void Update()
    {
        // Looks at the z axis may be change to x
        float distance = Vector3.Distance(new Vector3(0, 0, transform.position.z), new Vector3(0, 0, Player.transform.position.z));

        Speed = Mathf.Clamp(distance * SpeedMultiplaier, SlowSpeed, FastSpeed);


        // Feather
        if (CastFeatherOnce == false && FeatherCast < 4)
        {
            if (Vector3.Distance(new Vector3(0, 0, transform.position.z), new Vector3(0, 0, Player.transform.position.z)) < 1)
            {
                Feather _feather = Instantiate(FeatherPrefab, transform.position, transform.rotation).GetComponent<Feather>();

                _feather.Target = FeatherTarget;

                CastFeatherOnce = true;

                FeatherHasBeenAttached = false;

                FeatherCast += 1;
            }
        }

        // IK heads
        if (LookAtBird == true)
        {
            LightGodHeadLookAtNowIk = LightGodHeadIk[1];
            DarkGodHeadLookAtNowIk = DarkGodHeadIk[1];
        }
        else if (LookAtFeather == true)
        {
            LightGodHeadLookAtNowIk = LightGodHeadIk[2];
            DarkGodHeadLookAtNowIk = DarkGodHeadIk[2];
        }
        else if (LookAtKite == true)
        {
            LightGodHeadLookAtNowIk = LightGodHeadIk[3];
            DarkGodHeadLookAtNowIk = DarkGodHeadIk[3];
        }
        else
        {
            if (Vector3.Distance(transform.position, Player.transform.position) < 50)
            {
                LightGodHeadLookAtNowIk = LightGodHeadIk[1];
                DarkGodHeadLookAtNowIk = DarkGodHeadIk[1];
            }
            else
            {
                LightGodHeadLookAtNowIk = null;
                DarkGodHeadLookAtNowIk = null;
            }
        }

        ChancheHeadIK();
    }

    private void FixedUpdate()
    {
        if (ReadyToFly == true)
        {
            // Reset Pos
            if (Vector3.Distance(transform.position, EndPoint.position) < .01)
            {
                if (FeatherHasBeenAttached == true)
                {
                    transform.position = StartPoint.position;
                    CastFeatherOnce = false;
                    VoiceOver.Done = false;

                    if (Kite.EndScene == true)
                    {
                        ReadyForEnd = true;
                    }
                }
            }
            else
            {
                if (ReadyForEnd == true)
                {
                    transform.position = Vector3.MoveTowards(transform.position, Kite.transform.position, Speed);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, EndPoint.position, Speed);
                }
            }
        }
        else
        {
            if (ReadyToFlyUp == true)
            {
                if (Ani.GetCurrentAnimatorStateInfo(0).IsName("Armature|TakeOff"))
                {
                    if (WaitToFlyTimer > WaitToFlyTimerMax)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, new Vector3(EndPoint.position.x, EndPoint.position.y * 2.5f, EndPoint.position.z), Speed);
                    }
                    else
                    {
                        WaitToFlyTimer += Time.deltaTime;
                    }
                }

                if (Ani.GetCurrentAnimatorStateInfo(0).IsName("Armature|Flying"))
                {
                    ReadyToFlyUp = false;
                    ReadyToFly = true;
                    Debug.Log("flying normal");
                }
            }
            else
            {
                if (Vector3.Distance(new Vector3(0, transform.position.y, 0), new Vector3(0, MoveUpTo.transform.position.y, 0)) < .01)
                {
                    LightGodHandIK.weight -= Time.deltaTime / 3;
                    DarkGodHandIK.weight -= Time.deltaTime / 3;

                    if (WaitToFlyTimer > WaitToFlyTimerMax - .5)
                    {
                        Ani.SetBool("Fly", true);
                        ReadyToFlyUp = true;
                    }
                    else
                    {
                        WaitToFlyTimer += Time.deltaTime;
                    }
                }
                else
                {
                    LightGodHandIK.weight += Time.deltaTime / 3;
                    DarkGodHandIK.weight += Time.deltaTime / 3;

                    transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, MoveUpTo.transform.position.y, transform.position.z), MoveUpSpeed);
                }
            }
        }
    }

    public void ChancheHeadIK()
    {
        foreach (var item in LightGodHeadIk)
        {
            if (item != LightGodHeadLookAtNowIk)
            {
                item.weight -= Time.deltaTime / 3;
            }
            else
            {
                item.weight += Time.deltaTime / 3;
            }
        }

        foreach (var item in DarkGodHeadIk)
        {
            if (item != DarkGodHeadLookAtNowIk)
            {
                item.weight -= Time.deltaTime / 3;
            }
            else
            {
                item.weight += Time.deltaTime / 3;
            }
        }
    }
}
