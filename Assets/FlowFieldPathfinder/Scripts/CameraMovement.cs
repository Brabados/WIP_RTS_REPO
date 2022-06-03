using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    public Camera cam;
    public float moveSpeed = 10;
    public float rotateSpeed = 38;
	public float zoom = 3;

	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey("q"))
            transform.RotateAround(Vector3.zero, Vector3.up, rotateSpeed * Time.deltaTime);

        if (Input.GetKey("e"))
            transform.RotateAround(Vector3.zero, Vector3.up, -rotateSpeed * Time.deltaTime);

        if (Input.GetKey("w"))
            transform.position += Vector3.forward * moveSpeed * Time.deltaTime;

        if (Input.GetKey("s"))
            transform.position += Vector3.back * moveSpeed * Time.deltaTime;

        if (Input.GetKey("a"))
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (Input.GetKey("d"))
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            cam.orthographicSize -= zoom;
            cam.transform.position += cam.transform.forward;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            cam.orthographicSize += zoom;
            cam.transform.position -= cam.transform.forward;
        }
	}
}
