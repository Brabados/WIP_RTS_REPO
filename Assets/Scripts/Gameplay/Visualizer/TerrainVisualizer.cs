using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainVisualizer : MonoBehaviour
{
    public Canvas UI;
    public GameObject ParticalsPrefab;

    public GameObject MyPartical_Object;
    public ParticleSystem MyParticalSystem;

    // Start is called before the first frame update
    void Start()
    {
        MyPartical_Object = Instantiate(ParticalsPrefab, UI.transform, true);
        MyParticalSystem = MyPartical_Object.GetComponent<ParticleSystem>();
        MyPartical_Object.transform.localScale = new UnityEngine.Vector3(10, 10, 10);
    }

    private void Update()
    {
        MyPartical_Object.transform.position = UI.transform.position;
    }

    public void Vis(UnityEngine.ParticleSystem.EmitParams ToEmit)
    {
        MyParticalSystem.Emit(ToEmit, 1);
    }

    public void Clear()
    {
        MyParticalSystem.Clear();
    }
}
