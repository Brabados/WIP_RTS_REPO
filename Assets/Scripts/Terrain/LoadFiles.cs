using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.UI;




public class LoadFiles: MonoBehaviour
{
    Terrain terr;
    public Dropdown ToLoad;
    public static string[] _Files;

    private void Start()
    {
        _Files = Directory.GetFiles($"{Application.persistentDataPath}/save/", "*.txt");
        List<string> files = new List<string>();
        foreach(string x in _Files)
        {
            files.Add(x);
        }
        ToLoad.AddOptions(files);
       
    }

    // Start is called before the first frame update
    public void Load()
    {
        terr = Terrain.activeTerrain;
        
        string path = $"{Application.persistentDataPath}/save/{ToLoad.value.ToString()}";
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
