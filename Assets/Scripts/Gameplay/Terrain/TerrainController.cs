using System.Collections.Generic;
using UnityEngine;
using Accord.Math;

public class TerrainController : MonoBehaviour
{
    Terrain terr; // terrain to modify
    TerrainData Save;
    int hmWidth; // heightmap width
    int hmHeight; // heightmap height

    public int XrotDegree;
    public int YrotDegree;
    public int ZrotDegree;

    private float XrotRadian;
    private float YrotRadian;
    private float ZrotRadian;

    int posXInTerrain; // position of the game object in terrain width (x axis)
    int posYInTerrain; // position of the game object in terrain height (z axis)

    int size = 0; // the diameter of terrain portion that will raise under the game object

    List<TreeInstance> TreeInstances = new List<TreeInstance>(); // list to hold a reffrence of all trees
    public TerrainVisualizer _Visualizer;

    public UnityEngine.Vector3 lastHit;

    float[,] Xrotation;
    float[,] Yrotation;
    float[,] Zrotation;

public void Start()
    {
        terr = Terrain.activeTerrain;
        hmWidth = terr.terrainData.heightmapResolution;
        hmHeight = terr.terrainData.heightmapResolution;
        TreeInstances = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treeInstances); //Gets the list of trees from the main terraindata and stores it for later use
        _Visualizer = gameObject.GetComponent<TerrainVisualizer>();
    }

    public void Update()
    {
        XrotRadian = XrotDegree * Mathf.PI / 180;
        YrotRadian = YrotDegree * Mathf.PI / 180;
        ZrotRadian = ZrotDegree * Mathf.PI / 180;
        Xrotation = new float[3, 3] { { 1, 0, 0 }, { 0, Mathf.Cos(XrotRadian), -Mathf.Sin(XrotRadian) }, { 0, Mathf.Sin(XrotRadian), Mathf.Cos(XrotRadian) } };
        Yrotation = new float[3, 3] { { Mathf.Cos(YrotRadian), 0, Mathf.Sin(YrotRadian) }, { 0, 1, 0 }, { -Mathf.Sin(YrotRadian), 0, Mathf.Cos(YrotRadian) } };
        Zrotation = new float[3, 3] { { Mathf.Cos(ZrotRadian), -Mathf.Sin(ZrotRadian), 0 }, { Mathf.Sin(ZrotRadian), Mathf.Cos(ZrotRadian), 0 }, { 0, 0, 1 } };
    }

    //Start of refactoring out code to be used better and more effecently, will recomment when fully refactored
    public float[,] TerrainPoint(UnityEngine.Vector3 CheckPoint)
    {
        if(size < 5)
        { 
            size = 10;
        }

        // get the normalized position of the mouse relative to the terrain
        UnityEngine.Vector3 tempCoord = (CheckPoint - terr.gameObject.transform.position);
        UnityEngine.Vector3 coord;

        coord.x = tempCoord.x / terr.terrainData.size.x;
        coord.y = tempCoord.y / terr.terrainData.size.y;
        coord.z = tempCoord.z / terr.terrainData.size.z;

        // get the position of the terrain heightmap where the mouse is
        posXInTerrain = (int)(coord.x * hmWidth);
        posYInTerrain = (int)(coord.z * hmHeight);

        // set an offset so that all the manipulated terrain is under the mouse
        int offset = size / 2;

        // get the heights of the terrain under the mouse
        float[,] heights = terr.terrainData.GetHeights(posXInTerrain - offset, posYInTerrain - offset, size, size);

        return heights;
    }
    public void PlaceBuilding(GameObject currentPlaceableObject)
    {
        size = (int)currentPlaceableObject.GetComponent<BoxCollider>().size.x; //Sets the diamiter of the offset from placement point
        // get the normalized position of this game object relative to the terrain
        UnityEngine.Vector3 tempCoord = (currentPlaceableObject.transform.position - terr.gameObject.transform.position);
        UnityEngine.Vector3 coord;
        coord.x = tempCoord.x / terr.terrainData.size.x;
        coord.y = tempCoord.y / terr.terrainData.size.y;
        coord.z = tempCoord.z / terr.terrainData.size.z;

        // get the position of the terrain heightmap where this game object is
        posXInTerrain = (int)(coord.x * hmWidth);
        posYInTerrain = (int)(coord.z * hmHeight);

        // set an offset so that all the raising terrain is under this game object
        int offset = size / 2;

        RemoveTrees(currentPlaceableObject.transform.position, offset);

        // get the heights of the terrain under this game object
        float[,] heights = terr.terrainData.GetHeights(posXInTerrain - offset, posYInTerrain - offset, size, size);


        heights = PlaneOfBestFit(heights);

        terr.terrainData.SetHeights(posXInTerrain - offset, posYInTerrain - offset, heights);
 
       
    }

    float[,] PlaneOfBestFit(float[,] calc)
    {
        double[,] fit = SumOfAPlane(calc);

        float[,] ToMod = calc;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                ToMod[i, j] = (float)((i * fit[0, 0]) + (j * fit[1, 0]) + fit[2, 0]);
            }
        }

        double angle = angelof(fit);

        return ToMod;

    }

    public double[,] SumOfAPlane(float[,] calc)
    {
        float[,] ToMod = calc;

        double[,] temp_A = new double[ToMod.Length, 3];
        double[,] temp_B = new double[100, 1];

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

        // double[,] b = Matrix.Transpose(temp_B);
        double[,] A = temp_A;
        double[,] Transpose = Matrix.Dot(Matrix.Transpose(A), A);
        double[,] Psudo = Matrix.PseudoInverse(Transpose);
        double[,] PsudoTranspose = Matrix.Dot(Psudo, Matrix.Transpose(A));
        double[,] fit = Matrix.Dot(PsudoTranspose, temp_B);

        return fit;
    }

    public float angelof( double[,] Plane)
    {
        float thata = 0;

        double[,] UpVector = new double[3,1];
        UpVector[0, 0] = 1;
        UpVector[1, 0] = 0;
        UpVector[2, 0] = 1;

        double magnitudeA = Mathf.Sqrt((float)((Plane[0, 0] * Plane[0, 0]) + (Plane[1, 0] * Plane[1, 0]) + (Plane[2, 0] * Plane[2, 0])));
        double magnitudeB = Mathf.Sqrt((float)((UpVector[0, 0] * UpVector[0, 0]) + (UpVector[1, 0] * UpVector[1, 0]) + (UpVector[2, 0] * UpVector[2, 0])));
        double todiv = Plane[0, 0] * UpVector[0, 0] + Plane[1, 0] * UpVector[1, 0] + Plane[2, 0] * UpVector[2, 0];
        thata = Mathf.Acos((float)(todiv / (magnitudeA * magnitudeB)));


        return thata;


    }

    public void RemoveTrees(UnityEngine.Vector3 coords, int offset)
    {
        List<TreeInstance> RemoveTrees = new List<TreeInstance>();
        for (int i = 0; i < TreeInstances.Count; i++)
        {
            TreeInstance currentTree = TreeInstances[i];
            // The the actual world position of the current tree checking
            UnityEngine.Vector3 currentTreeWorldPosition = UnityEngine.Vector3.Scale(currentTree.position, terr.terrainData.size) + Terrain.activeTerrain.transform.position;

            //Checks world possiton against possiton of each tree in list to find ones to be removed
            if (currentTreeWorldPosition.x < coords.x + offset && currentTreeWorldPosition.x > coords.x - offset 
                && currentTreeWorldPosition.z > coords.z - offset && currentTreeWorldPosition.z < coords.z + offset)
            {
                RemoveTrees.Add(currentTree);
            }
        }

        //Deletes trees
        foreach (TreeInstance x in RemoveTrees)
        {
            //Debug.Log(x.ToString());
            TreeInstances.Remove(x);
        }

        //Updates list of trees
        terr.terrainData.treeInstances = TreeInstances.ToArray();
        RemoveTrees.Clear();
    }

    //beung used to make a visualizer for the terrain manipulation, we probably be used as a neet visual for the hud
    private void OnMouseOver()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit Hitout;
        Physics.Raycast(ray, out Hitout);
        if (lastHit != Hitout.point)
        {
            _Visualizer.Clear();
            float[,] Calc = TerrainPoint(Hitout.point);

            ParticleSystem.EmitParams emmiter = new ParticleSystem.EmitParams();
            emmiter.velocity = new UnityEngine.Vector3(0, 0, 0);
            emmiter.startSize = 0.5f;

            //need to refactor so multiple partical systems can show diffrent information
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    float[] point = new float[] { j, Calc[i, j] * 100, i };

                    float[] xrotated = Matrix.Dot(point, Xrotation);
                    float[] yrotated = Matrix.Dot(xrotated, Yrotation);

                    emmiter.position = new UnityEngine.Vector3(yrotated[0], yrotated[1], yrotated[2]);
                    _Visualizer.Vis(emmiter);
                }
            }
        }
        lastHit = Hitout.point;
        
    }
}
