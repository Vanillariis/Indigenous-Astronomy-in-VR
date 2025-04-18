using UnityEngine;

public class Feather : MonoBehaviour
{
    public GameObject Target;

    public float Speed;

    public Animator Animator;

    public Kite Kite;
    OstrichLogic OL;

    public bool Still;
    public bool PickedUp;

    private void Start()
    {
        Kite = FindAnyObjectByType<Kite>();
        Kite.FeatherAttacheded = false;

        OL = FindAnyObjectByType<OstrichLogic>();
    }

    private void FixedUpdate()
    {
        if (Still == false)
        {
            if (Vector3.Distance(transform.position, Target.transform.position) > .001)
            {
                transform.position = Vector3.MoveTowards(transform.position, Target.transform.position, Speed);
            }
            else
            {
                Animator.SetBool("still", true);
                Still = true;
                OL.LookAtFeather = true;
            }
        }
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, Kite.transform.position) < 3)
        {
            Kite.FeatherPlaced();

            OL.LookAtFeather = false;
            OL.LookAtKite = false;

            Destroy(this.gameObject);
        }

        if (PickedUp == true)
        {
            OL.LookAtFeather = false;
            OL.LookAtKite = true;
        }
    }
}
