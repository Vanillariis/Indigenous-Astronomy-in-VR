using System.Collections.Generic;
using UnityEngine;

public class RopeBuilder : MonoBehaviour
{
    [Header("Rope Settings")]
    public GameObject segmentPrefab;
    public int segmentCount = 20;

    [Header("Anchors")]
    public Transform groundAnchor;
    public Rigidbody kiteRigidbody;

    List<Rigidbody> bodies = new List<Rigidbody>();

    void Start()
    {
        // previous body to connect to
        Rigidbody prevBody = groundAnchor.GetComponent<Rigidbody>();
        if (prevBody == null)
        {
            // attach a kinematic Rigidbody to the anchor if it doesn't have one
            prevBody = groundAnchor.gameObject.AddComponent<Rigidbody>();
            prevBody.isKinematic = true;
        }

        // build segments
        for (int i = 0; i < segmentCount; i++)
        {
            // interpolate position between ground & kite
            float t = (i + 1f) / (segmentCount + 1f);
            Vector3 spawnPos = Vector3.Lerp(groundAnchor.position, kiteRigidbody.position, t);

            // instantiate
            GameObject seg = Instantiate(segmentPrefab, spawnPos, Quaternion.identity, transform);
            Rigidbody segBody = seg.GetComponent<Rigidbody>();
            bodies.Add(segBody);

            // orient it to look at the previous body
            seg.transform.LookAt(prevBody.position);

            //Disable collisions between this segment and its parent
            Collider segCollider = seg.GetComponent<Collider>();
            Collider prevCollider = prevBody.GetComponent<Collider>();

            if (segCollider != null && prevCollider != null)
                Physics.IgnoreCollision(segCollider, prevCollider);

            // connect via a ConfigurableJoint (more flexible than HingeJoint)
            var joint = seg.AddComponent<ConfigurableJoint>();
            joint.connectedBody = prevBody;
            joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Limited;
            SoftJointLimitSpring spring = new SoftJointLimitSpring { spring = 20f, damper = 10f };
            joint.angularXLimitSpring = joint.angularYZLimitSpring = spring;
            joint.lowAngularXLimit = new SoftJointLimit { limit = -30f };
            joint.highAngularXLimit = new SoftJointLimit { limit = 30f };
            joint.angularYLimit = new SoftJointLimit { limit = 30f };
            joint.angularZLimit = new SoftJointLimit { limit = 30f };

            prevBody = segBody;
        }

        // finally, connect the last segment to the kite
        var lastJoint = prevBody.gameObject.AddComponent<ConfigurableJoint>();
        lastJoint.connectedBody = kiteRigidbody;
        lastJoint.xMotion = lastJoint.yMotion = lastJoint.zMotion = ConfigurableJointMotion.Locked;
        lastJoint.angularXMotion = lastJoint.angularYMotion = lastJoint.angularZMotion = ConfigurableJointMotion.Limited;
        lastJoint.angularXLimitSpring = lastJoint.angularYZLimitSpring = new SoftJointLimitSpring { spring = 20f, damper = 10f };
        lastJoint.lowAngularXLimit = new SoftJointLimit { limit = -30f };
        lastJoint.highAngularXLimit = new SoftJointLimit { limit = 30f };
        lastJoint.angularYLimit = new SoftJointLimit { limit = 30f };
        lastJoint.angularZLimit = new SoftJointLimit { limit = 30f };


        // Now turn on physics for all segments
        foreach (var rb in bodies)
        {
            rb.isKinematic = false;
        }
    }
}
