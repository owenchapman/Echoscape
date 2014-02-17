using UnityEngine;
using System.Collections;

public class MouseMove : MonoBehaviour
{

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    public float moveSpeed = 1f;
    public float rigidbodyForce = 210f;

    float rotationY = 0F;
	
	public bool freeze = false;
    public bool clampY = false;

    private WindowManager guiManager;

    void Update()
    {
        if(guiManager.userState == UserState.guiInteract || guiManager.userState == UserState.freeze)
            return;
		
		if (Input.GetMouseButton(0) && !Input.GetKey("1"))
        {
            if (axes == RotationAxes.MouseXAndY)
            {
                float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
            }
            else if (axes == RotationAxes.MouseX)
            {
                transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
            }
            else
            {
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
            }
        }
    }

    void FixedUpdate()
    {
        if(guiManager.userState == UserState.guiInteract || guiManager.userState == UserState.freeze)
            return;
            
		var rb = this.rigidbody;
        var dir = new Vector3();


        if (rb != null)
            rb.velocity *= 0.8f;

        if (Input.GetKey("w"))
        {
            if (rb != null)
                dir = (moveSpeed * rigidbodyForce) * transform.forward;
            else
                transform.position += moveSpeed * transform.forward;
        }

        if (Input.GetKey("s"))
        {
            if (rb != null)
                dir = -(moveSpeed * rigidbodyForce) * transform.forward;
            else
                transform.position += -moveSpeed * transform.forward;
        }

        if (Input.GetKey("d"))
        {
            if (rb != null)
                dir = (moveSpeed * rigidbodyForce) * transform.right;
            else
                transform.position += moveSpeed * transform.right;
        }

        if (Input.GetKey("a"))
        {
            if (rb != null)
                dir = -(moveSpeed * rigidbodyForce) * transform.right;
            else
                transform.position += -moveSpeed * transform.right;
        }

        if(clampY)
            dir.y = 0f;

        rb.AddForce(dir);

        var pos = this.transform.position;
        pos.y = Util.GetTerrainHeightAt(pos) + 0.5f;

        this.transform.position = pos;
    }

    void Start()
    {
        guiManager = GameObject.FindWithTag("GUIManager").GetComponent<WindowManager>();

        // Make the rigid body not change rotation
        if (rigidbody)
            rigidbody.freezeRotation = true;
    }
}