using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseAI : MonoBehaviour
{
    [SerializeField]
    protected string _Name;

    public int ID;
    [SerializeField] 
    public Transform Destination;
    [SerializeField] 
    public BaseInteraction Task;
    public NavMeshAgent navMeshAgent;
    [SerializeField]
    public float TimeBetweenTasks = 2f;
    public float TimeUntillnewTask = -1;
    public bool isDoingTask = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public abstract void AssignAction(List<SmartObject> toAssign);

    public abstract void OnTaskFinish(BaseInteraction Interation);
}
