using UnityEngine;

public class Feather : MonoBehaviour
{
    public GameObject Target;

    public float Speed;


    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, Target.transform.position, Speed);
    }
}
