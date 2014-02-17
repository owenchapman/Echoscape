using UnityEngine;
using System.Collections;

public class MouseGuide : MonoBehaviour {


    private Camera cam;
    public GameObject fpsTarget;
    public float camPos;
    public GameObject playerCam;
    public GameObject orthoTarget;
    private WindowManager guiManager;
    // Use this for initialization
	void Start () {

        cam = this.GetComponentInChildren<Camera>();
        camPos = 0f;
        guiManager = GameObject.FindWithTag("GUIManager").GetComponent<WindowManager>();

	
	}
	
	// Update is called once per frame
	void FixedUpdate () {


        if(guiManager.userState == UserState.guiInteract || guiManager.userState == UserState.freeze)
            return;

        if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift))
        {

            var ray = cam.ScreenPointToRay(Input.mousePosition);
            var hitInfo = new RaycastHit();

            var hit = Physics.Raycast(ray, out hitInfo, 1000f, 1 << 8);

            if (hit)
            {

                var tmpVec = hitInfo.point - this.transform.position;
                tmpVec.Normalize();
                //tmpVec.y = this.transform.position.y;

                this.rigidbody.AddForce(200f * tmpVec);
                this.rigidbody.velocity *= 0.7f;
            }

        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
            this.rigidbody.velocity *= 0.2f;

        
        //Lerp between FPS and ortho view....
        camPos += Input.GetAxis("Mouse ScrollWheel") * 0.2f;
        camPos = Mathf.Clamp(camPos, 0f, 1f);

        playerCam.transform.position = Vector3.Lerp(orthoTarget.transform.position, fpsTarget.transform.position, camPos);
        playerCam.transform.rotation = Quaternion.Lerp(orthoTarget.transform.rotation, fpsTarget.transform.rotation, camPos);
	}
}
