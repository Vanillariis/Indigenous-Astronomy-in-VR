using UnityEngine;

public class Linerendertest : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    [SerializeField] private LineController line;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        line.SetUpLine(points);
    }
}
