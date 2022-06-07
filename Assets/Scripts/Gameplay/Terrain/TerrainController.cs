using System.Collections.Generic;
using UnityEngine;
using Accord.Math;

public class TerrainController : MonoBehaviour
{
    Terrain terr; // terrain to modify
    TerrainData Save;
    int hmWidth; // heightmap width
    int hmHeight; // heightmap height

    int posXInTerrain; // position of the game object in terrain width (x axis)
    int posYInTerrain; // position of the game object in terrain height (z axis)

    int size = 0; // the diameter of terrain portion that will raise under the game object

    List<TreeInstance> TreeInstances = new List<TreeInstance>(); // list to hold a reffrence of all trees

    public void Start()
    {
        terr = Terrain.activeTerrain;
        hmWidth = terr.terrainData.heightmapResolution;
        hmHeight = terr.terrainData.heightmapResolution;
        TreeInstances = new List<TreeInstance>(Terrain.activeTerrain.terrainData.treeInstances); //Gets the list of trees from the main terraindata and stores it for later use

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

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                ToMod[i, j] = (float)((i * fit[0, 0]) + (j * fit[1, 0]) + fit[2, 0]);
            }
        }

        return ToMod;

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
}
