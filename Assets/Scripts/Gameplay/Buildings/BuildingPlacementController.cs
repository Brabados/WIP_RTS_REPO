using UnityEngine;


public class BuildingPlacementController : MonoBehaviour
{

    [SerializeField]
    private GameObject placeableObjectPrefab;
    [SerializeField]
    private Material isPlaceable;
    [SerializeField]
    private Material isNotPlaceable;
    [SerializeField]
    private Material materialWhenPlaced;
    [SerializeField]
    private double RotationScale = 10;
    private float mousedirection = 0;

    public GameObject uiBranch;


    [SerializeField]
    private GameObject currentPlaceableObject;

    [SerializeField]
    private GameObjectEvent OnChangeTerrain;

    [SerializeField]
    private VoidEvent OnBuild;


    private float mouseWheelRotation;

    public bool toggle = false;



    public void Start()
    {
        materialWhenPlaced = placeableObjectPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
       
    }

    // Update is called once per frame
    private void Update()
    {
        
        // HandleNewObjectHotKey();
        HandleCancelObject();
        if (currentPlaceableObject != null)
        {
             if (currentPlaceableObject.transform.localRotation.x != 0 || currentPlaceableObject.transform.localRotation.z != 0)
              {
                currentPlaceableObject.GetComponentInChildren<Renderer>().sharedMaterial = isNotPlaceable;
              }
            
            MoveCurrentPlaceableObjectToMouse();
            
            if(mousedirection != Input.mouseScrollDelta.y)
            RotateFromMouseWheel();
            mousedirection = Input.mouseScrollDelta.y;

            ReleaseIfClicked();

        }
    }

    public void SetPlaceable(bool set)
    {
        if (currentPlaceableObject != null)
        {
            if (!set)
            {
                currentPlaceableObject.GetComponentInChildren<Renderer>().sharedMaterial = isNotPlaceable;
            }
            else
            {
                currentPlaceableObject.GetComponentInChildren<Renderer>().sharedMaterial = isPlaceable;
            }
        }
    }

    private void ReleaseIfClicked()
    {

        if (currentPlaceableObject.GetComponentInChildren<Renderer>().sharedMaterial == isPlaceable)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnChangeTerrain.Raise(currentPlaceableObject);// runs the function for modifying terrain
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                OnBuild.Raise();

                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    Vector3 LocalRot = currentPlaceableObject.transform.rotation.eulerAngles;
                    currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(UnityEngine.Vector3.up, hitInfo.normal);
                    Vector3 ChangedRot = currentPlaceableObject.transform.rotation.eulerAngles;
                    ChangedRot[1] = LocalRot[1];
                    currentPlaceableObject.transform.rotation = Quaternion.Euler(ChangedRot[0], ChangedRot[1], ChangedRot[2]);
                }
                currentPlaceableObject.GetComponentInChildren<Renderer>().sharedMaterial = materialWhenPlaced;
                currentPlaceableObject.GetComponent<InstantiateMaterials>().placed = true;
                currentPlaceableObject = null;
                
                CloseBranch();

            }
        }
        else if (currentPlaceableObject.GetComponentInChildren<Renderer>().sharedMaterial == isNotPlaceable)
        {
            print("This object cannot be placed here");
        }
    }

    private void RotateFromMouseWheel()
    {
        mouseWheelRotation = Input.mouseScrollDelta.y * (float)RotationScale;
        currentPlaceableObject.transform.Rotate(UnityEngine.Vector3.up, mouseWheelRotation);
    }
    private void MoveCurrentPlaceableObjectToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {

            //Vector3 roundUp = new Vector3(Mathf.RoundToInt(hitInfo.point.x), Mathf.RoundToInt(hitInfo.point.y), Mathf.RoundToInt(hitInfo.point.z));
            currentPlaceableObject.transform.position = hitInfo.point;

            // the object will rotate to fit if the ground is uneven
            // commented out to avoid buildings being placed on thier edges, might modify later for allowing slight incline to placement
           // currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        }

    }

    public void HandleNewObjectHotKey()
    {
        if (toggle == false)
        {
            toggle = true;
            if (currentPlaceableObject == null)
            {
                currentPlaceableObject = Instantiate(placeableObjectPrefab);
                currentPlaceableObject.GetComponentInChildren<Renderer>().sharedMaterial = isPlaceable;
            }
            else
            {
                Destroy(currentPlaceableObject);
            }
        }
        else
        {
            toggle = false;

            Destroy(currentPlaceableObject);

        }




    }
    private void HandleCancelObject()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentPlaceableObject != null)
            {
                Destroy(currentPlaceableObject);
                CloseBranch();
            }
        }
    }

    private void CloseBranch()
    {
        toggle = false;
        uiBranch.SetActive(false);

    }
}

