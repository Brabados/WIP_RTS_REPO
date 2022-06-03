using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cameraTransform;

    private float movementSpeed;
    public float movementSpeedNormal;
    public float movementSpeedFast;
    public float movementTime;
    public float rotationAmount;
    public Vector3 zoomAmount;

    public Vector3 newPosition;
    public Quaternion newRotation;
    public Vector3 newZoom;

    public Vector3 dragStartPosition;
    public Vector3 dragCurrentPosition;
    public Vector3 rotateStartPosition;
    public Vector3 rotateCurrentPosition;
    // Start is called before the first frame update
    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;

    }

    // Update is called once per frame
    void Update()
    {
        if (newZoom.y >=325)
        {
            movementSpeed = movementSpeedFast;
        }
        else
        {
            movementSpeed = movementSpeedNormal;
        }
        HandleMouseInput();
        HandleMovementInput();
       
    }

    void HandleMouseInput()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            //multiply the zoom amount by 2 to make zooming on the mouse feel faster
            newZoom += Input.mouseScrollDelta.y * (zoomAmount *2);     
            if (newZoom.y < 50)
            {
                newZoom.y = 50;
            }
            if (newZoom.z > -50)
            {
                newZoom.z = -50;
            }
            if (newZoom.y > 700)
            {
                newZoom.y = 700;
            }
            if (newZoom.z < -700)
            {
                newZoom.z = -700;
            }

        }
        if (Input.GetMouseButtonDown(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry;
            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry;
            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);

                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }
        if (Input.GetMouseButtonDown(2))
        {
            rotateStartPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            rotateCurrentPosition = Input.mousePosition;
            Vector3 difference = rotateStartPosition - rotateCurrentPosition;

            //reset the drag position for the next frame
            rotateStartPosition = rotateCurrentPosition;

            //Negating the value here so that the world spins in the opposite direction of the drag 

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5));
        }
    }
    void HandleMovementInput()
    {


        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += (transform.forward * movementSpeed);

        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += (transform.forward * -movementSpeed);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += (transform.right * movementSpeed);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += (transform.right * -movementSpeed);
        }

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Comma))
        {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Period))
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }
        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.LeftBracket))
        {
            newZoom += zoomAmount;
            if (newZoom.y < 50)
            {
                newZoom.y = 50;
            }
            if (newZoom.z > -50)
            {
                newZoom.z = -50;
            }
            if (newZoom.y > 700)
            {
                newZoom.y = 700;
            }
            if (newZoom.z < -700)
            {
                newZoom.z = -700;
            }

        }
        if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.RightBracket))
        {
            newZoom -= zoomAmount;

            if (newZoom.y < 50)
            {
              newZoom.y = 50;
            }
            if (newZoom.z > -50)
            {
                newZoom.z = -50;
            }
            if (newZoom.y >700)
            {
                newZoom.y = 700;
            }
            if (newZoom.z < -700)
            {
                newZoom.z = -700;
            }

           
        }



        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }
}


