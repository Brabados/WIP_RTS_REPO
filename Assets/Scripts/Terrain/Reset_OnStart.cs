using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;



public class Reset_OnStart : MonoBehaviour
{
    Terrain terr;
    // Start is called before the first frame update
    void Start()
    {
        terr = Terrain.activeTerrain;
        string path = "ArrayOfZ.txt";
        StreamReader reader = new StreamReader(path);
        int XSize = Convert.ToInt32(reader.ReadLine());
        int YSize = Convert.ToInt32(reader.ReadLine());

        float[,] TerrainData = new float[XSize, YSize];


        for (int i = 0; i < XSize; i++)
        {
            for (int j = 0; j < YSize; j++)
            {
                TerrainData[i, j] = (float)Convert.ToDouble(reader.ReadLine());
            }

        }
        reader.Close();
        terr.terrainData.SetHeights(0, 0, TerrainData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
