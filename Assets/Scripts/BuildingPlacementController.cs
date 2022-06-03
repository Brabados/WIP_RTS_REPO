using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Accord;
using Accord.Math;
using MathNet.Numerics;

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

    public GameObject uiBranch;


    [SerializeField]

    private GameObject currentPlaceableObject;
    private float mouseWheelRotation;

    Terrain terr; // terrain to modify
    TerrainData Save;
    int hmWidth; // heightmap width
    int hmHeight; // heightmap height

    int posXInTerrain; // position of the game object in terrain width (x axis)
    int posYInTerrain; // position of the game object in terrain height (z axis)

    public int size = 0; // the diameter of terrain portion that will raise under the game object
    public float desiredHeight = 0; // the height we want that portion of terrain to be

    List<float> MedianList = new List<float>(); // list of each hight value to find the median value
    List<TreeInstance> TreeInstances = new List<TreeInstance>(); // list to hold a reffrence of all trees

    public bool toggle = false;



    public void Start()
    {
        materialWhenPlaced = placeableObjectPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
        terr = Terrain.activeTerrain;
        hmWidth = terr.terrainData.heightmapResolution;
        hmHeight = terr.terrainData.heightmapResolution;
        TreeInstances = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treeInstances); //Gets the list of trees from the main terraindata and stores it for later use
        
    }

    // Update is called once per frame
    private void Update()
    {
        
        // HandleNewObjectHotKey();
        HandleCancelObject();
        if (currentPlaceableObject != null)
        {
            // if (currentPlaceableObject.transform.localRotation.x != 0 || currentPlaceableObject.transform.localRotation.z != 0)
            //  {
            //    currentPlaceableObject.GetComponentInChildren<Renderer>().sharedMaterial = isNotPlaceable;
            //   }
            size = (int)placeableObjectPrefab.GetComponent<BoxCollider>().size.x; //Sets the diamiter of the offset from placement point
            MoveCurrentPlaceableObjectToMouse();
            RotateFromMouseWheel();
            ReleaseIfClicked();

        }
    }



    private void ReleaseIfClicked()
    {

        if (currentPlaceableObject.GetComponentInChildren<Renderer>().sharedMaterial == isPlaceable)
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding(); // runs the function for modifying terrain
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(UnityEngine.Vector3.up, hitInfo.normal);
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

        mouseWheelRotation += Input.mouseScrollDelta.y;
        currentPlaceableObject.transform.Rotate(UnityEngine.Vector3.up, mouseWheelRotation * 90f);
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


    //Function that modifys terrain. This includeds for the moment trees. May move to seperate funtion for clarity later.

    public void PlaceBuilding()
    {

        // get the normalized position of this game object relative to the terrain
        UnityEngine.Vector3 tempCoord = (currentPlaceableObject.transform.position - terr.gameObject.transform.position);
        UnityEngine.Vector3 coord;
        coord.x = tempCoord.x / terr.terrainData.size.x;
        coord.y = tempCoord.y / terr.terrainData.size.y;
        coord.z = tempCoord.z / terr.terrainData.size.z;
        

        // get the position of the terrain heightmap where this game object is
        posXInTerrain = (int)(coord.x * hmWidth);
        posYInTerrain = (int)(coord.z * hmHeight);

        // we set an offset so that all the raising terrain is under this game object
        int offset = size / 2;

        

        List<TreeInstance> RemoveTrees = new List<TreeInstance>();
        for (int i = 0; i < TreeInstances.Count; i++)
        {
            TreeInstance currentTree = TreeInstances[i];
            // The the actual world position of the current tree checking
            UnityEngine.Vector3 currentTreeWorldPosition = UnityEngine.Vector3.Scale(currentTree.position, terr.terrainData.size) + Terrain.activeTerrain.transform.position;

            if (currentTreeWorldPosition.x < currentPlaceableObject.transform.position.x + offset && currentTreeWorldPosition.x > currentPlaceableObject.transform.position.x - offset && currentTreeWorldPosition.z > currentPlaceableObject.transform.position.z - offset && currentTreeWorldPosition.z < currentPlaceableObject.transform.position.z + offset)
            {
                RemoveTrees.Add(currentTree);
            }
        }

        foreach(TreeInstance x in RemoveTrees)
        {
            Debug.Log(x.ToString());
            TreeInstances.Remove(x);
        }
        terr.terrainData.treeInstances = TreeInstances.ToArray();
        RemoveTrees.Clear();
     
        // get the heights of the terrain under this game object
        float[,] heights = terr.terrainData.GetHeights(posXInTerrain - offset, posYInTerrain - offset, size, size);


        heights = TheLine(heights);

                // set each sample of the terrain in the size to the desired height
      //  var heightScale = 1.0f / terr.terrainData.size.y;
      //  desiredHeight = median(heights) * terr.terrainData.size.y;
      //  for (int i = 0; i < size; i++)
      //     for (int j = 0; j < size; j++)
       //         heights[i, j] = desiredHeight * heightScale;

        // set the new height
        terr.terrainData.SetHeights(posXInTerrain - offset, posYInTerrain - offset, heights);
        MedianList.Clear();

    }

    float[,] TheLine(float[,] calc)
    {
        float[,] ToMod = calc;

        Double[,] temp_A = new double[ToMod.Length, 3];
        Double[,] temp_B = new double[100, 1];

        int rowcount = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                temp_A[rowcount, 0] = i;
                temp_A[rowcount, 1] = j;
                temp_A[rowcount, 2] = 1;
                temp_B[rowcount, 0] = ToMod[i, j];
                rowcount++;
            }
        }

            double[,] b = Matrix.Transpose(temp_B);
            double[,] A = temp_A;
            double[,] Transpose = Matrix.Dot(Matrix.Transpose(A), A);
            double[,] Psudo = Matrix.PseudoInverse(Transpose);
            double[,] PsudoTranspose = Matrix.Dot(Psudo, Matrix.Transpose(A));
            double[,] fit = Matrix.Dot(PsudoTranspose, temp_B);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                ToMod[i, j] = (float)((i * fit[0, 0]) + (j * fit[1, 0]) + fit[2, 0]);
            }
        }

        return ToMod;

    }

    float median(float[,] calc)
    {
        float found = 0;

        foreach (float x in calc)
        {
            MedianList.Add(x);
        }

        MedianList.Sort();

        if (MedianList.Count % 2 == 0)
        {
            found = MedianList[(MedianList.Count / 2)];
        }
        else
        {
            found = MedianList[((MedianList.Count - 1) / 2)];
        }

        return found;
    }
}

