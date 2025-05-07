using Oculus.Interaction;
using UnityEngine;

public class Feather : MonoBehaviour
{
    public GameObject Target;

    public float Speed;

    public Animator Animator;

    public Kite Kite;
    OstrichLogic OL;

    public Grabbable grabbable;

    public bool Still;

    private void Start()
    {
        Kite = FindAnyObjectByType<Kite>();
        Kite.FeatherAttacheded = false;

        OL = FindAnyObjectByType<OstrichLogic>();
        grabbable = GetComponentInChildren<Grabbable>();
    }

    private void FixedUpdate()
    {
        if (Still == false)
        {
            if (Vector3.Distance(transform.position, Target.transform.position) > .01)
            {
                Debug.Log(Vector3.Distance(transform.position, Target.transform.position));
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
        if (Vector3.Distance(transform.position, Kite.transform.position) < .5f)
        {
            Kite.FeatherPlaced();

            OL.LookAtFeather = false;
            OL.LookAtKite = false;

            Destroy(this.gameObject);
        }

        if (grabbable.HasBeenGrabed == true)
        {
            Kite.KiteState = KiteState.HoistIn;
            OL.LookAtFeather = false;
            OL.LookAtKite = true;
        }
        else
        {
            OL.LookAtFeather = true;
            OL.LookAtKite = false;
        }
    }
}
