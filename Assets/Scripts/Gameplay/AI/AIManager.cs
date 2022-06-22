using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; } = null;

    public List<TestNavMeshAI> RegisteredAI { get; private set; } = new List<TestNavMeshAI>();

    private int TotalAI = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Attempted second instance of SmartObjectManager");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Register(TestNavMeshAI toRegister)
    {
        if (!RegisteredAI.Contains(toRegister))
        {
            RegisteredAI.Add(toRegister);
        }
        toRegister.ID = TotalAI;
        TotalAI++;
        Debug.Log(RegisteredAI.Count);
    }

    public void Deregister(TestNavMeshAI toDeregister)
    {
        if (RegisteredAI.Contains(toDeregister))
        {
            RegisteredAI.Remove(toDeregister);
        }
    }

    public TestNavMeshAI FindID(int ID)
    {
        foreach(TestNavMeshAI x in RegisteredAI)
        {
            if(x.ID == ID)
            {
                return x;
            }
        }
        return null;
    }
}
