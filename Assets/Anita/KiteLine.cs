using System;
using UnityEngine;

public class KiteLine : MonoBehaviour
{
    public LineRenderer lineR;
    public int segments = 10;
    public Vector3[] points;
    
    public Transform ground;
    public Transform kite;

    public float slackFactor;
    public float maxLineLength;
    
    private void Start()
    {
        lineR.positionCount = segments;
        points = new Vector3[segments];

    }

    private void Update()
    {
        points[0] = ground.position; // Start point
        points[segments - 1] = kite.position; // End point

        Vector3 start = ground.position;
        Vector3 end = kite.position;

        // Calculate the total length and direction of the line
        Vector3 direction = end - start;
        float length = direction.magnitude;

        for (int i = 1; i < segments - 1; i++)
        {
            float t = (float)i / (segments - 1); // Normalized position (0 to 1)

            // Interpolate between start and end points
            Vector3 point = Vector3.Lerp(start, end, t);
            // Add slack using a parabolic sag
            float sagAmount = Mathf.Sin(Mathf.PI * t) * slackFactor; // Slack curve
            Vector3 sagOffset = Vector3.down * sagAmount; // Apply sag in the downward direction

            // Offset the point to create the slack
            points[i] = point + sagOffset;
        }
        
        // Update the LineRenderer
        lineR.SetPositions(points);

    }
}
