using UnityEngine;

public class MoonMove : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] public Transform target;
    [SerializeField] public float moveSpeed = 8f; // longer move time
    [SerializeField] public AnimationCurve moveCurve = AnimationCurve.Linear(0, 0, 1, 1); // <-- We'll customize this in Inspector

    [Header("Scale Settings")]
    [SerializeField] public float startScale = 0.1f;
    [SerializeField] public float scaleSpeed = 1f;

    private Vector3 originalScale;
    private Vector3 startPosition;
    private float moveTimer = 0f;
    private bool hasArrived = false;

    void Start()
    {
        originalScale = transform.localScale;
        startPosition = transform.position;
        transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        if (hasArrived || target == null)
            return;

        moveTimer += Time.deltaTime;
        float t = Mathf.Clamp01(moveTimer / moveSpeed); // 0 to 1 over moveDuration seconds
        float easedT = moveCurve.Evaluate(t);

        // Move using easing
        transform.position = Vector3.Lerp(startPosition, target.position, easedT);

        // Scale up gradually
        transform.localScale = Vector3.MoveTowards(transform.localScale, originalScale, scaleSpeed * Time.deltaTime);

        if (t >= 1f && Vector3.Distance(transform.localScale, originalScale) < 0.01f)
        {
            hasArrived = true;
        }
    }
}