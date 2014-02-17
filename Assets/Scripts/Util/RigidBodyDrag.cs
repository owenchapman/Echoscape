using UnityEngine;
using System.Collections;

public class RigidBodyDrag : MonoBehaviour
{

    public float spring = 50f;
    public float damper = 5f;
    public float drag = 10f;
    public float angularDrag = 5f;
    public float distance = 0.2f;
    public bool attachToCenterOfMass = false;

    private SpringJoint springJoint;
    private GameObject rigidBodyDragger;

    // Use this for initialization
    void Start()
    {
        rigidBodyDragger = new GameObject("Rigidbody dragger");
        rigidBodyDragger.AddComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        // Make sure the user pressed the mouse down
        if (!Input.GetMouseButtonDown(0))
            return;

        var mainCamera = Camera.main;

        // We need to actually hit an object
        var hit = new RaycastHit();

        if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 100f))
            return;
        // We need to hit a rigidbody that is not kinematic
        if (hit.rigidbody.isKinematic)
            return;

        if (!springJoint)
        {
            var body = rigidBodyDragger.GetComponent<Rigidbody>();
            springJoint = rigidBodyDragger.AddComponent<SpringJoint>();
            springJoint.breakForce = 5f;
            //body.isKinematic = true;
        }

        springJoint.transform.position = hit.point;
        
        if (attachToCenterOfMass)
        {
            var anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position;
            anchor = springJoint.transform.InverseTransformPoint(anchor);
            springJoint.anchor = anchor;
        }
        else
        {
            springJoint.anchor = Vector3.zero;
        }

        springJoint.spring = spring;
        springJoint.damper = damper;
        springJoint.maxDistance = distance;
        springJoint.connectedBody = hit.rigidbody;
        //springJoint.breakForce = 5.0;
        StartCoroutine("DragObject", hit.distance);
    }

    IEnumerator DragObject(float distance)
    {
        var oldDrag = springJoint.connectedBody.drag;
        var oldAngularDrag = springJoint.connectedBody.angularDrag;
        springJoint.connectedBody.drag = drag;
        //springJoint.connectedBody.angularDrag = angularDrag;
        var mainCamera = Camera.main;


        if (rigidBodyDragger.GetComponent<SpringJoint>())
        {
            if (springJoint.connectedBody)
            {
                springJoint.connectedBody.drag = oldDrag;
                springJoint.connectedBody.angularDrag = 20f;
                springJoint.connectedBody = null;
            }
        }

        yield return 0;
    }
}
