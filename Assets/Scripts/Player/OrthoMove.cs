using UnityEngine;
using System.Collections;

public class OrthoMove : MonoBehaviour {

    public float moveSpeed = 1f;
    public float rigidbodyForce = 210f;



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
    void FixedUpdate()
    {

        var rb = this.rigidbody;


        if (rb != null)
            rb.velocity *= 0.8f;

        if (Input.GetKey("w"))
        {
            if (rb != null)
                rb.AddForce((moveSpeed * rigidbodyForce) * transform.forward);
            else
                transform.position += moveSpeed * transform.forward;
        }

        if (Input.GetKey("s"))
        {
            if (rb != null)
                rb.AddForce((-moveSpeed * rigidbodyForce) * transform.forward);
            else
                transform.position += -moveSpeed * transform.forward;
        }

        if (Input.GetKey("d"))
        {
            if (rb != null)
                rb.AddForce((moveSpeed * rigidbodyForce) * transform.right);
            else
                transform.position += moveSpeed * transform.right;
        }

        if (Input.GetKey("a"))
        {
            if (rb != null)
                rb.AddForce((-moveSpeed * rigidbodyForce) * transform.right);
            else
                transform.position += -moveSpeed * transform.right;
        }
    }
}
