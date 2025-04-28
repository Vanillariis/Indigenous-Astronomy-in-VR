using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;

public class PullGestureDetector : MonoBehaviour
{
    public HandRef leftHand;
    public HandRef rightHand;
    public float minDistanceChange = 0.1f;

    private Vector3 lastFrameLeftPos;
    private Vector3 lastFrameRightPos;
    private bool wasPinching = false;

    void Update()
    {
        bool leftPinching = leftHand.GetFingerIsPinching(HandFinger.Index);
        bool rightPinching = rightHand.GetFingerIsPinching(HandFinger.Index);

        if (leftPinching && rightPinching)
        {
            float leftHandMovement = Vector3.Distance(leftHand.transform.position, lastFrameLeftPos);
            float rightHandMovement = Vector3.Distance(rightHand.transform.position, lastFrameRightPos);

            if (wasPinching)
            {
                if (leftHandMovement > minDistanceChange || rightHandMovement > minDistanceChange)
                {
                    Debug.Log("Pull gesture detected!");
                }
            }

            wasPinching = true;
        }
        else
        {
            wasPinching = false;
        }

        lastFrameLeftPos = leftHand.transform.position;
        lastFrameRightPos = rightHand.transform.position;
    }
}
