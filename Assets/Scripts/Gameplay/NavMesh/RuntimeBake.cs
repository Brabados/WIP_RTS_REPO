using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RuntimeBake: MonoBehaviour
{

    public NavMeshSurface[] surfaces;

    // Use this for initialization
    public void Bake()
    {
        for (int i = 0; i < surfaces.Length; i++)
        {
            surfaces[i].BuildNavMesh();
        }
    }

}