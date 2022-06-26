using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartObjectManager : MonoBehaviour
{
    public static SmartObjectManager Instance { get; private set; } = null;

    [SerializeField]
    public List<SmartObject> RegisteredObjects { get; private set; } = new List<SmartObject>();

    

    private void Awake()
    {
        if(Instance != null)
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

    public void Register(SmartObject toRegister)
    {
        if(!RegisteredObjects.Contains(toRegister))
        {
            RegisteredObjects.Add(toRegister);
        }
        Debug.Log("Registered " + toRegister.Name);
    }

    public void Deregister(SmartObject toDeregister)
    {
        if(RegisteredObjects.Contains(toDeregister))
        {
            RegisteredObjects.Remove(toDeregister);
        }
    }

    public void GiveTask(int ToGiveID)
    {
        TestNavMeshAI AI = AIManager.Instance.FindID(ToGiveID);
        AI.AssignAction(RegisteredObjects);
    }

}
