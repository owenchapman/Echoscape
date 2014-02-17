using UnityEngine;
using System.Collections;

public class RotateCamera : MonoBehaviour {

public Transform target;
private float horizontalSpeed = 360.0f;
private float verticalSpeed = 120.0f;
private float zoomSpeed = 50f;

private float minVertical = 20.0f;
private float maxVertical = 85.0f;
private float x = 0.0f;
private float y = 0.0f;
private float distance = 0.0f;

void Start()
{
	x = transform.eulerAngles.y;
    y = transform.eulerAngles.x;
    distance = (transform.position - target.position).magnitude;
}

void LateUpdate()
{
	var dt = Time.deltaTime;
	x -= Input.GetAxis("Horizontal") * horizontalSpeed * dt;
	y += Input.GetAxis("Vertical") * verticalSpeed * dt;
    
    var diff = distance - Input.GetAxis("Mouse ScrollWheel") * (zoomSpeed * 100) * dt;

    if (diff > 0)
        distance = diff;

    if (Input.GetKey("r") && distance > 0)
        distance -= zoomSpeed * dt;
    if (Input.GetKey("f"))
        distance += zoomSpeed * dt;

	y = ClampAngle(y, minVertical, maxVertical);
	 
	var rotation = Quaternion.Euler(y, x, 0);
	var position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;
	
	transform.rotation = rotation;
	transform.position = position;
}

static float ClampAngle (float angle, float min, float max) 
{
	if (angle < -360f)
		angle += 360f;
	if (angle > 360f)
		angle -= 360f;
	return Mathf.Clamp (angle, min, max);
}
}
