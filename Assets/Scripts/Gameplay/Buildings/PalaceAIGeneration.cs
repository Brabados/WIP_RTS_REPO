using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PalaceAIGeneration : MonoBehaviour
{
    public GameObject TaxCollector;
    public GameObject Entrance;

    void Start()
    {
        for (int x = 0; x < 1; x++)
        {
            Instantiate(TaxCollector, Entrance.transform.position, Quaternion.identity);
        }
    }
}
